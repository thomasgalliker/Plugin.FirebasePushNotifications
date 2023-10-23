using Plugin.FirebasePushNotifications.Internals;

namespace Plugin.FirebasePushNotifications.Model
{
    public class NotificationData : INotificationData
    {
        private readonly string body;
        private readonly string title;

        public NotificationData(
            string body = null,
            string title = null,
            IDictionary<string, string> data = null)
        {
            this.body = body;
            this.title = title;
            this.Data = data;
        }

        public string Title
        {
            get => this.title ?? this.Data?["title"];
        }

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