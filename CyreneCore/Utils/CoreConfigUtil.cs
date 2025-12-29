using Newtonsoft.Json;
using static I18N.DotNet.GlobalLocalizer;

namespace CyreneCore.Utils;

public class CoreConfigUtil
{
    protected static C LoadConfig<C, L>(C config) where C : CoreConfig, new() where L : Logger, new()
    {
        I18NUtil.Init(config.Language);

        if (File.Exists(CoreConst.ConfigPath))
        {
            try
            {
                var file = JsonUtil.DeserializeObject<C>(CoreConst.ConfigPath);
                if (file != null) config = file;
            }
            catch (JsonException ex)
            {
                Logger.Error(LocalizeFormat("DeserializeErr", "Config", ex.Message));
            }
        }
        SaveConfig(config);
        GenDirs(config);

        I18NUtil.Init(config.Language);

        Logger.Init<L>(config.Path.LogPath);

        return config;
    }

    protected static void SaveConfig<C>(C config) where C : CoreConfig, new()
    {
        var json = JsonConvert.SerializeObject(config, JsonUtil.Settings);
        using var stream = new FileStream(CoreConst.ConfigPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        using var writer = new StreamWriter(stream);
        writer.Write(json);
    }

    private static void GenDirs<C>(C config) where C : CoreConfig, new()
    {
        foreach (var property in config.Path.GetType().GetProperties())
        {
            var dir = property.GetValue(config.Path)?.ToString();
            if (!string.IsNullOrEmpty(dir) && !dir.Contains('.') && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
    }
}