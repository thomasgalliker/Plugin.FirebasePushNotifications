using Android.App;
using Android.Content;
using Plugin.FirebasePushNotifications.Extensions;

namespace Plugin.FirebasePushNotifications.Platforms
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class PushNotificationActionReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var extras = intent.GetExtrasDict();

            var identifier = extras.GetStringOrDefault(DefaultPushNotificationHandler.ActionIdentifierKey);
            var notificationCategoryType = NotificationCategoryType.Default;

            CrossFirebasePushNotification.Current.HandleNotificationAction(extras, identifier, notificationCategoryType);

            var manager = context.GetSystemService(Context.NotificationService) as NotificationManager;
            var notificationId = extras.GetValueOrDefault(DefaultPushNotificationHandler.ActionNotificationIdKey, -1);
            if (notificationId != -1)
            {
                var notificationTag = extras.GetValueOrDefault(DefaultPushNotificationHandler.ActionNotificationTagKey, string.Empty);

                if (notificationTag == null)
                {
                    manager.Cancel(notificationId);
                }
                else
                {
                    manager.Cancel(notificationTag, notificationId);
                }
            }
        }
    }
}
