using CommunityToolkit.Mvvm.ComponentModel;
using MauiSampleApp.Services.Logging;
using Microsoft.Extensions.Logging;

namespace MauiSampleApp.ViewModels
{
    public class LogViewModel : ObservableObject
    {
        private readonly ILogger<LogViewModel> logger;
        private readonly ILogFileReader logFileReader;
        private string logContent;

        public LogViewModel(
            ILogger<LogViewModel> logger,
            ILogFileReader logFileReader)
        {
            this.logger = logger;
            this.logFileReader = logFileReader;

            _ = this.LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                this.LogContent = await this.logFileReader.ReadLogFileAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "LoadDataAsync failed with exception");
            }
        }

        public string LogContent
        {
            get => this.logContent;
            private set => this.SetProperty(ref this.logContent, value);
        }
    }
}
