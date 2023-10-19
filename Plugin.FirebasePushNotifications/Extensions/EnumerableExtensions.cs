namespace Plugin.FirebasePushNotifications.Extensions
{
    internal static class EnumerableExtensions
    {
        public static string GetValue(this IEnumerable<(string Key, string Value)> items, string key, string defaultValue = default)
        {
            if (items.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public static int GetInt(this IEnumerable<(string Key, string Value)> items, string key, int defaultValue = default)
        {
            if (items.TryGetInt(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Try to get the value by given <paramref name="key"/>.
        /// </summary>
        public static bool TryGetValue(this IEnumerable<(string Key, string Value)> items, string key, out string value)
        {
            var item = items.SingleOrDefault(x => x.Key == key);
            if (item != default)
            {
                value = item.Value;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetInt(this IEnumerable<(string Key, string Value)> items, string key, out int value)
        {
            if (items.TryGetValue(key, out var o) && int.TryParse(o, out var integerValue))
            {
                value = integerValue;
                return true;
            }

            value = default;
            return false;
        }

    }
}
