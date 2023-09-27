using Plugin.FirebasePushNotifications;

namespace MauiSampleApp
{
    public partial class MainPage : ContentPage
    {
        private int count = 0;

        public MainPage()
        {
            this.InitializeComponent();

            var isSupported = CrossFirebasePushNotification.IsSupported;

            var firebasePushNotification = CrossFirebasePushNotification.Current;
            firebasePushNotification.ClearAllNotifications();

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

            SemanticScreenReader.Announce(this.CounterBtn.Text);
        }
    }
}