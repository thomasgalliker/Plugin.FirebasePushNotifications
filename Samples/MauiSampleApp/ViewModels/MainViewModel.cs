using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSampleApp.Services;
using MauiSampleApp.Views;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Extensions;

namespace MauiSampleApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly IDialogService dialogService;
        private readonly INavigationService navigationService;
        private readonly IFirebasePushNotification firebasePushNotification;
        private readonly IShare share;

        private AsyncRelayCommand registerForPushNotificationsCommand;
        private AsyncRelayCommand unregisterForPushNotificationsCommand;
        private AsyncRelayCommand subscribeEventsCommand;
        private AsyncRelayCommand unsubscribeEventsCommand;
        private AsyncRelayCommand navigateToQueuesPageCommand;
        private AsyncRelayCommand shareTokenCommand;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            IDialogService dialogService,
            INavigationService navigationService,
            IFirebasePushNotification firebasePushNotification,
            IShare share)
        {
            this.logger = logger;
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.firebasePushNotification = firebasePushNotification;
            this.share = share;
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
            this.OnPropertyChanged(nameof(this.Token));

            await this.dialogService.ShowDialogAsync("OnTokenRefresh", $"{e.Token}", "OK");
        }

        private async void OnNotificationReceived(object sender, FirebasePushNotificationDataEventArgs e)
        {
            await this.dialogService.ShowDialogAsync("OnNotificationReceived", e.Data.ToDebugString(), "OK");
        }

        private async void OnNotificationOpened(object sender, FirebasePushNotificationResponseEventArgs e)
        {
            await this.dialogService.ShowDialogAsync("OnNotificationOpened", e.Data.ToDebugString(), "OK");
        }

        private async void OnNotificationAction(object sender, FirebasePushNotificationResponseEventArgs e)
        {
            await this.dialogService.ShowDialogAsync("OnNotificationAction", e.Data.ToDebugString(), "OK");
        }

        private async void OnNotificationDeleted(object sender, FirebasePushNotificationDataEventArgs e)
        {
            await this.dialogService.ShowDialogAsync("OnNotificationDeleted", e.Data.ToDebugString(), "OK");
        }

        private async void OnNotificationError(object sender, FirebasePushNotificationErrorEventArgs e)
        {
            await this.dialogService.ShowDialogAsync("OnNotificationError", e.Message, "OK");
        }

        public ICommand ShareTokenCommand => this.shareTokenCommand ??= new AsyncRelayCommand(this.ShareTokenAsync);

        public string Token
        {
            get
            {
                return this.firebasePushNotification.Token;
            }
        }

        private async Task ShareTokenAsync()
        {
            var shareRequest = new ShareTextRequest(this.Token);
            await this.share.RequestAsync(shareRequest);
        }

        public ICommand NavigateToQueuesPageCommand => this.navigateToQueuesPageCommand ??= new AsyncRelayCommand(this.NavigateToQueuesPageAsync);

        private async Task NavigateToQueuesPageAsync()
        {
            await this.navigationService.PushAsync<QueuesPage>();
        }
    }
}
