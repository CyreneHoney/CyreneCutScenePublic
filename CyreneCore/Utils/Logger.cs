namespace CyreneCore.Utils;

public class Logger
{
    protected static readonly Lock LogLock = new();
    private static Logger LogInst = new();
    private static FileInfo? LogFile;

    public static void Init<T>(string logPath) where T : Logger, new()
    {
        LogInst = new T();

        var path = Path.Combine(logPath, $"{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        LogFile = new FileInfo(path);
    }

    public static void WriteLog(string message, LogLevel level)
    {
        if (LogFile == null) return;

        lock (LogLock)
        {
            try
            {
                using var sw = LogFile.AppendText();
                sw.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}");
            }
            catch
            {
                // Ignore
            }
        }
    }

    protected virtual void Log(string message, LogLevel level)
    {
        WriteLog(message, level);
    }

    public static void Debug(string message)
    {
        LogInst.Log(message, LogLevel.Debug);
    }

    public static void Info(string message)
    {
        LogInst.Log(message, LogLevel.Info);
    }

    public static void Warn(string message)
    {
        LogInst.Log(message, LogLevel.Warn);
    }

    public static void Error(string message)
    {
        LogInst.Log(message, LogLevel.Error);
    }
}