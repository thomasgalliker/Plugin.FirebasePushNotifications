using UserNotifications;

namespace Plugin.FirebasePushNotifications.Platforms
{
    internal sealed class UNUserNotificationCenterDelegateImpl : UNUserNotificationCenterDelegate
    {
        private readonly Action<UNUserNotificationCenter, UNNotificationResponse, Action> didReceiveNotificationResponse;
        private readonly Action<UNUserNotificationCenter, UNNotification, Action<UNNotificationPresentationOptions>> willPresentNotification;

        public UNUserNotificationCenterDelegateImpl(
            Action<UNUserNotificationCenter, UNNotificationResponse, Action> didReceiveNotificationResponse,
            Action<UNUserNotificationCenter, UNNotification, Action<UNNotificationPresentationOptions>> willPresentNotification
        )
        {
            this.didReceiveNotificationResponse = didReceiveNotificationResponse;
            this.willPresentNotification = willPresentNotification;
        }

        public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
        {
            this.didReceiveNotificationResponse(center, response, completionHandler);
        }

        public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            this.willPresentNotification(center, notification, completionHandler);
        }
    }
}
