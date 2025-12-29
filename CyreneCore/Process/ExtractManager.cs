using System.Threading.Channels;
using CyreneCore.Usm;
using Newtonsoft.Json;

namespace CyreneCore.Process;

public class ExtractManager(CancellationToken ct)
{
    private readonly CancellationToken Ct = ct;

    public async Task ExtractAsync(string inputPath, string outDir, ulong key)
    {
        var inputChannel = Channel.CreateBounded<UsmData>(new BoundedChannelOptions(100)
        {
            SingleWriter = true,
            SingleReader = false
        });
        var outputChannel = Channel.CreateBounded<UsmData>(new BoundedChannelOptions(100)
        {
            SingleWriter = false,
            SingleReader = true
        });

        // Reader
        var readerTask = Task.Run(async () =>
        {
            try
            {
                using var reader = new UsmReader(inputPath);
                foreach (var chunk in reader.IterChunks())
                    await inputChannel.Writer.WriteAsync(chunk, Ct);
            }
            finally
            {
                inputChannel.Writer.Complete();
            }
        }, Ct);

        // Decryptor
        var workerCount = key != 0 ? Environment.ProcessorCount : 1;
        var workers = new Task[workerCount];
        for (var i = 0; i < workerCount; i++)
        {
            workers[i] = Task.Run(async () =>
            {
                var usmCrypter = key != 0 ? new UsmCrypter(key) : null;
                await foreach (var chunk in inputChannel.Reader.ReadAllAsync(Ct))
                {
                    if (usmCrypter != null)
                    {
                        if (chunk.IsVideo)
                            usmCrypter.DecryptVideo(chunk.Data.Span);
                        else
                            usmCrypter.DecryptAudio(chunk.Data.Span);
                    }

                    await outputChannel.Writer.WriteAsync(chunk, Ct);
                }
            }, Ct);
        }

        // Writer
        var writerTask = Task.Run(async () =>
        {
            var fileName = Path.GetFileNameWithoutExtension(inputPath);
            var streams = new Dictionary<string, Stream>();

            try
            {
                // Keep chunk write order
                var nextIndex = 0;
                var chunkCache = new Dictionary<int, UsmData>();
                await foreach (var chunk in outputChannel.Reader.ReadAllAsync(Ct))
                    if (chunk.Index != nextIndex)
                        chunkCache[chunk.Index] = chunk;
                    else
                    {
                        WriteChunk(chunk, streams, outDir, fileName);
                        nextIndex++;

                        while (chunkCache.Remove(nextIndex, out var nextChunk))
                        {
                            WriteChunk(nextChunk, streams, outDir, fileName);
                            nextIndex++;
                        }
                    }
            }
            finally
            {
                foreach (var stream in streams.Values)
                    stream.Dispose();
            }
        }, Ct);

        try
        {
            await readerTask;
            await Task.WhenAll(workers).ContinueWith(_ => outputChannel.Writer.Complete(), Ct);
            await writerTask;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (Ct.IsCancellationRequested && Directory.Exists(outDir))
                Directory.Delete(outDir, true);
        }
    }

    private static void WriteChunk(UsmData chunk, Dictionary<string, Stream> streams, string outDir, string name)
    {
        if (chunk.IsVideo && chunk.ChannelNumber != 0)
            throw new InvalidOperationException("Fatal: Invalid channel for video.");

        var fileName = name + (chunk.IsVideo ? ".dat" : $"_{chunk.ChannelNumber}.adx");
        var path = Path.Combine(outDir, fileName);

        if (!streams.TryGetValue(path, out var stream))
        {
            stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            streams[path] = stream;
        }
        stream.Write(chunk.Data.Span);

        if (chunk.IsVideo)
            File.WriteAllText(Path.Combine(outDir, name + ".json"), JsonConvert.SerializeObject(chunk.Meta));
    }
}
