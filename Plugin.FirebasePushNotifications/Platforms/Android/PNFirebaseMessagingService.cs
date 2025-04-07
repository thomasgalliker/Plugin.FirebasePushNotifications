using Android.App;
using Android.Content;
using Android.OS;
using Firebase.Messaging;
using Microsoft.Extensions.Logging;

namespace Plugin.FirebasePushNotifications.Platforms
{
    /// <summary>
    /// Original source:
    /// https://github.com/firebase/firebase-android-sdk/blob/main/firebase-messaging/src/main/java/com/google/firebase/messaging/FirebaseMessagingService.java
    /// </summary>
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PNFirebaseMessagingService : FirebaseMessagingService // EnhancedIntentService
    {
        private const string ActionRemoteIntent = "com.google.android.c2dm.intent.RECEIVE";
        private const string ActionNewToken = "com.google.firebase.messaging.NEW_TOKEN";
        private const string ExtraToken = "token";

        private readonly ILogger logger;
        private readonly IFirebasePushNotification firebasePushNotification;
        private readonly INotificationBuilder notificationBuilder;

        public PNFirebaseMessagingService()
        {
            this.logger = IPlatformApplication.Current.Services.GetService<ILogger<PNFirebaseMessagingService>>();

            this.firebasePushNotification = IFirebasePushNotification.Current;
            this.notificationBuilder = IPlatformApplication.Current.Services.GetService<INotificationBuilder>();
        }

        public override void HandleIntent(Intent intent)
        {
            var action = intent.Action;
            this.logger.LogDebug($"HandleIntent: Action={action}");

            // HandleIntent calls OnMessageReceived because - for some reason - plain notification messages
            // are not forwarded to OnMessageReceived. Only data messages arrive in OnMessageReceived which makes it impossible to
            // send a notification message with click_action/category content.

            try
            {
                if (action == ActionRemoteIntent || action == ActionDirectBootRemoteIntent)
                {
                    this.HandleMessageIntent(intent);
                }
                else if (action == ActionNewToken)
                {
                    this.HandleTokenIntent(intent);
                }
                else
                {
                    this.logger.LogDebug($"HandleIntent: Unknown intent action '{intent.Action}'");
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "HandleIntent failed with exception");
            }
        }

        private void HandleMessageIntent(Intent intent)
        {
            // TODO: Implement same functionality as in original FirebaseMessagingService.java
            // var messageId = intent.GetStringExtra(Firebase.Messaging.Constants.MessagePayloadKeys.Msgid);
            // if (!alreadyReceivedMessage(messageId))
            {
                this.PassMessageIntentToSdk(intent);
            }
        }

        private void PassMessageIntentToSdk(Intent intent)
        {
            var messageType = intent.GetStringExtra(Firebase.Messaging.Constants.MessagePayloadKeys.MessageType);
            if (messageType == null)
            {
                messageType = Firebase.Messaging.Constants.MessageTypes.Message;
            }

            switch (messageType)
            {
                case Firebase.Messaging.Constants.MessageTypes.Message:
                    MessagingAnalytics.LogNotificationReceived(intent);
                    this.DispatchMessage(intent);
                    break;
                case Firebase.Messaging.Constants.MessageTypes.Deleted:
                    // TODO: Implement same functionality as in original FirebaseMessagingService.java
                    // onDeletedMessages();
                    break;
                case Firebase.Messaging.Constants.MessageTypes.SendEvent:
                    // TODO: Implement same functionality as in original FirebaseMessagingService.java
                    // onMessageSent(intent.GetStringExtra(Firebase.Messaging.Constants.MessagePayloadKeys.Msgid));
                    break;
                case Firebase.Messaging.Constants.MessageTypes.SendError:
                    // TODO: Implement same functionality as in original FirebaseMessagingService.java
                    // onSendError(
                    //     getMessageId(intent),
                    //     new SendException(intent.GetStringExtra(IPC_BUNDLE_KEY_SEND_ERROR)));
                    break;
                default:
                    this.logger.LogWarning($"PassMessageIntentToSdk: Received message with unknown type: {messageType}");
                    break;
            }
        }

        /// <summary>
        /// Dispatch a message to the app's onMessageReceived method, or show a notification
        /// </summary>
        private void DispatchMessage(Intent intent)
        {
            var extras = intent.Extras;
            if (extras == null)
            {
                // The intent should always have at least one extra so this shouldn't be null, but
                // this is the easiest way to handle the case where it does happen.
                extras = new Bundle();
            }

            // Remove any parameters that shouldn't be passed to the app.
            // The wakelock ID set by the WakefulBroadcastReceiver.
            extras.Remove("androidx.content.wakelockid");

            var data = intent.GetExtrasDict();
            data.Remove("com.google.firebase.iid.WakeLockHolder.wakefulintent");

            if (this.notificationBuilder.ShouldHandleNotificationReceived(data))
            {
                this.notificationBuilder.OnNotificationReceived(data);
            }
            else
            {
                this.firebasePushNotification.HandleNotificationReceived(data);
            }
        }

        // TODO: Check if this code is still needed or if it can be removed.
        private void OnMessageReceived(RemoteMessage remoteMessage)
        {
            this.logger.LogDebug("OnMessageReceived");

            // OnMessageReceived will be fired if a notification is received
            // while the Android app runs in foreground - OR - if the notification
            // only contains data payload.

            var data = new Dictionary<string, object>();
            var notification = remoteMessage.GetNotification();
            if (notification != null)
            {
                if (!string.IsNullOrEmpty(notification.Body))
                {
                    data.Add(Constants.NotificationBodyKey, notification.Body);
                }

                if (!string.IsNullOrEmpty(notification.BodyLocalizationKey))
                {
                    data.Add("body_loc_key", notification.BodyLocalizationKey);
                }

                var bodyLocArgs = notification.GetBodyLocalizationArgs();
                if (bodyLocArgs != null && bodyLocArgs.Any())
                {
                    data.Add("body_loc_args", bodyLocArgs);
                }

                if (!string.IsNullOrEmpty(notification.Title))
                {
                    data.Add(Constants.NotificationTitleKey, notification.Title);
                }

                if (!string.IsNullOrEmpty(notification.TitleLocalizationKey))
                {
                    data.Add("title_loc_key", notification.TitleLocalizationKey);
                }

                var titleLocArgs = notification.GetTitleLocalizationArgs();
                if (titleLocArgs != null && titleLocArgs.Any())
                {
                    data.Add("title_loc_args", titleLocArgs);
                }

                if (!string.IsNullOrEmpty(notification.Tag))
                {
                    data.Add(Constants.NotificationTagKey, notification.Tag);
                }

                if (!string.IsNullOrEmpty(notification.Sound))
                {
                    data.Add(Constants.SoundKey, notification.Sound);
                }

                if (!string.IsNullOrEmpty(notification.Icon))
                {
                    data.Add("icon", notification.Icon);
                }

                if (notification.Link != null)
                {
                    data.Add("link_path", notification.Link.Path);
                }

                if (!string.IsNullOrEmpty(notification.ClickAction))
                {
                    data.Add(Constants.ClickActionKey, notification.ClickAction);
                }

                if (!string.IsNullOrEmpty(notification.Color))
                {
                    data.Add("color", notification.Color);
                }
            }

            foreach (var d in remoteMessage.Data)
            {
                if (!data.ContainsKey(d.Key))
                {
                    data.Add(d.Key, d.Value);
                }
            }

            // Fix localization arguments parsing
            var localizationKeys = new[] { "title_loc_args", "body_loc_args" };
            foreach (var locKey in localizationKeys)
            {
                if (data.ContainsKey(locKey) && data[locKey] is string parameterValue)
                {
                    if (parameterValue.StartsWith("[") && parameterValue.EndsWith("]") && parameterValue.Length > 2)
                    {
                        var arrayValues = parameterValue[1..^1];
                        data[locKey] = arrayValues.Split(',').Select(t => t.Trim()).ToArray();
                    }
                    else
                    {
                        data[locKey] = Array.Empty<string>();
                    }
                }
            }

            this.firebasePushNotification.HandleNotificationReceived(data);
        }

        private void HandleTokenIntent(Intent intent)
        {
            var token = intent.GetStringExtra(ExtraToken);
            this.OnNewToken(token);
        }

        private void OnNewToken(string refreshedToken)
        {
            this.firebasePushNotification.HandleTokenRefresh(refreshedToken);
        }
    }
}