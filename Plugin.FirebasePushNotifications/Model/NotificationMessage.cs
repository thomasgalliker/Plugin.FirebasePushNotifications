using Newtonsoft.Json;
using Plugin.FirebasePushNotifications.Internals;
using Plugin.FirebasePushNotifications.Extensions;

namespace Plugin.FirebasePushNotifications.Model
{
    public class NotificationMessage : INotificationMessage
    {
        public NotificationMessage(
            string title,
            string body,
            IDictionary<string, string> data = null)
            : this(data)
        {
            this.Title = title;
            this.Body = body;
        }

        public NotificationMessage(IDictionary<string, string> data)
        {
            this.Data = data ?? new Dictionary<string, string>();
        }

        [JsonProperty(Constants.NotificationTitleKey)]
        public string Title
        {
            get => this.Data.GetValueOrDefault(Constants.NotificationTitleKey);
            set => this.Data[Constants.NotificationTitleKey] = value;
        }

        [JsonProperty(Constants.NotificationBodyKey)]
        public string Body
        {
            get => this.Data.GetValueOrDefault(Constants.NotificationBodyKey);
            set => this.Data[Constants.NotificationBodyKey] = value;
        }

        [JsonProperty(Constants.NotificationTagKey)]
        public string Tag
        {
            get => this.Data.GetValueOrDefault(Constants.NotificationTagKey);
            set => this.Data[Constants.NotificationTagKey] = value;
        }

        [JsonProperty(Constants.NotificationDataKey)]
        public IDictionary<string, string> Data { get; private set; } = new Dictionary<string, string>();

        public override string ToString()
        {
            var dict = DictionaryJsonConverter.Flatten(this.Data);
            return string.Join($",{Environment.NewLine}", dict.Select(d => $"{{{d.Key}, {d.Value ?? "null"}}}"));
        }
    }
}