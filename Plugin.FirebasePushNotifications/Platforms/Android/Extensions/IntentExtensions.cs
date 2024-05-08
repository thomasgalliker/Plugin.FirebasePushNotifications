using System.Diagnostics;
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
            var extras = intent.GetExtras().ToArray();
            return ConvertValues(extras).ToDictionary(x => x.Key, x => x.Value);
        }

        private static IEnumerable<(string Key, object Value)> ConvertValues(IEnumerable<(string Key, object Value)> values)
        {
            foreach (var (Key, Value) in values)
            {
                if (Value.GetType().Namespace == "Java.Lang")
                {
                    if (Value is Java.Lang.String stringValue)
                    {
                        yield return (Key, (string)stringValue);
                    }
                    else if (Value is Java.Lang.Long longValue)
                    {
                        yield return (Key, (long)longValue);
                    }
                    else if (Value is Java.Lang.Integer integerValue)
                    {
                        yield return (Key, (int)integerValue);
                    }
                    else if (Value is Java.Lang.Boolean booleanValue)
                    {
                        yield return (Key, (bool)booleanValue);
                    }
                    else
                    {
                        Debug.WriteLine($"Value of type {Value?.GetType().Name} is currently not supported (Key: {Key})");
                    }
                }
            }
        }
    }
}
