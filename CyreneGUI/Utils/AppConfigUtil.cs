using CyreneCore.Utils;

namespace CyreneGUI.Utils;

public class AppConfigUtil : CoreConfigUtil
{
    public static AppConfig Config { get; set; } = new();

    public static void Init()
    {
        Config = LoadConfig<AppConfig, AppLog>(Config);
    }

    public static void Save()
    {
        SaveConfig(Config);
    }
}