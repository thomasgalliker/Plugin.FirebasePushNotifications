using Android.App;
using Android.Content;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Extensions;

namespace Plugin.FirebasePushNotifications.Platforms
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class PushNotificationActionReceiver : BroadcastReceiver
    {
        private readonly ILogger logger;

        public PushNotificationActionReceiver()
        {
            this.logger = IPlatformApplication.Current.Services.GetService<ILogger<PushNotificationActionReceiver>>();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            this.logger.LogDebug("OnReceive");

            try
            {
                var extras = intent.GetExtrasDict();
                var notificationCategoryId = extras.GetStringOrDefault(Constants.NotificationCategoryKey);
                var notificationActionId = extras.GetStringOrDefault(Constants.NotificationActionId);

                var firebasePushNotification = IFirebasePushNotification.Current;
                firebasePushNotification.HandleNotificationAction(extras, notificationCategoryId, notificationActionId, NotificationCategoryType.Default);

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
            catch (Exception ex)
            {
                this.logger.LogError(ex, "OnReceive failed with exception");
            }
        }
    }
}
