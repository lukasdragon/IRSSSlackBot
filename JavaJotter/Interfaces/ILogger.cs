namespace JavaJotter.Interfaces;

public interface ILogger
{
    public enum LogSeverity
    {
        Info,
        Warning,
        Error
    }

    public void Log(string message, LogSeverity logSeverity = LogSeverity.Info);

    public void LogWarning(string message)
    {
        Log(message, LogSeverity.Warning);
    }

    public void LogError(string message)
    {
        Log(message, LogSeverity.Error);
    }
}