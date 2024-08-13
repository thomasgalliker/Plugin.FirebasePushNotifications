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
        public static IEnumerable<(string Key, Java.Lang.Object Value)> GetExtras(this Intent intent)
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
            return intent.GetExtras()
                .ConvertToCLRObjects()
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private static IEnumerable<(string Key, object Value)> ConvertToCLRObjects(this IEnumerable<(string Key, Java.Lang.Object Value)> values)
        {
            foreach (var (key, value) in values)
            {
                yield return ConvertToCLRObject(key, value);
            }
        }

        private static (string Key, object Value) ConvertToCLRObject(string key, Java.Lang.Object value)
        {
            // Type mapping between Java and CLR objects:
            // https://j-integra.intrinsyc.com/support/net/doc/type_mapping.html
            // TODO: This could be improved by introducing a dictionary for lookup.

            switch (value)
            {
                case Java.Lang.String stringValue:
                    return (key, (string)stringValue);

                case Java.Lang.Boolean booleanValue:
                    return (key, (bool)booleanValue);

                case Java.Lang.Integer integerValue:
                    return (key, (int)integerValue);

                case Java.Lang.Long longValue:
                    return (key, (long)longValue);

                case Java.Lang.Float floatValue:
                    return (key, (float)floatValue);

                case Java.Lang.Double doubleValue:
                    return (key, (double)doubleValue);

                case Java.Lang.Character characterValue:
                    return (key, (char)characterValue);

                default:
                    return (key, value?.ToString());
            }
        }
    }
}
