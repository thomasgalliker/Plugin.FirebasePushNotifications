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
        public static IEnumerable<(string Key, object Value)> GetExtras(this Intent intent)
        {
            if (intent == null)
            {
                yield break;
            }

            var extras = intent.Extras;
            if (extras == null || extras.IsEmpty)
            {
                yield break;
            }

            foreach (var key in extras.KeySet())
            {
                var value = extras.Get(key);
                yield return (key, value);
            }
        }

        public static IDictionary<string, object> GetExtrasDict(this Intent intent)
        {
            return intent.GetExtras().ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
