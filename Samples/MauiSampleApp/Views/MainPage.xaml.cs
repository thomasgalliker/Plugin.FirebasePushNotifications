using MauiSampleApp.ViewModels;
using Plugin.FirebasePushNotifications;

namespace MauiSampleApp.Views
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel mainViewModel)
        {
            this.InitializeComponent();
            this.BindingContext = mainViewModel;

            // For those who prefer to use code-behind instead of MVVM,
            // and don't want to use dependency injection, uncomment the following lines of code
            // to see how such code would work:

            //IFirebasePushNotification.Current.TokenRefreshed += this.OnTokenRefresh;
            //IFirebasePushNotification.Current.NotificationOpened += this.OnNotificationOpened;
            //IFirebasePushNotification.Current.NotificationReceived += this.OnNotificationReceived;
            //IFirebasePushNotification.Current.NotificationDeleted += this.OnNotificationDeleted;
            //IFirebasePushNotification.Current.NotificationAction += this.OnNotificationAction;

            //var notificationCategories = new NotificationCategory[]
            //{
            //    new NotificationCategory("dismiss", new []
            //    {
            //        new NotificationAction("Dismiss","Dismiss", NotificationActionType.Default),
            //    }),
            //    new NotificationCategory("navigate", new []
            //    {
            //        new NotificationAction("Dismiss","Dismiss", NotificationActionType.Default),
            //        new NotificationAction("Navigate","Navigate To", NotificationActionType.Foreground)
            //    })
            //};

            //IFirebasePushNotification.Current.RegisterNotificationCategories(notificationCategories);

            //_ = IFirebasePushNotification.Current.RegisterForPushNotificationsAsync();
        }
    }
}