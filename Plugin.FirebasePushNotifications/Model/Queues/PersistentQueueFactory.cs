using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Plugin.FirebasePushNotifications.Internals;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public class PersistentQueueFactory : IQueueFactory
    {
        private readonly PersistentQueueOptions options;
        private readonly IFileInfoFactory fileInfoFactory;
        private readonly IDirectoryInfoFactory directoryInfoFactory;
        private readonly IDirectoryInfo baseDirectoryInfo;
        private ILoggerFactory loggerFactory;

        public PersistentQueueFactory()
            : this(PersistentQueueOptions.Default)
        {
        }

        public PersistentQueueFactory(PersistentQueueOptions options)
            : this(options, FileInfoFactory.Current, DirectoryInfoFactory.Current)
        {
        }

        internal PersistentQueueFactory(
            PersistentQueueOptions options,
            IFileInfoFactory fileInfoFactory,
            IDirectoryInfoFactory directoryInfoFactory)
        {
            this.options = options;
            this.fileInfoFactory = fileInfoFactory;
            this.directoryInfoFactory = directoryInfoFactory;

            this.baseDirectoryInfo = this.CreateDirectoryIfNotExists(options.BaseDirectory);
        }

        public ILoggerFactory LoggerFactory
        {
            get => this.loggerFactory;
            set => this.loggerFactory = value ?? new NullLoggerFactory();
        }

        private IDirectoryInfo CreateDirectoryIfNotExists(string path)
        {
            var directoryInfo = this.directoryInfoFactory.FromPath(path);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            return directoryInfo;
        }

        /// <inheritdoc />
        public IQueue<T> Create<T>(string key)
        {
            var logger = this.LoggerFactory.CreateLogger<PersistentQueue<T>>();
            var fileInfo = this.fileInfoFactory.FromPath(Path.Combine(this.baseDirectoryInfo.FullName, this.options.FileNameSelector((typeof(T), key))));
            return new PersistentQueue<T>(logger, fileInfo);
        }
    }
}