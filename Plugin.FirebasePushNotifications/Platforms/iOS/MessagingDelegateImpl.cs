using Firebase.CloudMessaging;

namespace Plugin.FirebasePushNotifications.Platforms
{
    internal sealed class MessagingDelegateImpl : MessagingDelegate
    {
        private readonly Action<Messaging, string> didReceiveRegistrationToken;

        public MessagingDelegateImpl(Action<Messaging, string> didReceiveRegistrationToken)
        {
            this.didReceiveRegistrationToken = didReceiveRegistrationToken;
        }

        public override void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            this.didReceiveRegistrationToken(messaging, fcmToken);
        }
    }
}
