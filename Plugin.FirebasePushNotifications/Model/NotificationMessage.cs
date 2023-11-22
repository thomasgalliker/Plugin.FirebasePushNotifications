using Newtonsoft.Json;
using Plugin.FirebasePushNotifications.Internals;

namespace Plugin.FirebasePushNotifications.Model
{
    public class NotificationMessage : INotificationMessage
    {
        private readonly string body;
        private readonly string title;
        private string tag;

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

        [JsonProperty(Constants.NotificationTitleKey)]
        public string Title
        {
            get => this.title ?? this.Data?[Constants.NotificationTitleKey];
        }

        [JsonProperty(Constants.NotificationBodyKey)]
        public string Body
        {
            get => this.body ?? this.Data?[Constants.NotificationBodyKey];
        }
        
        [JsonProperty(Constants.NotificationTagKey)]
        public string Tag
        {
            get => this.tag ?? this.Data?[Constants.NotificationTagKey];
        }

        [JsonProperty(Constants.NotificationDataKey)]
        public IDictionary<string, string> Data { get; }

        public override string ToString()
        {
            var dict = DictionaryJsonConverter.Flatten(this.Data);
            return string.Join($",{Environment.NewLine}", dict.Select(d => $"{{{d.Key}, {d.Value ?? "null"}}}"));
        }
    }
}