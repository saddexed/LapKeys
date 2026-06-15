using System.IO;
using System.Text;

namespace LapKeys.Services;

public enum LogLevel
{
    Info,
    Warning,
    Error
}

/// <summary>
/// Lightweight file logger. Appends timestamped entries to a rolling log file in
/// %LocalAppData%/LapKeys/logs and raises <see cref="Logged"/> so the UI footer can
/// reflect the latest message.
/// </summary>
public static class LogService
{
    private static readonly string LogFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LapKeys", "logs");

    private static readonly string LogFile = Path.Combine(LogFolder, "lapkeys.log");

    // Roll the file once it grows past this size, keeping a single .old backup.
    private const long MaxLogBytes = 1024 * 1024; // 1 MB

    private static readonly object Sync = new();

    /// <summary>Raised on every log entry with the level and formatted message.</summary>
    public static event Action<LogLevel, string>? Logged;

    public static string LogFilePath => LogFile;

    public static void Info(string message) => Write(LogLevel.Info, message);

    public static void Warning(string message) => Write(LogLevel.Warning, message);

    public static void Error(string message) => Write(LogLevel.Error, message);

    public static void Error(string message, Exception ex)
        => Write(LogLevel.Error, $"{message}: {ex.GetType().Name}: {ex.Message}");

    private static void Write(LogLevel level, string message)
    {
        // Notify subscribers (footer) regardless of whether file IO succeeds.
        Logged?.Invoke(level, message);

        try
        {
            lock (Sync)
            {
                Directory.CreateDirectory(LogFolder);
                RollIfNeeded();

                string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level.ToString().ToUpperInvariant()}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFile, line, Encoding.UTF8);
            }
        }
        catch
        {
            // Never let logging throw into the app.
        }
    }

    private static void RollIfNeeded()
    {
        try
        {
            var info = new FileInfo(LogFile);
            if (info.Exists && info.Length >= MaxLogBytes)
            {
                string backup = LogFile + ".old";
                if (File.Exists(backup))
                    File.Delete(backup);
                File.Move(LogFile, backup);
            }
        }
        catch
        {
            // Ignore roll failures; logging continues to the existing file.
        }
    }
}
