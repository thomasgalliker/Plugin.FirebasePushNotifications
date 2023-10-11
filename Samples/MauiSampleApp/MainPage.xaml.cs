using Plugin.FirebasePushNotifications;

namespace MauiSampleApp
{
    public partial class MainPage : ContentPage
    {
        private int count = 0;

        public MainPage()
        {
            this.InitializeComponent();

            var firebasePushNotification = CrossFirebasePushNotification.Current;
            firebasePushNotification.OnTokenRefresh += this.OnTokenRefresh;
            firebasePushNotification.OnNotificationReceived += this.OnNotificationReceived;

            firebasePushNotification.ClearAllNotifications();
        }

        private void OnNotificationReceived(object sender, FirebasePushNotificationDataEventArgs e)
        {
            this.DisplayAlert("FirebasePushNotification", "OnNotificationReceived", "OK");
        }

        private void OnTokenRefresh(object sender, FirebasePushNotificationTokenEventArgs e)
        {
            this.DisplayAlert("FirebasePushNotification", "OnTokenRefresh", "OK");
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            this.count++;

            if (this.count == 1)
            {
                this.CounterBtn.Text = $"Clicked {this.count} time";
            }
            else
            {
                this.CounterBtn.Text = $"Clicked {this.count} times";
            }
        }
    }
}