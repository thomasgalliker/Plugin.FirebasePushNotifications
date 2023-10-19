namespace Plugin.FirebasePushNotifications.Extensions
{
    internal static class DictionaryExtensions
    {
        public static T GetValueOrDefault<T>(this IDictionary<string, object> items, string key, T defaultValue = default)
        {
            if (items.TryGetValue(key, out var value))
            {
                return (T)value;
            }

            return defaultValue;
        }

        public static string GetStringOrDefault(this IDictionary<string, object> items, string key, string defaultValue = default)
        {
            return items.GetValueOrDefault(key, defaultValue);
        }

        public static bool TryGetInt(this IDictionary<string, object> items, string key, out int value)
        {
            if (items.TryGetValue(key, out var item) && int.TryParse($"{item}", out var integerValue))
            {
                value = integerValue;
                return true;
            }

            value = default;
            return false;
        }
        
        public static bool TryGetString(this IDictionary<string, object> items, string key, out string value)
        {
            if (items.TryGetValue(key, out var item) && item is string stringValue)
            {
                value = stringValue;
                return true;
            }

            value = default;
            return false;
        }
    }
}
