namespace Plugin.FirebasePushNotifications
{
    /// <summary>
    /// Preferences abstraction used by this plugin
    /// to store settings (key-value pairs).
    /// </summary>
    public interface IFirebasePushNotificationPreferences
    {
        /// <summary>
        /// Sets a preference <paramref name="value"/> to <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The generic type of <paramref name="value"/>.</typeparam>
        /// <param name="key">The key which is later used to read the preference value.</param>
        /// <param name="value">The preference value.</param>
        void Set<T>(string key, T value);

        /// <summary>
        /// Gets the preference value for <paramref name="key"/>.
        /// If the value cannot be found, the default value <paramref name="defaultValue"/> is returned.
        /// </summary>
        /// <typeparam name="T">The generic type which is expected for the preference value.</typeparam>
        /// <param name="key">The preference key.</param>
        /// <param name="defaultValue">The default value, if the value for the given key cannot be found.</param>
        /// <returns>The preference value of type <typeparamref name="T"/>.</returns>
        T Get<T>(string key, T defaultValue = default);

        /// <summary>
        /// Removes the preference value for <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The preference key.</param>
        void Remove(string key);

        /// <summary>
        /// Checks if there is a value for <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The preference key.</param>
        /// <returns><c>true</c> if a value for <paramref name="key"/> exists, otherwise, <c>false</c>.</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Removes all settings written by this library.
        /// </summary>
        /// <remarks>
        /// You may want to clear all settings
        /// when the user of your app is about to log-out.
        /// </remarks>
        void ClearAll();
    }
}
