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

        public PersistentQueueFactory()
            : this(PersistentQueueOptions.Default)
        {
        }

        public PersistentQueueFactory(PersistentQueueOptions options)
            : this(options, IPlatformApplication.Current.Services.GetService<ILoggerFactory>(), FileInfoFactory.Current, DirectoryInfoFactory.Current)
        {
            this.options = options;
        }

        internal PersistentQueueFactory(
            PersistentQueueOptions options,
            ILoggerFactory loggerFactory,
            IFileInfoFactory fileInfoFactory,
            IDirectoryInfoFactory directoryInfoFactory)
        {
            this.options = options;
            this.fileInfoFactory = fileInfoFactory;
            this.directoryInfoFactory = directoryInfoFactory;

            this.baseDirectoryInfo = this.CreateDirectoryIfNotExists(options.BaseDirectory);
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
            var logger = new NullLogger<PersistentQueue<T>>();
            var fileInfo = this.fileInfoFactory.FromPath(Path.Combine(this.baseDirectoryInfo.FullName, this.options.FileNameSelector((typeof(T), key))));
            return new PersistentQueue<T>(logger, fileInfo);
        }
    }
}