using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MauiSampleApp.Services;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications;
#if ANDROID
using Plugin.FirebasePushNotifications.Platforms.Channels;
using Android.App;
using MauiSampleApp.Services;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications;
using NotificationImportance = Android.App.NotificationImportance;
using NotificationVisibility = Android.App.NotificationVisibility;
#else
using NotificationImportance = object;
using NotificationVisibility = object;
#endif

namespace MauiSampleApp.ViewModels
{
    public class NotificationChannelViewModel
    {
        private readonly ILogger logger;
        private readonly IDialogService dialogService;
        private readonly Action<string> deleteNotificationChannel;
        private IAsyncRelayCommand deleteNotificationChannelCommand;

#if ANDROID
        public NotificationChannelViewModel(
            ILogger<NotificationChannelViewModel> logger,
            IDialogService dialogService,
            Action<string> deleteNotificationChannel,
            NotificationChannel notificationChannel)
        {
            this.ChannelId = notificationChannel.Id;
            this.ChannelName = notificationChannel.Name;
            this.IsDefault = notificationChannel.Id == NotificationChannels.Current.Channels.DefaultNotificationChannelId;
            this.Description = notificationChannel.Description;
            this.LockscreenVisibility = Enum.GetName(typeof(NotificationVisibility), notificationChannel.LockscreenVisibility) ?? $"{notificationChannel.LockscreenVisibility}";
            this.Group = notificationChannel.Group ?? "null";
            this.Importance = Enum.GetName(typeof(NotificationImportance), notificationChannel.Importance);

            this.logger = logger;
            this.dialogService = dialogService;
            this.deleteNotificationChannel = deleteNotificationChannel;
        }
#endif

        public string ChannelId { get; }

        public string ChannelName { get; }

        public bool IsDefault { get; }

        public string Description { get; }

        public string LockscreenVisibility { get; }

        public string Group { get; }

        public string Importance { get; }

        public ICommand DeleteNotificationChannelCommand => this.deleteNotificationChannelCommand ??= new AsyncRelayCommand(this.DeleteNotificationChannelAsync);

        private async Task DeleteNotificationChannelAsync()
        {
            try
            {
#if ANDROID
                this.deleteNotificationChannel(this.ChannelId);
#endif
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "DeleteNotificationChannelAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", $"Failed to delete notification channel: {ex.Message}", "OK");
            }
        }
    }
}