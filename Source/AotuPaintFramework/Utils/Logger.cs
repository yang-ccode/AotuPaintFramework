using System;
using System.IO;

namespace AotuPaintFramework.Utils
{
    /// <summary>
    /// Static logger class for writing log messages to files.
    /// Logs are stored in the user's AppData\Roaming\AotuPaintFramework\Logs directory.
    /// </summary>
    public static class Logger
    {
        private static readonly string LogFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AotuPaintFramework",
            "Logs");

        private static readonly object LockObject = new object();

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Warning(string message)
        {
            WriteLog("WARNING", message);
        }

        /// <summary>
        /// Logs an error with exception details.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">Optional context message.</param>
        public static void Error(Exception ex, string message = "")
        {
            var logMessage = string.IsNullOrEmpty(message) ? ex.Message : message;
            var fullMessage = $"{logMessage}\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";

            if (ex.InnerException != null)
            {
                fullMessage += $"\nInnerException: {ex.InnerException.Message}";
                if (ex.InnerException.StackTrace != null)
                {
                    fullMessage += $"\nInnerException StackTrace: {ex.InnerException.StackTrace}";
                }
            }

            WriteLog("ERROR", fullMessage);
        }

        /// <summary>
        /// Writes a log entry to the log file.
        /// </summary>
        /// <param name="level">The log level (INFO, WARNING, ERROR).</param>
        /// <param name="message">The message to log.</param>
        private static void WriteLog(string level, string message)
        {
            try
            {
                lock (LockObject)
                {
                    // Ensure log directory exists
                    if (!Directory.Exists(LogFolder))
                    {
                        Directory.CreateDirectory(LogFolder);
                    }

                    // Generate log file name with current date
                    var fileName = $"log_{DateTime.Now:yyyyMMdd}.txt";
                    var filePath = Path.Combine(LogFolder, fileName);

                    // Format log entry
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var logEntry = $"[{timestamp}] [{level}] {message}\n";

                    // Append to log file
                    File.AppendAllText(filePath, logEntry);
                }
            }
            catch
            {
                // Silently ignore logging errors to avoid breaking the application
                // If logging fails, we don't want to crash the plugin
            }
        }
    }
}
