﻿using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSampleApp.Services;
using MauiSampleApp.Views;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications;
using Plugin.FirebasePushNotifications.Model;

#if ANDROID
using Android.App;
using NotificationChannelRequest = Plugin.FirebasePushNotifications.Platforms.Channels.NotificationChannelRequest;
using NotificationChannelSamples = MauiSampleApp.Platforms.Notifications.NotificationChannelSamples;
using NotificationChannelGroupSamples = MauiSampleApp.Platforms.Notifications.NotificationChannelGroupSamples;
#endif

#if IOS
using UserNotifications;
#endif

namespace MauiSampleApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private const bool DefaultSubscribeEventsAtStartup = true;
        private const string SubscribeEventsAtStartupKey = "SubscribeEventsAtStartup";

        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly IDialogService dialogService;
        private readonly INavigationService navigationService;
        private readonly IFirebasePushNotification firebasePushNotification;
        private readonly INotificationChannels notificationChannels;
        private readonly INotificationPermissions notificationPermissions;
        private readonly FirebasePushNotificationOptions firebasePushNotificationOptions;
        private readonly IShare share;
        private readonly IClipboard clipboard;
        private readonly IPreferences preferences;
        private readonly IBrowser browser;
        private readonly IAppInfo appInfo;

        private IAsyncRelayCommand registerForPushNotificationsCommand;
        private IAsyncRelayCommand unregisterForPushNotificationsCommand;
        private IAsyncRelayCommand subscribeEventsCommand;
        private IAsyncRelayCommand unsubscribeEventsCommand;
        private IAsyncRelayCommand navigateToQueuesPageCommand;
        private IAsyncRelayCommand navigateToLogPageCommand;
        private IAsyncRelayCommand capturePhotoCommand;
        private IAsyncRelayCommand shareTokenCommand;
        private IAsyncRelayCommand getTokenCommand;
        private IAsyncRelayCommand subscribeToTopicCommand;
        private IAsyncRelayCommand requestNotificationPermissionsCommand;
        private AuthorizationStatus authorizationStatus;
        private string token;
        private string topic;
        private IAsyncRelayCommand unsubscribeAllTopicsCommand;
        private SubscribedTopicViewModel[] subscribedTopics;
        private IAsyncRelayCommand getSubscribedTopicsCommand;
        private bool subscribeEventsAtStartup;
        private bool isSubscribedToEvents;
        private IAsyncRelayCommand appearingCommand;
        private bool isInitialized;
        private IAsyncRelayCommand registerNotificationCategoriesCommand;
        private IAsyncRelayCommand getNotificationCategoriesCommand;
        private IAsyncRelayCommand clearNotificationCategoriesCommand;
        private NotificationCategoryViewModel[] notificationCategories;
        private IAsyncRelayCommand getNotificationChannelsCommand;
        private string[] channelGroups;
        private NotificationChannelViewModel[] channels;
        private IAsyncRelayCommand copyTokenCommand;
        private IAsyncRelayCommand deleteNotificationChannelsCommand;
        private IAsyncRelayCommand setNotificationChannelsCommand;
        private IAsyncRelayCommand createNotificationChannelsCommand;
        private IAsyncRelayCommand<string> openUrlCommand;
        private IAsyncRelayCommand createNotificationChannelGroupsCommand;
        private IAsyncRelayCommand deleteNotificationChannelGroupsCommand;
        private string sdkVersion;
        private IAsyncRelayCommand getNotificationChannelGroupsCommand;
        private IAsyncRelayCommand openNotificationSettingsCommand;
        private IAsyncRelayCommand openNotificationChannelSettingsCommand;
        private string defaultNotificationChannelId;

#if IOS
        private UNNotificationPresentationOptions[] presentationOptions;
#endif

        public MainViewModel(
            ILogger<MainViewModel> logger,
            ILoggerFactory loggerFactory,
            IDialogService dialogService,
            INavigationService navigationService,
            IFirebasePushNotification firebasePushNotification,
            INotificationChannels notificationChannels,
            INotificationPermissions notificationPermissions,
            FirebasePushNotificationOptions firebasePushNotificationOptions,
            IShare share,
            IClipboard clipboard,
            IPreferences preferences,
            IBrowser browser,
            IAppInfo appInfo)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.firebasePushNotification = firebasePushNotification;
            this.notificationChannels = notificationChannels;
            this.notificationPermissions = notificationPermissions;
            this.firebasePushNotificationOptions = firebasePushNotificationOptions;
            this.share = share;
            this.clipboard = clipboard;
            this.preferences = preferences;
            this.browser = browser;
            this.appInfo = appInfo;
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
                this.SdkVersion = this.firebasePushNotification.SdkVersion;

                await this.UpdateAuthorizationStatusAsync();
                this.UpdateToken();

                var subscribeEventsAtStartup = this.preferences.Get(SubscribeEventsAtStartupKey, DefaultSubscribeEventsAtStartup);

                this.subscribeEventsAtStartup = subscribeEventsAtStartup;
                this.OnPropertyChanged(nameof(this.SubscribeEventsAtStartup));

                if (subscribeEventsAtStartup)
                {
                    await this.SubscribeEventsAsync();
                }

                await this.GetNotificationChannelGroupsAsync();
                await this.GetNotificationChannelsAsync();
                await this.GetSubscribedTopicsAsync();
                await this.GetNotificationCategoriesAsync();

#if IOS
                this.PresentationOptions = Enum.GetValues<UNNotificationPresentationOptions>()
                    .Concat(new[]
                    {
                        UNNotificationPresentationOptions.List | UNNotificationPresentationOptions.Banner,
                        UNNotificationPresentationOptions.List | UNNotificationPresentationOptions.Banner | UNNotificationPresentationOptions.Sound,
                    })
                    .ToArray();
#endif
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "InitializeAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Initialization failed", "OK");
            }
        }

        public string SdkVersion
        {
            get => this.sdkVersion;
            private set => this.SetProperty(ref this.sdkVersion, value);
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

        public ICommand RequestNotificationPermissionsCommand
        {
            get => this.requestNotificationPermissionsCommand ??= new AsyncRelayCommand(this.RequestNotificationPermissionsAsync);
        }

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

        public IAsyncRelayCommand RegisterForPushNotificationsCommand
        {
            get => this.registerForPushNotificationsCommand ??= new AsyncRelayCommand(this.RegisterForPushNotificationsAsync);
        }

        private async Task RegisterForPushNotificationsAsync()
        {
            try
            {
                await this.firebasePushNotification.RegisterForPushNotificationsAsync();
                await this.UpdateAuthorizationStatusAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "RegisterForPushNotificationsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Register for push notifications failed with exception", "OK");
            }
            finally
            {
                this.UpdateToken();
            }
        }

        public IAsyncRelayCommand UnregisterForPushNotificationsCommand
        {
            get => this.unregisterForPushNotificationsCommand ??= new AsyncRelayCommand(this.UnregisterForPushNotificationsAsync);
        }

        private async Task UnregisterForPushNotificationsAsync()
        {
            try
            {
                await this.firebasePushNotification.UnregisterForPushNotificationsAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UnregisterForPushNotificationsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Unregister from push notifications failed with exception", "OK");
            }
            finally
            {
                this.UpdateToken();
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

        public bool IsSubscribedToEvents
        {
            get => this.isSubscribedToEvents;
            set
            {
                if (this.SetProperty(ref this.isSubscribedToEvents, value))
                {
                    this.SubscribeEventsCommand.NotifyCanExecuteChanged();
                    this.UnsubscribeEventsCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public IAsyncRelayCommand SubscribeEventsCommand
        {
            get =>
                this.subscribeEventsCommand ??= new AsyncRelayCommand(
                    execute: this.SubscribeEventsAsync,
                    canExecute: () => !this.IsSubscribedToEvents);
        }

        private async Task SubscribeEventsAsync()
        {
            try
            {
                this.firebasePushNotification.TokenRefreshed += this.OnTokenRefresh;
                this.firebasePushNotification.NotificationReceived += this.OnNotificationReceived;
                this.firebasePushNotification.NotificationOpened += this.OnNotificationOpened;
                this.firebasePushNotification.NotificationAction += this.OnNotificationAction;
                this.firebasePushNotification.NotificationDeleted += this.OnNotificationDeleted;

                this.IsSubscribedToEvents = true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "SubscribeEvents failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "SubscribeEvents failed with exception", "OK");
            }
        }

        public IAsyncRelayCommand UnsubscribeEventsCommand
        {
            get => this.unsubscribeEventsCommand ??= new AsyncRelayCommand(
                execute: this.UnsubscribeEventsAsync,
                canExecute: () => this.IsSubscribedToEvents);
        }

        private async Task UnsubscribeEventsAsync()
        {
            try
            {
                this.firebasePushNotification.TokenRefreshed -= this.OnTokenRefresh;
                this.firebasePushNotification.NotificationReceived -= this.OnNotificationReceived;
                this.firebasePushNotification.NotificationOpened -= this.OnNotificationOpened;
                this.firebasePushNotification.NotificationAction -= this.OnNotificationAction;
                this.firebasePushNotification.NotificationDeleted -= this.OnNotificationDeleted;

                this.IsSubscribedToEvents = false;
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

            this.UpdateSubscribedTopics();
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

        private async void OnNotificationAction(object sender, FirebasePushNotificationActionEventArgs e)
        {
            await WaitAsync();
            await this.dialogService.ShowDialogAsync("OnNotificationAction", e.ToString(), "OK");
        }

        private async void OnNotificationDeleted(object sender, FirebasePushNotificationDataEventArgs e)
        {
            await WaitAsync();
            await this.dialogService.ShowDialogAsync("OnNotificationDeleted", e.ToString(), "OK");
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

        public ICommand ShareTokenCommand
        {
            get => this.shareTokenCommand ??= new AsyncRelayCommand(this.ShareTokenAsync);
        }

        private async Task ShareTokenAsync()
        {
            try
            {
                var fileName = $"fcm_token_{this.appInfo.PackageName}_{DateTime.Now:yyyy-dd-MM_THH-mm-ss}.txt";
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName);
                await File.WriteAllTextAsync(path, this.Token);

                var shareFile = new ShareFile(path);
                var shareRequest = new ShareFileRequest { Title = $"FCM Token {this.appInfo.Name}", File = shareFile };
                await this.share.RequestAsync(shareRequest);

                File.Delete(path);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ShareTokenAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "ShareTokenAsync failed with exception", "OK");
            }
        }

        public ICommand GetTokenCommand
        {
            get => this.getTokenCommand ??= new AsyncRelayCommand(this.GetTokenAsync);
        }

        private async Task GetTokenAsync()
        {
            try
            {
                this.Token = null;
                this.Token = this.firebasePushNotification.Token;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "GetTokenAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Failed to get the token", "OK");
            }
        }

        public ICommand CopyTokenCommand
        {
            get => this.copyTokenCommand ??= new AsyncRelayCommand(this.CopyTokenAsync);
        }

        private async Task CopyTokenAsync()
        {
            try
            {
                await this.clipboard.SetTextAsync(this.Token);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "SetTextAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Failed to copy the token", "OK");
            }
        }

        public ICommand GetNotificationChannelGroupsCommand
        {
            get => this.getNotificationChannelGroupsCommand ??= new AsyncRelayCommand(this.GetNotificationChannelGroupsAsync);
        }

        private async Task GetNotificationChannelGroupsAsync()
        {
            try
            {
                this.UpdateNotificationChannelGroups();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "GetNotificationChannelGroupsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Get notification channel groups failed with exception", "OK");
            }
        }

        public string[] ChannelGroups
        {
            get => this.channelGroups;
            private set => this.SetProperty(ref this.channelGroups, value);
        }

        public ICommand CreateNotificationChannelGroupsCommand
        {
            get => this.createNotificationChannelGroupsCommand ??= new AsyncRelayCommand(this.CreateNotificationChannelGroupsAsync);
        }

        private async Task CreateNotificationChannelGroupsAsync()
        {
            try
            {
#if ANDROID
                var notificationChannelGroupRequests = NotificationChannelGroupSamples.GetAll().ToArray();
                this.notificationChannels.CreateNotificationChannelGroups(notificationChannelGroupRequests);
                this.UpdateNotificationChannelGroups();
#endif
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "CreateNotificationChannelGroupsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Create notification channel groups failed with exception", "OK");
            }
        }

        public ICommand DeleteNotificationChannelGroupsCommand
        {
            get => this.deleteNotificationChannelGroupsCommand ??= new AsyncRelayCommand(this.DeleteNotificationChannelGroupsAsync);
        }

        private async Task DeleteNotificationChannelGroupsAsync()
        {
            try
            {
#if ANDROID
                this.notificationChannels.DeleteAllNotificationChannelGroups();
                this.UpdateNotificationChannelGroups();
                this.UpdateNotificationChannels();
#endif
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "DeleteNotificationChannelGroupsAsync failed with exception");
                await this.dialogService.ShowDialogAsync(
                    "Error",
                    $"Delete notification channel groups failed with exception: {ex.Message}",
                    "OK");
            }
        }

        public NotificationChannelViewModel[] Channels
        {
            get => this.channels;
            private set => this.SetProperty(ref this.channels, value);
        }

        public ICommand OpenNotificationSettingsCommand
        {
            get => this.openNotificationSettingsCommand ??= new AsyncRelayCommand(this.OpenNotificationSettingsAsync);
        }

        private async Task OpenNotificationSettingsAsync()
        {
            try
            {
#if ANDROID
                this.notificationChannels.OpenNotificationSettings();
#endif
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "OpenNotificationSettingsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Open notification settings failed with exception", "OK");
            }
        }

        public ICommand OpenNotificationChannelSettingsCommand
        {
            get => this.openNotificationChannelSettingsCommand ??= new AsyncRelayCommand(this.OpenNotificationChannelSettingsAsync);
        }

        private async Task OpenNotificationChannelSettingsAsync()
        {
            try
            {
#if ANDROID
                var defaultNotificationChannel = this.notificationChannels.Channels.GetDefault();
                if (defaultNotificationChannel != null)
                {
                    this.notificationChannels.OpenNotificationChannelSettings(defaultNotificationChannel.Id);
                }
#endif
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "OpenNotificationChannelSettingsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Open notification channel settings failed with exception", "OK");
            }
        }

#if ANDROID
        public string DefaultNotificationChannelId
        {
            get => this.defaultNotificationChannelId;
            set
            {
                if (this.SetProperty(ref this.defaultNotificationChannelId, value))
                {
                    this.notificationChannels.Channels.DefaultNotificationChannelId = value;
                }
            }
#else
        public string DefaultNotificationChannelId
        {
            get => null;
            set { }
#endif
        }

        public ICommand GetNotificationChannelsCommand
        {
            get => this.getNotificationChannelsCommand ??= new AsyncRelayCommand(this.GetNotificationChannelsAsync);
        }

        private async Task GetNotificationChannelsAsync()
        {
            try
            {
                this.UpdateNotificationChannels();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "GetNotificationChannelsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Get notification channels failed with exception", "OK");
            }
        }

        public ICommand DeleteNotificationChannelsCommand
        {
            get => this.deleteNotificationChannelsCommand ??= new AsyncRelayCommand(this.DeleteNotificationChannelsAsync);
        }

        private async Task DeleteNotificationChannelsAsync()
        {
            try
            {
#if ANDROID
                this.notificationChannels.DeleteAllNotificationChannels();
                this.UpdateNotificationChannels();
#endif
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "DeleteNotificationChannelsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Delete notification channels failed with exception", "OK");
            }
        }

        public ICommand SetNotificationChannelsCommand
        {
            get => this.setNotificationChannelsCommand ??= new AsyncRelayCommand(this.SetNotificationChannelsAsync);
        }

        private async Task SetNotificationChannelsAsync()
        {
            try
            {
#if ANDROID
                var notificationChannelRequests = NotificationChannelSamples.GetAll().ToArray();
                // var notificationChannelRequests = Array.Empty<NotificationChannelRequest>();
                this.notificationChannels.SetNotificationChannels(notificationChannelRequests);
                this.UpdateNotificationChannels();
#endif
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "SetNotificationChannelsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", $"Set notification channels failed with exception: {ex.Message}",
                    "OK");
            }
        }

        public ICommand CreateNotificationChannelsCommand
        {
            get => this.createNotificationChannelsCommand ??= new AsyncRelayCommand(this.CreateNotificationChannelsAsync);
        }

        private async Task CreateNotificationChannelsAsync()
        {
            try
            {
#if ANDROID
                var notificationChannelRequests = NotificationChannelSamples.GetAll().ToArray();
                this.notificationChannels.CreateNotificationChannels(notificationChannelRequests);
                this.UpdateNotificationChannels();
#endif
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "CreateNotificationChannelsAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", $"Create notification channels failed with exception: {ex.Message}",
                    "OK");
            }
        }

        private void UpdateNotificationChannels()
        {
#if ANDROID
            var notificationChannelViewModelLogger = this.loggerFactory.CreateLogger<NotificationChannelViewModel>();
            this.Channels = this.notificationChannels.Channels
                .Select(c =>
                {
                    void DeleteNotificationChannel(string id)
                    {
                        this.notificationChannels.DeleteNotificationChannel(id);
                        this.UpdateNotificationChannels();
                    }

                    return new NotificationChannelViewModel(
                        notificationChannelViewModelLogger,
                        this.dialogService,
                        this.notificationChannels,
                        DeleteNotificationChannel,
                        c);
                })
                .ToArray();

            this.DefaultNotificationChannelId = this.notificationChannels.Channels.DefaultNotificationChannelId;
#endif
        }

        private void UpdateNotificationChannelGroups()
        {
#if ANDROID
            this.ChannelGroups = this.notificationChannels.ChannelGroups
                .Select(g => g.Id)
                .ToArray();
#endif
        }

        public SubscribedTopicViewModel[] SubscribedTopics
        {
            get => this.subscribedTopics;
            private set => this.SetProperty(ref this.subscribedTopics, value);
        }

        public ICommand GetSubscribedTopicsCommand
        {
            get => this.getSubscribedTopicsCommand ??= new AsyncRelayCommand(this.GetSubscribedTopicsAsync);
        }

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
                .Select(t => new SubscribedTopicViewModel(t, this.UnsubscribeFromTopicAsync))
                .ToArray();
        }

        public string Topic
        {
            get => this.topic;
            set => this.SetProperty(ref this.topic, value);
        }

        public ICommand SubscribeToTopicCommand
        {
            get => this.subscribeToTopicCommand ??= new AsyncRelayCommand(this.SubscribeToTopicAsync);
        }

        private async Task SubscribeToTopicAsync()
        {
            try
            {
                var topic = this.Topic;
                await this.firebasePushNotification.SubscribeTopicAsync(topic);
                this.UpdateSubscribedTopics();
                this.Topic = null;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "SubscribeToTopicAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Subscribe to topic failed with exception", "OK");
            }
        }

        private async Task UnsubscribeFromTopicAsync(string topic)
        {
            try
            {
                await this.firebasePushNotification.UnsubscribeTopicAsync(topic);
                this.UpdateSubscribedTopics();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UnsubscribeFromTopicAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Unsubscribe from topic failed with exception", "OK");
            }
        }

        public ICommand UnsubscribeAllTopicsCommand
        {
            get => this.unsubscribeAllTopicsCommand ??= new AsyncRelayCommand(this.UnsubscribeAllTopicsAsync);
        }

        private async Task UnsubscribeAllTopicsAsync()
        {
            try
            {
                await this.firebasePushNotification.UnsubscribeAllTopicsAsync();
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

        public ICommand GetNotificationCategoriesCommand
        {
            get => this.getNotificationCategoriesCommand ??= new AsyncRelayCommand(this.GetNotificationCategoriesAsync);
        }

        private async Task GetNotificationCategoriesAsync()
        {
            try
            {
                var notificationCategories = this.firebasePushNotification.NotificationCategories;
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

        public ICommand RegisterNotificationCategoriesCommand => this.registerNotificationCategoriesCommand ??=
            new AsyncRelayCommand(this.RegisterNotificationCategoriesAsync);

        private async Task RegisterNotificationCategoriesAsync()
        {
            try
            {
                var categories = NotificationCategorySamples.GetAll().ToArray();
                this.firebasePushNotification.RegisterNotificationCategories(categories);

                await this.GetNotificationCategoriesAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "RegisterNotificationCategoriesAsync failed with exception");
                await this.dialogService.ShowDialogAsync("Error", "Registration of notification categories failed with exception", "OK");
            }
        }

        public ICommand ClearNotificationCategoriesCommand
        {
            get => this.clearNotificationCategoriesCommand ??= new AsyncRelayCommand(this.ClearNotificationCategoriesAsync);
        }

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

#if IOS
        public UNNotificationPresentationOptions[] PresentationOptions
        {
            get => this.presentationOptions;
            private set
            {
                if (this.SetProperty(ref this.presentationOptions, value))
                {
                    this.OnPropertyChanged(nameof(this.SelectedPresentationOptions));
                }
            }
        }

        public UNNotificationPresentationOptions SelectedPresentationOptions
        {
            get => this.firebasePushNotificationOptions.iOS.PresentationOptions;
            set => this.firebasePushNotificationOptions.iOS.PresentationOptions = value;
        }
#else
        public string SelectedPresentationOptions { get; set; }

        public string[] PresentationOptions { get; set; }
#endif

        public ICommand NavigateToQueuesPageCommand
        {
            get => this.navigateToQueuesPageCommand ??= new AsyncRelayCommand(this.NavigateToQueuesPageAsync);
        }

        private async Task NavigateToQueuesPageAsync()
        {
            await this.navigationService.PushAsync<QueuesPage>();
        }

        public ICommand NavigateToLogPageCommand
        {
            get => this.navigateToLogPageCommand ??= new AsyncRelayCommand(this.NavigateToLogPageAsync);
        }

        private async Task NavigateToLogPageAsync()
        {
            await this.navigationService.PushAsync<LogPage>();
        }

        public ICommand CapturePhotoCommand
        {
            get => this.capturePhotoCommand ??= new AsyncRelayCommand(this.CapturePhotoAsync);
        }

        private async Task CapturePhotoAsync()
        {
            try
            {
                var result = await MediaPicker.Default.CapturePhotoAsync().ConfigureAwait(true);
                if (result != null)
                {
                    await this.dialogService.ShowDialogAsync("CapturePhotoAsync", "Success", "OK");
                }
            }
            catch (Exception)
            {
                await this.dialogService.ShowDialogAsync("CapturePhotoAsync", "Cancelled", "OK");
            }
        }

        public IAsyncRelayCommand<string> OpenUrlCommand
        {
            get => this.openUrlCommand ??= new AsyncRelayCommand<string>(this.OpenUrlAsync);
        }

        private async Task OpenUrlAsync(string url)
        {
            try
            {
                await this.browser.OpenAsync(url);
            }
            catch
            {
                // Ignore exceptions
            }
        }

        public async void OnResume()
        {
            await this.UpdateAuthorizationStatusAsync();
            await this.GetNotificationChannelsAsync();
        }
    }
}