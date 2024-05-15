using NLog;
using NLog.Config;
using NLog.Targets;

namespace MauiSampleApp.Services.Logging
{
    public static class NLogLoggerConfiguration
    {
        static NLogLoggerConfiguration()
        {
            LogFilePath = CreateLogFile();
            LogManager.Configuration = GetLoggingConfiguration(LogFilePath);
        }

        public static string LogFilePath { get; }

        private static string CreateLogFile()
        {
            var filename = $"{AppInfo.PackageName}.log";
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (!Directory.Exists(folder))
            {
                folder = Directory.CreateDirectory(folder).FullName;
            }

            var filePath = Path.Combine(folder, filename);
            if (!File.Exists(filePath))
            {
                File.Create(filePath);
            }

            return filePath;
        }

        private static LoggingConfiguration GetLoggingConfiguration(string logFilePath)
        {
            var config = new LoggingConfiguration();
            var layout = "${longdate:universalTime=True}|${level}|${logger}|${message}|${exception:format=tostring}[EOL]";

            // Console Target
            var consoleTarget = new ConsoleTarget();
            consoleTarget.Layout = layout;
            config.AddTarget("console", consoleTarget);

            var consoleRule = new LoggingRule("*", LogLevel.Trace, consoleTarget);
            config.LoggingRules.Add(consoleRule);
            
            // Debug Target
            var debugTarget = new DebugTarget();
            debugTarget.Layout = layout;
            config.AddTarget("debug", debugTarget);

            var debugRule = new LoggingRule("*", LogLevel.Trace, debugTarget);
            config.LoggingRules.Add(debugRule);

            // File Target
            var fileTarget = new FileTarget();
            fileTarget.FileName = logFilePath;
            fileTarget.Layout = layout;
            fileTarget.MaxArchiveFiles = 1;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Rolling;
            fileTarget.ArchiveAboveSize = 10485760; // 10MB
            fileTarget.ConcurrentWrites = true;
            fileTarget.KeepFileOpen = false;
            config.AddTarget("file", fileTarget);

            var fileRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.LoggingRules.Add(fileRule);

            LogManager.Configuration = config;

            return config;
        }
    }
}