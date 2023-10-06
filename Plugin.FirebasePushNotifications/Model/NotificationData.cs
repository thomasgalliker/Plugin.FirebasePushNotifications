using Newtonsoft.Json;
using Plugin.FirebasePushNotifications.Internals;

namespace Plugin.FirebasePushNotifications.Model
{
    public class NotificationData
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        public string ToString()
        {
            var dict = DictionaryJsonConverter.Flatten(this);
            return string.Join($",{Environment.NewLine}", dict.Select(d => $"{{{d.Key}, {d.Value ?? "null"}}}"));
        }
    }
}