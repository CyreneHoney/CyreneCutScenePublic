using System.Threading.Channels;
using CyreneCore.Caption;
using CyreneCore.Key;
using CyreneCore.Utils;

namespace CyreneCore.Process;

public record ResultData(string Input, int Index, int Total, ProcessState Status, string? Msg = null);

public class ProcessManager
{
    private readonly CoreConfig Config;
    private readonly List<string> InputPaths;
    private readonly string BaseOutDir;
    private readonly bool NeedConvert;
    private readonly bool MergeAudio;
    private readonly bool MergeCaption;
    private readonly ExtractManager ExtractMgr;
    private readonly ConvertManager ConvertMgr;
    private readonly CancellationToken Ct;

    public ProcessManager(CoreConfig config, CancellationToken ct)
    {
        Config = config;
        InputPaths = [];
        var exists = new HashSet<string>();
        foreach (var inputPath in config.Extract.Input)
        {
            if (File.Exists(inputPath))
            {
                var fileName = Path.GetFileNameWithoutExtension(inputPath);
                if (exists.Add(fileName)) InputPaths.Add(inputPath);
            }
            else if (Directory.Exists(inputPath))
                foreach (var file in Directory.GetFiles(inputPath, "*.usm"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (exists.Add(fileName)) InputPaths.Add(file);
                }
        }
        BaseOutDir = config.Extract.Output;
        MergeAudio = config.Extract.MergeAudio;
        MergeCaption = config.Extract.MergeCaption;
        NeedConvert = MergeAudio || MergeCaption || config.Extract.Convert;

        Ct = ct;
        ExtractMgr = new(ct);
        ConvertMgr = new(config.Path.FFmpegPath, ct);
    }

    public async ValueTask<bool> InitAsync()
    {
        if (!KeyManager.LoadKeyData(Config.Path.KeyPath)) return false;
        if (!CaptionManager.LoadCaptionData(Config.Path.ResPath)) return false;
        if (NeedConvert) await ConvertMgr.InitAsync();

        return true;
    }

    public async IAsyncEnumerable<ResultData> ProcessAsync()
    {
        var index = 0;
        var channel = Channel.CreateUnbounded<ResultData>();

        var task = Task.Run(async () =>
        {
            try
            {
                await Parallel.ForEachAsync(InputPaths, Ct, async (inputPath, ct) =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(inputPath);
                    var outputDir = Path.Combine(BaseOutDir, fileName);
                    if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

                    var ret = new ResultData(inputPath, index, InputPaths.Count, ProcessState.Processing);
                    await channel.Writer.WriteAsync(ret, ct);

                    var key = KeyManager.GetKey(fileName);
                    if (key == null)
                    {
                        await channel.Writer.WriteAsync(ret with { Status = ProcessState.Failed, Msg = "Key not found." }, ct);
                        return;
                    }

                    try
                    {
                        await ExtractMgr.ExtractAsync(inputPath, outputDir, key.Value);
                        CaptionManager.GenCaption(fileName, outputDir);
                        if (NeedConvert) await ConvertMgr.ConvertAsync(outputDir, BaseOutDir, MergeAudio, MergeCaption);

                        Interlocked.Increment(ref index);
                        await channel.Writer.WriteAsync(ret with { Index = index, Status = ProcessState.Completed }, ct);
                    }
                    catch (Exception ex)
                    {
                        Ct.ThrowIfCancellationRequested();

                        await channel.Writer.WriteAsync(ret with { Status = ProcessState.Failed, Msg = ex.Message }, ct);
                    }
                });
            }
            finally
            {
                channel.Writer.Complete();
            }
        }, Ct);
    
        await foreach (var ret in channel.Reader.ReadAllAsync(Ct))
            yield return ret;

        await task;
    }
}
