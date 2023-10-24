using Android.App;
using Android.Content;
using Firebase.Messaging;

namespace Plugin.FirebasePushNotifications.Platforms
{
    [Service(Exported = false)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PNFirebaseMessagingService : FirebaseMessagingService
    {
        public override void OnMessageReceived(RemoteMessage message)
        {
            var data = new Dictionary<string, object>();
            var notification = message.GetNotification();
            if (notification != null)
            {
                if (!string.IsNullOrEmpty(notification.Body))
                {
                    data.Add("body", notification.Body);
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
                    data.Add("title", notification.Title);
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
                    data.Add("tag", notification.Tag);
                }

                if (!string.IsNullOrEmpty(notification.Sound))
                {
                    data.Add("sound", notification.Sound);
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
                    data.Add("click_action", notification.ClickAction);
                }

                if (!string.IsNullOrEmpty(notification.Color))
                {
                    data.Add("color", notification.Color);
                }
            }

            foreach (var d in message.Data)
            {
                if (!data.ContainsKey(d.Key))
                {
                    data.Add(d.Key, d.Value);
                }
            }

            //Fix localization arguments parsing
            var localizationKeys = new string[] { "title_loc_args", "body_loc_args" };
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

            CrossFirebasePushNotification.Current.HandleNotificationReceived(data);
        }

        public override void OnNewToken(string refreshedToken)
        {
            var firebasePushNotification = CrossFirebasePushNotification.Current;

            // TODO: Use existing code in FirebasePushNotificationManager!

            // Resubscribe to topics since the old instance id isn't valid anymore
            foreach (var topic in firebasePushNotification.SubscribedTopics)
            {
                FirebaseMessaging.Instance.SubscribeToTopic(topic);
            }

            // TODO: Very dangerous! GetSharedPreferences access everywhere!
            var editor = Android.App.Application.Context.GetSharedPreferences(Constants.KeyGroupName, FileCreationMode.Private).Edit();
            editor.PutString(Constants.FirebaseTokenKey, refreshedToken);
            editor.Commit();

            firebasePushNotification.HandleTokenRefresh(refreshedToken);
        }
    }

}
