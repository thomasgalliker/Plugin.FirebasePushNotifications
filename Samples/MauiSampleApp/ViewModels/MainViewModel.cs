using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MauiSampleApp.Services;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Extensions;

namespace MauiSampleApp.ViewModels
{
    public class MainViewModel
    {
        private readonly ILogger logger;
        private readonly IDialogService dialogService;
        private readonly IFirebasePushNotification firebasePushNotification;

        private AsyncRelayCommand registerForPushNotificationsCommand;
        private AsyncRelayCommand unregisterForPushNotificationsCommand;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            IDialogService dialogService, 
            IFirebasePushNotification firebasePushNotification)
        {
            //var firebasePushNotification = CrossFirebasePushNotification.Current;

            firebasePushNotification.TokenRefreshed += this.OnTokenRefresh;
            firebasePushNotification.NotificationReceived += this.OnNotificationReceived;

            firebasePushNotification.ClearAllNotifications();
            this.logger = logger;
            this.dialogService = dialogService;
            this.firebasePushNotification = firebasePushNotification;
        }

        public ICommand RegisterForPushNotificationsCommand => this.registerForPushNotificationsCommand ??= new AsyncRelayCommand(this.RegisterForPushNotificationsAsync);

        private async Task RegisterForPushNotificationsAsync()
        {
            try
            {
                await this.firebasePushNotification.RegisterForPushNotificationsAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "RegisterForPushNotificationsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Register for push notifications failed with exception", "OK");
            }
        }
        
        public ICommand UnregisterForPushNotificationsCommand => this.unregisterForPushNotificationsCommand ??= new AsyncRelayCommand(this.UnregisterForPushNotificationsAsync);

        private async Task UnregisterForPushNotificationsAsync()
        {
            try
            {
                this.firebasePushNotification.UnregisterForPushNotifications();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UnregisterForPushNotificationsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Unregister from push notifications failed with exception", "OK");
            }
        }

        private async void OnNotificationReceived(object sender, FirebasePushNotificationDataEventArgs e)
        {
            this.logger.LogDebug($"OnNotificationReceived: {e.Data.ToDebugString()}");

            await this.dialogService.ShowDialogAsync("FirebasePushNotification", "OnNotificationReceived", "OK");
        }

        private async void OnTokenRefresh(object sender, FirebasePushNotificationTokenEventArgs e)
        {
            this.logger.LogDebug("OnTokenRefresh");

            await this.dialogService.ShowDialogAsync("FirebasePushNotification", "OnTokenRefresh", "OK");
        }
    }
}
