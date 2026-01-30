using System;
using System.IO;

namespace AotuPaintFramework.Utils
{
    /// <summary>
    /// Static logger class for application-wide logging.
    /// Writes logs to daily log files in the user's AppData folder.
    /// </summary>
    public static class Logger
    {
        private static readonly string LogDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AotuPaintFramework",
            "Logs"
        );

        private static readonly object _lock = new object();

        /// <summary>
        /// Gets the log file path for the current date.
        /// </summary>
        private static string GetLogFilePath()
        {
            string dateString = DateTime.Now.ToString("yyyyMMdd");
            return Path.Combine(LogDirectory, $"log_{dateString}.txt");
        }

        /// <summary>
        /// Ensures the log directory exists.
        /// </summary>
        private static void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }
        }

        /// <summary>
        /// Writes a log entry to the log file.
        /// </summary>
        /// <param name="level">Log level (INFO, WARNING, ERROR)</param>
        /// <param name="message">Log message</param>
        private static void WriteLog(string level, string message)
        {
            lock (_lock)
            {
                try
                {
                    EnsureLogDirectoryExists();
                    string logFilePath = GetLogFilePath();
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string logEntry = $"[{timestamp}] [{level}] {message}";
                    
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                }
                catch
                {
                    // Intentionally silent: Logging failures should never break the application.
                    // If logging fails (e.g., disk full, permissions issue), the application
                    // should continue to function. Consider writing to Event Log as fallback
                    // if logging infrastructure becomes critical.
                }
            }
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Warning(string message)
        {
            WriteLog("WARNING", message);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log</param>
        public static void Error(string message)
        {
            WriteLog("ERROR", message);
        }

        /// <summary>
        /// Logs an error with exception details.
        /// </summary>
        /// <param name="exception">The exception to log</param>
        /// <param name="message">Additional context message</param>
        public static void Error(Exception exception, string message)
        {
            if (exception == null)
            {
                Error(message);
                return;
            }

            lock (_lock)
            {
                try
                {
                    EnsureLogDirectoryExists();
                    string logFilePath = GetLogFilePath();
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    
                    var logEntry = $"[{timestamp}] [ERROR] {message}{Environment.NewLine}";
                    logEntry += $"Exception: {exception.GetType().Name}: {exception.Message}{Environment.NewLine}";
                    logEntry += $"Stack Trace: {exception.StackTrace}{Environment.NewLine}";
                    
                    if (exception.InnerException != null)
                    {
                        logEntry += $"Inner Exception: {exception.InnerException.GetType().Name}: {exception.InnerException.Message}{Environment.NewLine}";
                        logEntry += $"Inner Stack Trace: {exception.InnerException.StackTrace}{Environment.NewLine}";
                    }
                    
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                }
                catch
                {
                    // Intentionally silent: Logging failures should never break the application.
                    // If logging fails (e.g., disk full, permissions issue), the application
                    // should continue to function. Consider writing to Event Log as fallback
                    // if logging infrastructure becomes critical.
                }
            }
        }
    }
}
