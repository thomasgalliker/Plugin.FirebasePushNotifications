using System.Net.Mime;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSampleApp.Services.Logging;
using Microsoft.Extensions.Logging;

namespace MauiSampleApp.ViewModels
{
    public class LogViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly ILogFileReader logFileReader;
        private readonly IEmail email;
        private readonly IShare share;
        private readonly IAppInfo appInfo;
        private readonly IDeviceInfo deviceInfo;
        private readonly IFileSystem fileSystem;

        private string logContent;
        private string logFileInfo;

        public LogViewModel(
            ILogger<LogViewModel> logger,
            ILogFileReader logFileReader,
            IEmail email,
            IShare share,
            IAppInfo appInfo,
            IDeviceInfo deviceInfo,
            IFileSystem fileSystem)
        {
            this.logger = logger;
            this.logFileReader = logFileReader;
            this.email = email;
            this.share = share;
            this.appInfo = appInfo;
            this.deviceInfo = deviceInfo;
            this.fileSystem = fileSystem;

            this.ReloadLogCommand = new AsyncRelayCommand(
                execute: this.Init);

            this.DeleteLogFilesCommand = new AsyncRelayCommand(
                execute: this.DeleteLogFilesAsync);

            this.SendLogCommand = new AsyncRelayCommand(
                execute: this.SendLogAsync);

            this.ShareLogCommand = new AsyncRelayCommand(
                execute: this.ShareLogAsync);

            _ = this.Init();
        }

        public string LogFileInfo
        {
            get => this.logFileInfo;
            private set => this.SetProperty(ref this.logFileInfo, value);
        }

        public string LogContent
        {
            get => this.logContent;
            private set => this.SetProperty(ref this.logContent, value);
        }

        public ICommand ReloadLogCommand { get; }

        private async Task Init()
        {
            try
            {
                this.LogFileInfo = this.GetLogFileInfo();

                this.LogContent = await this.logFileReader.ReadLogFileAsync(numberOfLines: 1000);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to init viewmodel");
            }
        }

        private string GetLogFileInfo()
        {
            var logFileInfo = new FileInfo(this.logFileReader.FilePath);
            return $"{logFileInfo.Name} ({logFileInfo.Length}B)";
        }

        public ICommand DeleteLogFilesCommand { get; }

        private async Task DeleteLogFilesAsync()
        {
            try
            {
                var deletedFiles = this.logFileReader.DeleteLogFiles();

                this.logger.LogInformation($"Log file{(deletedFiles > 1 ? "s" : "")} deleted");

                await this.Init();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "DeleteLogFilesAsync failed with exception");
            }
        }

        public ICommand SendLogCommand { get; }

        private async Task SendLogAsync()
        {
            try
            {
                var messageBody = new StringBuilder()
                    .AppendLine($"Name: {this.appInfo.Name}")
                    .AppendLine($"PackageName: {this.appInfo.PackageName}")
                    .AppendLine($"Version: {this.appInfo.VersionString} ({this.appInfo.BuildString})")
                    .AppendLine($"Platform: {this.deviceInfo.Platform} {this.deviceInfo.Version}")
                    .AppendLine($"Device: {this.deviceInfo.Manufacturer} {this.deviceInfo.Model}")
                    .AppendLine()
                    .ToString();

                var cacheDirectory = this.fileSystem.CacheDirectory;
                var logFiles = this.logFileReader.EnumerateLogFiles()
                    .Select(f => CopyFileToCacheDirectory(f, cacheDirectory))
                    .Select(f => new EmailAttachment(f, MediaTypeNames.Text.Plain))
                    .ToList();

                var emailMessage = new EmailMessage
                {
                    Subject = $"{this.appInfo.Name} Log File{(logFiles.Count > 1 ? "s" : "")} {DateTime.Now:G}",
                    Body = messageBody,
                    Attachments = logFiles
                };
                await this.email.ComposeAsync(emailMessage);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "SendLogAsync failed with exception");
            }
        }

        public ICommand ShareLogCommand { get; }

        private async Task ShareLogAsync()
        {
            try
            {
                var cacheDirectory = this.fileSystem.CacheDirectory;
                var logFiles = this.logFileReader.EnumerateLogFiles()
                    .Select(f => CopyFileToCacheDirectory(f, cacheDirectory))
                    .Select(f => new ShareFile(f, MediaTypeNames.Text.Plain))
                    .ToList();

                var shareRequest = new ShareMultipleFilesRequest
                {
                    Title = $"{this.appInfo.Name} Log File{(logFiles.Count > 1 ? "s" : "")} {DateTime.Now:G}",
                    Files = logFiles
                };
                await this.share.RequestAsync(shareRequest);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ShareLogAsync failed with exception");
            }
        }

        private static string CopyFileToCacheDirectory(string sourceFilePath, string destinationFolder)
        {
            var destinationFileName = $"{Path.GetFileNameWithoutExtension(sourceFilePath)}_{File.GetLastWriteTime(sourceFilePath):yyyy-dd-MM_THH-mm-ss}{Path.GetExtension(sourceFilePath)}";
            var destinationFilePath = Path.Combine(destinationFolder, destinationFileName);
            File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
            return destinationFilePath;
        }
    }
}