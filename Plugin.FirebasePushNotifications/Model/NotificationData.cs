using Plugin.FirebasePushNotifications.Internals;

namespace Plugin.FirebasePushNotifications.Model
{
    public sealed class NotificationData
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

        public override string ToString()
        {
            var dict = DictionaryJsonConverter.Flatten(this.Data);
            return string.Join($",{Environment.NewLine}", dict.Select(d => $"{{{d.Key}, {d.Value ?? "null"}}}"));
        }

        public string Body
        {
            get => this.body ?? this.Data?["body"];
        }

        public string Title
        {
            get => this.title ?? (this.Data != null && this.Data.Any() ? this.Data["title"] : "");
        }

        public IDictionary<string, string> Data { get; }
    }
}