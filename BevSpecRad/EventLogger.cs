using System;
using System.IO;

namespace BevSpecRad
{
    public class EventLogger
    {
        private StreamWriter logFile;
        private const string logFileName = "BevSpecRadLog.txt";
        
        public string LogDirectory { get; }

        public EventLogger(string baseDirectory)
        {
            LogDirectory = CreateTimestampedSubdirectory(baseDirectory);
            logFile = new StreamWriter(Path.Combine(LogDirectory, logFileName));
            logFile.AutoFlush = true;
            LogEvent($"Log started. Filename: {logFileName}");
        }

        public void Write(string message)
        {
            logFile.Write(message);
        }

        public void LogEvent(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            logFile.WriteLine($"[{timestamp}] {message}");
        }

        public void WriteLine(string message)
        {
            logFile.WriteLine(message);
        }

        public void WriteLine()
        {
            logFile.WriteLine();
        }

        public void Close()
        {
            LogEvent("Log closed.");
            logFile?.Close();
        }

        // Creates a subdirectory under the specified basePath.
        // The directory name reflects the current local date and time down to minutes: "yyyy-MM-dd_HH-mm".
        // If a directory with the same name already exists, a numeric suffix is appended to make it unique.
        private static string CreateTimestampedSubdirectory(string basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
                throw new ArgumentException("Base path must be provided.", nameof(basePath));
            // Ensure base path exists
            Directory.CreateDirectory(basePath);
            // Use local time; adjust format to minutes only
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
            string dirName = timestamp;
            string candidatePath = Path.Combine(basePath, dirName);
            // If the directory already exists, append a counter to make it unique
            int counter = 1;
            while (Directory.Exists(candidatePath))
            {
                dirName = $"{timestamp}_{counter}";
                candidatePath = Path.Combine(basePath, dirName);
                counter++;
            }
            Directory.CreateDirectory(candidatePath);
            return candidatePath;
        }

    }
}
