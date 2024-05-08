namespace Plugin.FirebasePushNotifications.Model.Queues
{
    /// <summary>
    /// The options used to configure <see cref="PersistentQueue{T}"/>.
    /// </summary>
    public class PersistentQueueOptions
    {
        private static readonly string DefaultBaseDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FirebaseQueues");

        /// <summary>
        /// The default options.
        /// </summary>
        public static readonly PersistentQueueOptions Default = new PersistentQueueOptions();

        private string baseDirectory;

        /// <summary>
        /// The base directory to be used to store queued items.
        /// If null/empty, "<see cref="Environment.SpecialFolder.MyDocuments"/>/FirebaseQueues" is used as fallback.
        /// </summary>
        public virtual string BaseDirectory
        {
            get
            {
                return this.baseDirectory is string baseDirectory && !string.IsNullOrWhiteSpace(baseDirectory) 
                    ? baseDirectory
                    : DefaultBaseDirectory;
            }

            set => this.baseDirectory = value;
        }

        /// <summary>
        /// Selects the file name from a given generic type T in <see cref="PersistentQueue{T}"/>.
        /// </summary>
        public virtual Func<(Type Type, string Key), string> FileNameSelector { get; set; } = q => $"{q.Key}.json";
    }
}