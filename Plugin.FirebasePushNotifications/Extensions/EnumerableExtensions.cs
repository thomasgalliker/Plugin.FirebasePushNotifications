namespace Plugin.FirebasePushNotifications.Extensions
{
    internal static class EnumerableExtensions
    {
        internal static string GetValue(this IEnumerable<(string Key, string Value)> items, string key, string defaultValue = default)
        {
            if (items.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        internal static int GetInt(this IEnumerable<(string Key, string Value)> items, string key, int defaultValue = default)
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
        internal static bool TryGetValue(this IEnumerable<(string Key, string Value)> items, string key, out string value)
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

        internal static bool TryGetInt(this IEnumerable<(string Key, string Value)> items, string key, out int value)
        {
            if (items.TryGetValue(key, out var o) && int.TryParse(o, out var integerValue))
            {
                value = integerValue;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Splits <paramref name="source"/> using the condition given in <paramref name="predicate"/> into two sub-lists.
        /// </summary>
        internal static (IEnumerable<T> matches, IEnumerable<T> nonMatches) Fork<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            var groupedByMatching = source.ToLookup(predicate);
            return (groupedByMatching[true], groupedByMatching[false]);
        }

        /// <summary>
        /// Creates two <see cref="T:T[]"/> from the two <see cref="IEnumerable{T}"/> in <paramref name="source"/>.
        /// </summary>
        internal static (T[] Items1, T[] Items2) ToArray<T>(this (IEnumerable<T> Items1, IEnumerable<T> Items2) source)
        {
            return (source.Items1.ToArray(), source.Items2.ToArray());
        }
    }
}
