namespace Plugin.FirebasePushNotifications.Extensions
{
    public static class EventArgsExtensions
    {
        public static string ToDebugString(this FirebasePushNotificationDataEventArgs p)
        {
            return $"Data=[{p.Data.ToDebugString()}]";
        }

        public static string ToDebugString(this FirebasePushNotificationResponseEventArgs p)
        {
            return $"Identifier={p.Identifier ?? "null"}, Data=[{p.Data.ToDebugString()}]";
        }

        public static string ToDebugString(this FirebasePushNotificationErrorEventArgs p)
        {
            return $"Type={p.Type}, Message={p.Message ?? "null"}";
        }

        public static string ToDebugString(this FirebasePushNotificationTokenEventArgs p)
        {
            return $"Token={p.Token ?? "null"}";
        }

        public static string ToDebugString<T>(this IDictionary<string, T> data)
        {
            return string.Join(",", data.Select(d => $"{{{d.Key}={d.Value?.ToString() ?? "null"}}}"));
        }
    }
}