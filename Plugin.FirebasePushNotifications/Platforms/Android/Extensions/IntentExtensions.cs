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
