using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSampleApp.Services;
using MauiSampleApp.Views;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Extensions;
using Plugin.FirebasePushNotifications.Model;

namespace MauiSampleApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private const bool DefaultSubscribeEventsAtStartup = true;
        private const string SubscribeEventsAtStartupKey = "SubscribeEventsAtStartup";

        private readonly ILogger logger;
        private readonly IDialogService dialogService;
        private readonly INavigationService navigationService;
        private readonly IFirebasePushNotification firebasePushNotification;
        private readonly INotificationPermissions notificationPermissions;
        private readonly IShare share;
        private readonly IPreferences preferences;

        private AsyncRelayCommand registerForPushNotificationsCommand;
        private AsyncRelayCommand unregisterForPushNotificationsCommand;
        private AsyncRelayCommand subscribeEventsCommand;
        private AsyncRelayCommand unsubscribeEventsCommand;
        private AsyncRelayCommand navigateToQueuesPageCommand;
        private AsyncRelayCommand shareTokenCommand;
        private AsyncRelayCommand subscribeToTopicCommand;
        private AsyncRelayCommand requestNotificationPermissionsCommand;
        private AuthorizationStatus authorizationStatus;
        private string token;
        private string topic;
        private AsyncRelayCommand unsubscribeFromTopicCommand;
        private AsyncRelayCommand unsubscribeAllTopicsCommand;
        private SubscribedTopicViewModel[] subscribedTopics;
        private AsyncRelayCommand getSubscribedTopicsCommand;
        private bool subscribeEventsAtStartup;
        private AsyncRelayCommand appearingCommand;
        private bool isInitialized;
        private AsyncRelayCommand registerNotificationCategoriesCommand;
        private AsyncRelayCommand getNotificationCategoriesCommand;
        private AsyncRelayCommand clearNotificationCategoriesCommand;
        private NotificationCategoryViewModel[] notificationCategories;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            IDialogService dialogService,
            INavigationService navigationService,
            IFirebasePushNotification firebasePushNotification,
            INotificationPermissions notificationPermissions,
            IShare share,
            IPreferences preferences)
        {
            this.logger = logger;
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.firebasePushNotification = firebasePushNotification;
            this.notificationPermissions = notificationPermissions;
            this.share = share;
            this.preferences = preferences;
        }

        public IAsyncRelayCommand AppearingCommand => this.appearingCommand ??= new AsyncRelayCommand(this.OnAppearingAsync);

        private async Task OnAppearingAsync()
        {
            if (!this.isInitialized)
            {
                await this.InitializeAsync();
                this.isInitialized = true;
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                this.RegisterSampleNotificationCategories();

                await this.UpdateAuthorizationStatusAsync();
                this.UpdateToken();

                var subscribeEventsAtStartup = this.preferences.Get(SubscribeEventsAtStartupKey, DefaultSubscribeEventsAtStartup);

                this.subscribeEventsAtStartup = subscribeEventsAtStartup;
                this.OnPropertyChanged(nameof(this.SubscribeEventsAtStartup));

                if (subscribeEventsAtStartup)
                {
                    await this.SubscribeEventsAsync();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "InitializeAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Initialization failed", "OK");
            }
        }

        private void RegisterSampleNotificationCategories()
        {
            var categories = NotificationCategorySamples.GetAll().ToArray();
            this.firebasePushNotification.RegisterNotificationCategories(categories);
        }

        private async Task UpdateAuthorizationStatusAsync()
        {
            this.AuthorizationStatus = await this.notificationPermissions.GetAuthorizationStatusAsync();
        }

        public AuthorizationStatus AuthorizationStatus
        {
            get => this.authorizationStatus;
            private set => this.SetProperty(ref this.authorizationStatus, value);
        }

        public ICommand RequestNotificationPermissionsCommand => this.requestNotificationPermissionsCommand ??= new AsyncRelayCommand(this.RequestNotificationPermissionsAsync);

        private async Task RequestNotificationPermissionsAsync()
        {
            try
            {
                await this.notificationPermissions.RequestPermissionAsync();
                await this.UpdateAuthorizationStatusAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "RequestNotificationPermissionsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Request for permissions failed", "OK");
            }
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
                await this.firebasePushNotification.UnregisterForPushNotificationsAsync();
                this.UpdateToken();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UnregisterForPushNotificationsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Unregister from push notifications failed with exception", "OK");
            }
        }

        public bool SubscribeEventsAtStartup
        {
            get => this.subscribeEventsAtStartup;
            set
            {
                if (this.SetProperty(ref this.subscribeEventsAtStartup, value))
                {
                    this.preferences.Set(SubscribeEventsAtStartupKey, value);
                }
            }
        }

        public ICommand SubscribeEventsCommand => this.subscribeEventsCommand ??= new AsyncRelayCommand(this.SubscribeEventsAsync);

        private async Task SubscribeEventsAsync()
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

        public ICommand UnsubscribeEventsCommand => this.unsubscribeEventsCommand ??= new AsyncRelayCommand(this.UnsubscribeEventsAsync);

        private async Task UnsubscribeEventsAsync()
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
            this.Token = e.Token;

            await this.dialogService.ShowDialogAsync("OnTokenRefresh", e.ToString(), "OK");
        }

        private async void OnNotificationReceived(object sender, FirebasePushNotificationDataEventArgs e)
        {
            await this.dialogService.ShowDialogAsync("OnNotificationReceived", e.ToString(), "OK");
        }

        private async void OnNotificationOpened(object sender, FirebasePushNotificationResponseEventArgs e)
        {
            await WaitAsync();
            await this.dialogService.ShowDialogAsync("OnNotificationOpened", e.ToString(), "OK");
        }

        private async void OnNotificationAction(object sender, FirebasePushNotificationResponseEventArgs e)
        {
            await WaitAsync();
            await this.dialogService.ShowDialogAsync("OnNotificationAction", e.ToString(), "OK");
        }

        private async void OnNotificationDeleted(object sender, FirebasePushNotificationDataEventArgs e)
        {
            await WaitAsync();
            await this.dialogService.ShowDialogAsync("OnNotificationDeleted", e.ToString(), "OK");
        }

        private async void OnNotificationError(object sender, FirebasePushNotificationErrorEventArgs e)
        {
            await this.dialogService.ShowDialogAsync("OnNotificationError", e.ToString(), "OK");
        }

        private static async Task WaitAsync()
        {
            // Android doesn't display the alert dialog if we're about to come from background to foreground,
            // that's why we wait some time here...
            await Task.Delay(1000);
        }

        private void UpdateToken()
        {
            this.Token = this.firebasePushNotification.Token;
        }

        public string Token
        {
            get => this.token;
            private set => this.SetProperty(ref this.token, value);
        }

        public ICommand ShareTokenCommand => this.shareTokenCommand ??= new AsyncRelayCommand(this.ShareTokenAsync);

        private async Task ShareTokenAsync()
        {
            var shareRequest = new ShareTextRequest(this.Token);
            await this.share.RequestAsync(shareRequest);
        }

        public SubscribedTopicViewModel[] SubscribedTopics
        {
            get => this.subscribedTopics;
            private set => this.SetProperty(ref this.subscribedTopics, value);
        }

        public ICommand GetSubscribedTopicsCommand => this.getSubscribedTopicsCommand ??= new AsyncRelayCommand(this.GetSubscribedTopicsAsync);

        private async Task GetSubscribedTopicsAsync()
        {
            try
            {
                this.UpdateSubscribedTopics();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "GetSubscribedTopicsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Get subscribed topics failed with exception", "OK");
            }
        }

        private void UpdateSubscribedTopics()
        {
            this.SubscribedTopics = this.firebasePushNotification.SubscribedTopics
                .Select(t => new SubscribedTopicViewModel(t, t => this.UnsubscribeFromTopicAsync(t)))
                .ToArray();
        }

        public string Topic
        {
            get => this.topic;
            set => this.SetProperty(ref this.topic, value);
        }

        public ICommand SubscribeToTopicCommand => this.subscribeToTopicCommand ??= new AsyncRelayCommand(this.SubscribeToTopicAsync);

        private async Task SubscribeToTopicAsync()
        {
            try
            {
                var topic = this.Topic;
                this.firebasePushNotification.Subscribe(topic);
                this.UpdateSubscribedTopics();
                this.Topic = null;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "SubscribeToTopicAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Subscribe to topic failed with exception", "OK");
            }
        }

        public ICommand UnsubscribeFromTopicCommand => this.unsubscribeFromTopicCommand ??= new AsyncRelayCommand(() => this.UnsubscribeFromTopicAsync(this.Topic));

        private async Task UnsubscribeFromTopicAsync(string topic)
        {
            try
            {
                this.firebasePushNotification.Unsubscribe(topic);
                this.UpdateSubscribedTopics();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UnsubscribeFromTopicAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Unsubscribe from topic failed with exception", "OK");
            }
        }

        public ICommand UnsubscribeAllTopicsCommand => this.unsubscribeAllTopicsCommand ??= new AsyncRelayCommand(this.UnsubscribeAllTopicsAsync);

        private async Task UnsubscribeAllTopicsAsync()
        {
            try
            {
                this.firebasePushNotification.UnsubscribeAll();
                this.UpdateSubscribedTopics();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UnsubscribeAllTopicsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Unsubscribe from all topics failed with exception", "OK");
            }
        }


        public NotificationCategoryViewModel[] NotificationCategories
        {
            get => this.notificationCategories;
            private set => this.SetProperty(ref this.notificationCategories, value);
        }

        public ICommand GetNotificationCategoriesCommand => this.getNotificationCategoriesCommand ??= new AsyncRelayCommand(this.GetNotificationCategoriesAsync);

        private async Task GetNotificationCategoriesAsync()
        {
            try
            {
                var notificationCategories = this.firebasePushNotification.GetNotificationCategories();
                this.NotificationCategories = notificationCategories
                    .Select(n => new NotificationCategoryViewModel(n))
                    .ToArray();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "GetNotificationCategoriesAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Reading notification categories failed with exception", "OK");
            }
        }
        
        public ICommand RegisterNotificationCategoriesCommand => this.registerNotificationCategoriesCommand ??= new AsyncRelayCommand(this.RegisterNotificationCategoriesAsync);

        private async Task RegisterNotificationCategoriesAsync()
        {
            try
            {
                this.RegisterSampleNotificationCategories();
                await this.GetNotificationCategoriesAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "RegisterNotificationCategoriesAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Registration of notification categories failed with exception", "OK");
            }
        }
        
        public ICommand ClearNotificationCategoriesCommand => this.clearNotificationCategoriesCommand ??= new AsyncRelayCommand(this.ClearNotificationCategoriesAsync);

        private async Task ClearNotificationCategoriesAsync()
        {
            try
            {
                this.firebasePushNotification.ClearNotificationCategories();
                await this.GetNotificationCategoriesAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ClearNotificationCategoriesAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Clearing notification categories failed with exception", "OK");
            }
        }

        public ICommand NavigateToQueuesPageCommand => this.navigateToQueuesPageCommand ??= new AsyncRelayCommand(this.NavigateToQueuesPageAsync);

        private async Task NavigateToQueuesPageAsync()
        {
            await this.navigationService.PushAsync<QueuesPage>();
        }
    }
}
