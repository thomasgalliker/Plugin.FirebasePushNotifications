namespace Plugin.FirebasePushNotifications.Model.Queues
{
    /// <summary>
    /// The options used to configure <see cref="PersistentQueue{T}"/>.
    /// </summary>
    public class PersistentQueueOptions
    {
        /// <summary>
        /// The default options.
        /// </summary>
        public static readonly PersistentQueueOptions Default = new PersistentQueueOptions();

        /// <summary>
        /// The base directory to be used to store queued items.
        /// If null/empty, the current directory is used by default.
        /// </summary>
        public string BaseDirectory { get; set; }

        /// <summary>
        /// Selects the file name from a given generic type T in <see cref="PersistentQueue{T}"/>.
        /// </summary>
        public Func<Type, string> FileNameSelector { get; set; } = (t) => $"{t.Name}.json";
    }
}