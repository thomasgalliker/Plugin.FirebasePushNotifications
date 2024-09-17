using Plugin.FirebasePushNotifications;

namespace MauiSampleApp.Services
{
    /// <summary>
    /// This class is just used to demonstrate a custom implementation for <see cref="IFirebasePushNotificationPreferences"/>.
    /// </summary>
    public class CustomFirebasePushNotificationPreferences : IFirebasePushNotificationPreferences
    {
        private readonly IDictionary<string, object> preferences = new Dictionary<string, object>();

        public void ClearAll()
        {
            this.preferences.Clear();
        }

        public bool ContainsKey(string key)
        {
            return this.preferences.ContainsKey(key);
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (!this.preferences.TryGetValue(key, out var value))
            {
                value = defaultValue;
            }

            return (T)value;
        }

        public void Remove(string key)
        {
            this.preferences.Remove(key);
        }

        public void Set<T>(string key, T value)
        {
            this.preferences[key] = value;
        }
    }
}