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

            var notificationActionId = extras.GetStringOrDefault(Constants.NotificationActionId);

            CrossFirebasePushNotification.Current.HandleNotificationAction(extras, notificationActionId, NotificationCategoryType.Default);

            var manager = context.GetSystemService(Context.NotificationService) as NotificationManager;
            var notificationId = extras.GetValueOrDefault(Constants.ActionNotificationIdKey, -1);
            if (notificationId != -1)
            {
                var notificationTag = extras.GetStringOrDefault(Constants.ActionNotificationTagKey, null);
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
