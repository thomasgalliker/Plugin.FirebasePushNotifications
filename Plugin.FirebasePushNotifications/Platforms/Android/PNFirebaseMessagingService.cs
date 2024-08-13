using Android.App;
using Android.Content;
using Firebase.Messaging;
using Microsoft.Extensions.Logging;

namespace Plugin.FirebasePushNotifications.Platforms
{
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PNFirebaseMessagingService : FirebaseMessagingService
    {
        private readonly ILogger logger;
        private readonly INotificationBuilder notificationBuilder;

        public PNFirebaseMessagingService()
        {
            this.logger = IPlatformApplication.Current.Services.GetService<ILogger<PNFirebaseMessagingService>>();
            this.notificationBuilder = IPlatformApplication.Current.Services.GetService<INotificationBuilder>();
        }

        public override void HandleIntent(Intent intent)
        {
            this.logger.LogDebug("HandleIntent");

            // HandleIntent calls OnMessageReceived because - for some reason - plain notification messages
            // are not forwarded to OnMessageReceived. Only data messages arrive in OnMessageReceived which makes it impossible to
            // send a notification message with click_action/category content.

            // TODO: HandleIntent seems no longer used in firebase 11.8.0 and later.

            try
            {
                if (intent.Extras != null)
                {
                    var data = intent.GetExtrasDict();
                    if (this.notificationBuilder.ShouldHandleNotificationReceived(data))
                    {
                        this.notificationBuilder.OnNotificationReceived(data);
                    }
                    else
                    {
                        base.HandleIntent(intent);
                    }
                }
                else
                {
                    base.HandleIntent(intent);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "HandleIntent failed with exception");
                base.HandleIntent(intent);
            }
        }

        public override void OnMessageReceived(RemoteMessage remoteMessage)
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

            var firebasePushNotification = CrossFirebasePushNotification.Current;
            firebasePushNotification.HandleNotificationReceived(data);
        }

        public override void OnNewToken(string refreshedToken)
        {
            var firebasePushNotification = CrossFirebasePushNotification.Current;
            firebasePushNotification.HandleTokenRefresh(refreshedToken);
        }
    }
}