using System.Diagnostics;
using Newtonsoft.Json;
using Plugin.FirebasePushNotifications.Extensions;

namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationPreferences : IFirebasePushNotificationPreferences
    {
        private readonly IPreferences preferences;

        public FirebasePushNotificationPreferences(IPreferences preferences)
        {
            this.preferences = preferences;
        }

        /// <summary>
        /// Store an element using any kind of key (if it doesnt exist)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(string key, T value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            EnsureKeyRegistered(key);

            if (value is not string serializedValue)
            {
                serializedValue = JsonConvert.SerializeObject(value);
            }

            this.preferences.Set(key, serializedValue);
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            EnsureKeyRegistered(key);

            T value;

            try
            {
                var serializedValue = this.preferences.Get<string>(key, null);

                if (serializedValue is null)
                {
                    value = defaultValue;
                }
                else if (serializedValue is T stringValue)
                {
                    value = stringValue;
                }
                else
                {
                    value = JsonConvert.DeserializeObject<T>(serializedValue);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Get<{typeof(T).GetFormattedName()}> with key={key} failed with exception{Environment.NewLine}" +
                    $"{ex}");
                value = defaultValue;
            }

            return value;
        }

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            EnsureKeyRegistered(key);

            this.preferences.Remove(key);
        }

        private static void EnsureKeyRegistered(string key)
        {
            if (!Constants.Preferences.AllKeys.Contains(key))
            {
                throw new InvalidOperationException($"Key \"{key}\" is not registered in Constants.Preferences");
            }
        }

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            EnsureKeyRegistered(key);

            return this.preferences.ContainsKey(key);
        }

        /// <inheritdoc />
        public void ClearAll()
        {
            foreach (var key in Constants.Preferences.AllKeys)
            {
                this.preferences.Remove(key);
            }
        }
    }
}