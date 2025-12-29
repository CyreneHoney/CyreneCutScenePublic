using CyreneCore.Utils;

namespace CyreneCLI;

public class ConsoleConfigUtil : CoreConfigUtil
{
    public static CoreConfig Config { get; set; } = new();

    public static void Init()
    {
        Config = LoadConfig<CoreConfig, Logger>(Config);
    }

    public static void Save()
    {
        SaveConfig(Config);
    }
}