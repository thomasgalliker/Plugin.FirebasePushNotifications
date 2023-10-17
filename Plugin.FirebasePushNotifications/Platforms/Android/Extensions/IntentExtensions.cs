using Android.Content;

namespace Plugin.FirebasePushNotifications.Platforms
{
    internal static class IntentExtensions
    {
        public static void PutExtras(this Intent intent, IDictionary<string, string> extras)
        {
            foreach (var item in extras)
            {
                intent.PutExtra(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Returns a list of <c>(string Key, string Value)</c> tuples from the given <paramref name="intent"/>.
        /// </summary>
        /// <param name="intent">The intent.</param>
        /// <returns>List of key/value pairs. If intent is null, an empty list is returned.</returns>
        public static IEnumerable<(string Key, string Value)> GetExtras(this Intent intent)
        {
            if (intent?.Extras == null)
            {
                yield break;
            }

            foreach (var key in intent.Extras.KeySet())
            {
                var value = intent.Extras.Get(key);
                yield return (key, $"{value}");
            }
        }
    }
}
