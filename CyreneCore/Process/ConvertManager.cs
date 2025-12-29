using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;
using CyreneCore.Utils;
using Newtonsoft.Json;
using CyreneCore.Usm;

namespace CyreneCore.Process;

public record MergeTaskContext(
    string VideoPath,
    string MetaPath,
    List<string> AudioPaths,
    List<string> CaptionPaths,
    string OutputDir,
    bool MergeAudio,
    bool MergeCaption
);

public class ConvertManager(string path, CancellationToken ct)
{
    private readonly CancellationToken Ct = ct;

    public async Task InitAsync()
    {
        var ffmpegDir = path;
        var ffmpegExe = Path.Combine(ffmpegDir, "ffmpeg.exe");

        if (!File.Exists(ffmpegExe))
        {
            Logger.Warn("FFmpeg not found, downloading...");
            await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, ffmpegDir);
            Logger.Info("FFmpeg downloaded.");
        }

        FFmpeg.SetExecutablesPath(ffmpegDir);
    }

    private async Task<string> AddVideoTrack(IConversion conv, string path, string meta)
    {
        var info = await FFmpeg.GetMediaInfo(path, Ct);
        var stream = info.VideoStreams.First();
        var data = JsonConvert.DeserializeObject<UsmMetadata>(File.ReadAllText(meta));

        string ret;
        var codec = stream.Codec?.ToLower() ?? "unk";
        if (codec.Contains("mpeg1"))
            ret = "mp4";
        else if (codec.Contains("vp9"))
            ret = "mkv";
        else if (codec.Contains("h264") || codec.Contains("avc"))
        {
            ret = "mkv";
            conv.AddParameter("-f h264", ParameterPosition.PreInput);
        }
        else
            throw new NotSupportedException($"Unsupported video codec: {codec}");

        conv.AddParameter($"-r {data.Framerate / 100.0:F2}", ParameterPosition.PreInput);
        conv.AddParameter("-fflags +genpts", ParameterPosition.PreInput);
        conv.AddStream(stream);
        conv.AddParameter("-c:v copy");

        return ret;
    }

    private async Task AddAudioTracks(IConversion conv, List<string> paths)
    {
        if (paths.Count == 0) return;

        foreach (var path in paths)
        {
            var info = await FFmpeg.GetMediaInfo(path, Ct);
            conv.AddStream(info.AudioStreams);
        }
        conv.AddParameter("-c:a flac -compression_level 5");
    }

    private async Task AddCaptionTracks(IConversion conv, List<string> paths)
    {
        if (paths.Count == 0) return;

        for (int i = 0; i < paths.Count; i++)
        {
            var info = await FFmpeg.GetMediaInfo(paths[i], Ct); // TODO: low performance here
            conv.AddStream(info.SubtitleStreams);

            var lang = Path.GetFileNameWithoutExtension(paths[i]).Split('_').LastOrDefault() ?? "und";
            conv.AddParameter($"-metadata:s:s:{i} language={lang} -metadata:s:s:{i} title=\"{lang}\"");
        }
        conv.AddParameter("-c:s srt");
    }

    public async Task ConvertAsync(string inputDir, string outputDir, bool mergeAudio, bool mergeCaption)
    {
        var video = Directory.GetFiles(inputDir, "*.dat").FirstOrDefault();
        var meta = Directory.GetFiles(inputDir, "*.json").FirstOrDefault();
        if (video == null || meta == null) throw new FileNotFoundException("Missing video info.");
        var audios = Directory.GetFiles(inputDir, "*.adx").Order().ToList();
        var captions = Directory.GetFiles(inputDir, "*.srt").Order().ToList();
        var outputPath = (mergeAudio && mergeCaption) ? outputDir : inputDir;
        var context =  new MergeTaskContext(video, meta, audios, captions, outputPath, mergeAudio, mergeCaption);

        await ConvertVideo(context);
        if (!mergeAudio) await Task.WhenAll(context.AudioPaths.Select(a => ConvertAudio(a, context)));

        Cleanup(inputDir, context);
    }

    private async Task ConvertAudio(string audioPath, MergeTaskContext ctx)
    {
        var conversion = FFmpeg.Conversions.New();
        await AddAudioTracks(conversion, [audioPath]);

        var outputPath = Path.Combine(ctx.OutputDir, Path.GetFileNameWithoutExtension(audioPath) + ".flac");
        conversion.SetOutput(outputPath);
        conversion.SetOverwriteOutput(true);

        try
        {
            await conversion.Start(Ct);
        }
        catch
        {
            TryDeleteFile(outputPath);
            throw;
        }
    }

    private async Task ConvertVideo(MergeTaskContext ctx)
    {
        var conversion = FFmpeg.Conversions.New();
        var extension = await AddVideoTrack(conversion, ctx.VideoPath, ctx.MetaPath);
        if (ctx.MergeAudio) await AddAudioTracks(conversion, ctx.AudioPaths);
        if (ctx.MergeCaption) await AddCaptionTracks(conversion, ctx.CaptionPaths);

        var outputPath = Path.Combine(ctx.OutputDir, $"{Path.GetFileNameWithoutExtension(ctx.VideoPath)}.{extension}");
        conversion.SetOutput(outputPath);
        conversion.SetOverwriteOutput(true);

        try
        {
            await conversion.Start(Ct);
        }
        catch
        {
            TryDeleteFile(outputPath);
            throw;
        }
    }

    private static void TryDeleteFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        for (int i = 0; i < 3; i++)
            try
            {
                File.Delete(filePath);
                return;
            }
            catch (IOException)
            {
                if (i < 2) Thread.Sleep(100);
            }
    }

    private static void Cleanup(string inputDir, MergeTaskContext ctx)
    {
        if (ctx.MergeAudio && ctx.MergeCaption)
        {
            if (Path.GetFullPath(inputDir) != Path.GetFullPath(ctx.OutputDir))
            {
                if (Directory.Exists(inputDir))
                    Directory.Delete(inputDir, true);
            }
            else
            {
                TryDeleteFile(ctx.VideoPath);
                TryDeleteFile(ctx.MetaPath);
                ctx.AudioPaths.ForEach(TryDeleteFile);
                ctx.CaptionPaths.ForEach(TryDeleteFile);
            }
        }
        else
        {
            TryDeleteFile(ctx.VideoPath);
            TryDeleteFile(ctx.MetaPath);
            ctx.AudioPaths.ForEach(TryDeleteFile);
            if (ctx.MergeCaption) ctx.CaptionPaths.ForEach(TryDeleteFile);

            if (Directory.Exists(inputDir) && !Directory.EnumerateFileSystemEntries(inputDir).Any())
                Directory.Delete(inputDir);
        }
    }
}
