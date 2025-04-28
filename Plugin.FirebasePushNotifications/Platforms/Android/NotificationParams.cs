using Plugin.FirebasePushNotifications.Extensions;
using MessageNotificationKeys = Firebase.Messaging.Constants.MessageNotificationKeys;

namespace Plugin.FirebasePushNotifications.Platforms
{
    /// <summary>
    /// Original source: https://github.com/firebase/firebase-android-sdk/blob/main/firebase-messaging/src/main/java/com/google/firebase/messaging/NotificationParams.java
    /// </summary>
    internal class NotificationParams
    {
        private readonly IDictionary<string, object> data;

        public NotificationParams(IDictionary<string, object> data)
        {
            this.data = data;
            this.data = data;
        }

        public bool NoUI
        {
            get
            {
                this.data.TryGetBool(MessageNotificationKeys.NoUi, out var noUi);
                return noUi;
            }
        }

        public bool IsNotification
        {
            get
            {
                if (!this.data.TryGetBool(MessageNotificationKeys.EnableNotification, out var isNotification))
                {
                    this.data.TryGetBool(KeyWithOldPrefix(MessageNotificationKeys.EnableNotification), out isNotification);
                }

                return isNotification;
            }
        }

        public bool Silent
        {
            get
            {
                if (this.data.TryGetBool(Constants.SilentKey, out var silentValue) && silentValue)
                {
                    return true;
                }

                return false;
            }
        }

        private static string KeyWithOldPrefix(string key)
        {
            if (!key.StartsWith(MessageNotificationKeys.NotificationPrefix))
            {
                return key;
            }

            return key.Replace(
                MessageNotificationKeys.NotificationPrefix,
                MessageNotificationKeys.NotificationPrefixOld);
        }
    }
}