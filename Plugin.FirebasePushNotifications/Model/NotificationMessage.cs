using Newtonsoft.Json;
using Plugin.FirebasePushNotifications.Internals;

namespace Plugin.FirebasePushNotifications.Model
{
    public class NotificationMessage : INotificationMessage
    {
        private readonly string body;
        private readonly string title;

        public NotificationMessage(
            string body,
            string title,
            IDictionary<string, string> data = null) 
            : this(data)
        {
            this.body = body;
            this.title = title;
        }

        public NotificationMessage(IDictionary<string, string> data)
        {
            this.Data = data;
        }

        [JsonProperty("title")]
        public string Title
        {
            get => this.title ?? this.Data?["title"];
        }

        [JsonProperty("body")]
        public string Body
        {
            get => this.body ?? this.Data?["body"];
        }

        public IDictionary<string, string> Data { get; }

        public override string ToString()
        {
            var dict = DictionaryJsonConverter.Flatten(this.Data);
            return string.Join($",{Environment.NewLine}", dict.Select(d => $"{{{d.Key}, {d.Value ?? "null"}}}"));
        }
    }
}