using CyreneCore.Utils;

namespace CyreneCLI;

public class ConsoleLog : Logger
{
    protected override void Log(string message, LogLevel level)
    {
        base.Log(message, level);

        lock (LogLock)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var originalColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write($"[{timestamp}] ");

            Console.ForegroundColor = GetColor(level);
            Console.Write($"[{level}] ");

            Console.ForegroundColor = originalColor;
            Console.WriteLine(message);
        }
    }

    private static ConsoleColor GetColor(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => ConsoleColor.Blue,
            LogLevel.Info => ConsoleColor.Cyan,
            LogLevel.Warn => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
    }
}