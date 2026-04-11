using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Plugin.FirebasePushNotifications
{
    [DebuggerDisplay("{Id}")]
    public class NotificationAction
    {
        [JsonConstructor]
        public NotificationAction(
            string id,
            string title,
            NotificationActionType type,
            string icon)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this.Title = title ?? throw new ArgumentNullException(nameof(title));
            this.Type = type;
            this.Icon = icon;
        }

        public NotificationAction(string id, string title, NotificationActionType notificationActionType)
            : this(id, title, notificationActionType, null)
        {
        }

        [JsonPropertyName("id")]
        public string Id { get; }

        [JsonPropertyName("title")]
        public string Title { get; }

        [JsonPropertyName("type")]
        public NotificationActionType Type { get; }

        [JsonPropertyName("icon")]
        public string Icon { get; }

        public override string ToString()
        {
            return this.Id;
        }
    }
}
