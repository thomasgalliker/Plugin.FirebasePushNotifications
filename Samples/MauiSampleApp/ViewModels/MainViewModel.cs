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
        private AsyncRelayCommand subscribeEventsCommand;
        private AsyncRelayCommand unsubscribeEventsCommand;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            IDialogService dialogService,
            IFirebasePushNotification firebasePushNotification)
        {
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


        public ICommand SubscribeEventsCommand => this.subscribeEventsCommand ??= new AsyncRelayCommand(this.SubscribeEvents);

        private async Task SubscribeEvents()
        {
            try
            {
                this.firebasePushNotification.TokenRefreshed += this.OnTokenRefresh;
                this.firebasePushNotification.NotificationReceived += this.OnNotificationReceived;
                this.firebasePushNotification.NotificationOpened += this.OnNotificationOpened;
                this.firebasePushNotification.NotificationAction += this.OnNotificationAction;
                this.firebasePushNotification.NotificationDeleted += this.OnNotificationDeleted;
                this.firebasePushNotification.NotificationError += this.OnNotificationError;

            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "SubscribeEvents failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "SubscribeEvents failed with exception", "OK");
            }
        }

        public ICommand UnsubscribeEventsCommand => this.unsubscribeEventsCommand ??= new AsyncRelayCommand(this.UnsubscribeEvents);

        private async Task UnsubscribeEvents()
        {
            try
            {
                this.firebasePushNotification.TokenRefreshed -= this.OnTokenRefresh;
                this.firebasePushNotification.NotificationReceived -= this.OnNotificationReceived;
                this.firebasePushNotification.NotificationOpened -= this.OnNotificationOpened;
                this.firebasePushNotification.NotificationAction -= this.OnNotificationAction;
                this.firebasePushNotification.NotificationDeleted -= this.OnNotificationDeleted;
                this.firebasePushNotification.NotificationError -= this.OnNotificationError;

            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UnsubscribeEvents failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "UnsubscribeEvents failed with exception", "OK");
            }
        }

        private async void OnTokenRefresh(object sender, FirebasePushNotificationTokenEventArgs e)
        {
            this.logger.LogDebug("OnTokenRefresh");

            await this.dialogService.ShowDialogAsync("FirebasePushNotification", "OnTokenRefresh", "OK");
        }

        private async void OnNotificationReceived(object sender, FirebasePushNotificationDataEventArgs e)
        {
            this.logger.LogDebug($"OnNotificationReceived: {e.Data.ToDebugString()}");
            await this.dialogService.ShowDialogAsync("FirebasePushNotification", "OnNotificationReceived", "OK");
        }

        private async void OnNotificationOpened(object sender, FirebasePushNotificationResponseEventArgs e)
        {
            this.logger.LogDebug($"OnNotificationOpened: {e.Data.ToDebugString()}");
            await this.dialogService.ShowDialogAsync("FirebasePushNotification", "OnNotificationOpened", "OK");
        }
        
        private async void OnNotificationAction(object sender, FirebasePushNotificationResponseEventArgs e)
        {
            this.logger.LogDebug($"OnNotificationAction: {e.Data.ToDebugString()}");
            await this.dialogService.ShowDialogAsync("FirebasePushNotification", "OnNotificationAction", "OK");
        }
        
        private async void OnNotificationDeleted(object sender, FirebasePushNotificationDataEventArgs e)
        {
            this.logger.LogDebug($"OnNotificationDeleted: {e.Data.ToDebugString()}");
            await this.dialogService.ShowDialogAsync("FirebasePushNotification", "OnNotificationDeleted", "OK");
        }

        private async void OnNotificationError(object sender, FirebasePushNotificationErrorEventArgs e)
        {
            this.logger.LogDebug($"OnNotificationError: {e.Message}");
            await this.dialogService.ShowDialogAsync("FirebasePushNotification", $"OnNotificationError: {e.Message}", "OK");
        }

    }
}
