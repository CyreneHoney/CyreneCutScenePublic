using System.CommandLine;
using CyreneCore.Process;
using CyreneCore.Utils;
using static I18N.DotNet.GlobalLocalizer;

namespace CyreneCLI;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        ConsoleConfigUtil.Init();

        var io = new Option<string>(
            aliases: ["--input", "-i"],
            description: Localize("Input file or directory (use & to separate paths)"),
            getDefaultValue: () => string.Join("&", ConsoleConfigUtil.Config.Extract.Input));

        var oo = new Option<string>(
            aliases: ["--output", "-o"],
            description: "Output directory",
            getDefaultValue: () => ConsoleConfigUtil.Config.Extract.Output);

        var co = new Option<bool>(
            aliases: ["--convert", "-c"],
            description: "Whether to convert video and audios",
            getDefaultValue: () => ConsoleConfigUtil.Config.Extract.Convert);

        var mao = new Option<bool>(
            aliases: ["--merge-audio", "-ma"],
            description: "Whether to merge audio",
            getDefaultValue: () => ConsoleConfigUtil.Config.Extract.MergeAudio);

        var mco = new Option<bool>(
            aliases: ["--merge-caption", "-mc"],
            description: "Whether to merge caption",
            getDefaultValue: () => ConsoleConfigUtil.Config.Extract.MergeCaption);

        var ko = new Option<string>(
            aliases: ["--keyfile", "-k"],
            description: "Path to video key file",
            getDefaultValue: () => ConsoleConfigUtil.Config.Path.KeyPath);

        var fo = new Option<string>(
            aliases: ["--ffmpeg", "-f"],
            description: "Path to FFmpeg executable",
            getDefaultValue: () => ConsoleConfigUtil.Config.Path.FFmpegPath);

        var ro = new Option<string>(
            aliases: ["--respath", "-r"],
            description: "Path to resource directory",
            getDefaultValue: () => ConsoleConfigUtil.Config.Path.ResPath);

        var cmd = new RootCommand($"{CoreConst.Name} {CoreConst.Author} {CoreConst.Version}")
        {
            io, oo, co, mao, mco, ko, fo, ro
        };

        cmd.SetHandler(async (i, o, c, ma, mc, k, f, r) =>
        {
            ConsoleConfigUtil.Config.Extract.Input = [.. i.Split("&").Select(s => s.Trim())];
            ConsoleConfigUtil.Config.Extract.Output = o;
            ConsoleConfigUtil.Config.Extract.Convert = c;
            ConsoleConfigUtil.Config.Extract.MergeAudio = ma;
            ConsoleConfigUtil.Config.Extract.MergeCaption = mc;
            ConsoleConfigUtil.Config.Path.KeyPath = k;
            ConsoleConfigUtil.Config.Path.FFmpegPath = f;
            ConsoleConfigUtil.Config.Path.ResPath = r;
            ConsoleConfigUtil.Save();

            await ProcessVideo();
        }, io, oo, co, mao, mco, ko, fo, ro);

        return await cmd.InvokeAsync(args);
    }

    private static async Task ProcessVideo()
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };

        var mgr = new ProcessManager(ConsoleConfigUtil.Config, cts.Token);
        if (!await mgr.InitAsync()) return;

        try
        {
            await foreach (var data in mgr.ProcessAsync().WithCancellation(cts.Token))
            {
                var info = $"[{data.Index}/{data.Total}] ";
                switch (data.Status)
                {
                    case ProcessState.Processing:
                        info += $"Processing {data.Input}";
                        break;
                    case ProcessState.Completed:
                        info += $"Successfully process {data.Input}";
                        break;
                    case ProcessState.Failed:
                        info += $"Failed to process {data.Input}: {data.Msg ?? "Unknown error"}";
                        break;
                }

                Logger.Info(info);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
}
