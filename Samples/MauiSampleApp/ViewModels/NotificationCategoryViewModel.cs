using Plugin.FirebasePushNotifications;

namespace MauiSampleApp.ViewModels
{
    public class NotificationCategoryViewModel
    {
        public NotificationCategoryViewModel(NotificationCategory notificationCategory)
        {
            this.Category = notificationCategory.CategoryId;
            this.Actions = string.Join(Environment.NewLine, notificationCategory.Actions.Select(a => $"> ActionIdentifier: {a.Id}"));
        }

        public string Category { get; }

        public string Actions { get; }
    }
}