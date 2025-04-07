namespace Plugin.FirebasePushNotifications.Extensions
{
    public static class DictionaryExtensions
    {
        public static string ToDebugString<T>(this IDictionary<string, T> data)
        {
            return string.Join(",", data.Select(d => $"{{{d.Key}={d.Value?.ToString() ?? "null"}}}"));
        }

        public static T GetValueOrDefault<T>(this IDictionary<string, T> items, string key, T defaultValue = default)
        {
            if (items.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public static Ty GetValueOrDefault<Tx, Ty>(this IDictionary<string, Tx> items, string key, Ty defaultValue = default)
        {
            if (items.TryGetValue(key, out var value))
            {
                return (Ty)Convert.ChangeType(value, typeof(Ty));
            }

            return defaultValue;
        }

        public static string GetStringOrDefault(this IDictionary<string, object> items, string key, string defaultValue = default)
        {
            if (items.TryGetValue(key, out var value))
            {
                return value?.ToString();
            }

            return defaultValue;
        }

        public static bool TryGetInt(this IDictionary<string, object> items, string key, out int value)
        {
            if (items.TryGetValue(key, out var item) && (item is int integerValue || int.TryParse($"{item}", out integerValue)))
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

        public static bool TryGetBool(this IDictionary<string, object> items, string key, out bool value)
        {
            if (items.TryGetValue(key, out var item) && item is bool boolValue)
            {
                value = boolValue;
                return true;
            }

            if (items.TryGetString(key, out var stringValue))
            {
                if (stringValue.Equals("1", StringComparison.InvariantCultureIgnoreCase) ||
                    stringValue.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    value = true;
                    return true;
                }

                if (stringValue.Equals("0", StringComparison.InvariantCultureIgnoreCase) ||
                    stringValue.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                {
                    value = false;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
