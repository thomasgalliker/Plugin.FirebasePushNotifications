using Android.Content;

namespace Plugin.FirebasePushNotifications.Platforms
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class PushNotificationDeletedReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var extras = intent.GetExtrasDict();
            CrossFirebasePushNotification.Current.HandleNotificationDeleted(extras);
        }
    }
}
