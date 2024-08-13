using System.Diagnostics;
using Newtonsoft.Json;

namespace Plugin.FirebasePushNotifications
{
    [DebuggerDisplay("{Id}")]
    public class NotificationAction
    {
        [JsonConstructor]
        public NotificationAction(
            [JsonProperty("id")] string id,
            [JsonProperty("title")] string title,
            [JsonProperty("type")] NotificationActionType notificationActionType,
            [JsonProperty("icon")] string icon)
        {
            this.Id = id ?? throw new ArgumentNullException(nameof(id));
            this.Title = title ?? throw new ArgumentNullException(nameof(title));
            this.Type = notificationActionType;
            this.Icon = icon;
        }

        public NotificationAction(string id, string title, NotificationActionType notificationActionType)
            : this(id, title, notificationActionType, null)
        {
        }

        [JsonProperty("id")]
        public string Id { get; }

        [JsonProperty("title")]
        public string Title { get; }

        [JsonProperty("type")]
        public NotificationActionType Type { get; }

        [JsonProperty("icon")]
        public string Icon { get; }

        public override string ToString()
        {
            return this.Id;
        }
    }
}
