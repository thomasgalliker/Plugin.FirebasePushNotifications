using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Plugin.FirebasePushNotifications.Internals;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    /// <summary>
    /// Thread-safe queue with file-based persistance.
    /// </summary>
    /// <typeparam name="T">Generic element type.</typeparam>
    /// <remarks>
    /// Internally is uses <see cref="System.Collections.Generic.Queue{T}"/>, so Enqueue can be O(n). Dequeue is O(1).
    /// </remarks>
    [DebuggerDisplay("PersistentQueue<{typeof(T).Name,nq}> Count={this.Count}")]
    internal class PersistentQueue<T> : IQueue<T> //TODO: Mark internal
    {
        private readonly object lockObj = new object();
        private readonly IQueue<T> internalQueue;
        private readonly IFileInfo fileInfo;
        private readonly IFileInfoFactory fileInfoFactory;
        private readonly IDirectoryInfoFactory directoryInfoFactory;
        private readonly PersistentQueueOptions options;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        /// <summary>
        /// Creates a new instance of <see cref="PersistentQueue{T}"/> with options.
        /// </summary>
        public PersistentQueue(string key)
            : this(key, PersistentQueueOptions.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="PersistentQueue{T}"/> with options.
        /// </summary>
        /// <param name="options">The options.</param>
        public PersistentQueue(string key, PersistentQueueOptions options)
            : this(FileInfoFactory.Current, DirectoryInfoFactory.Current, key, options)
        {
        }

        internal PersistentQueue(
            IFileInfoFactory fileInfoFactory,
            IDirectoryInfoFactory directoryInfoFactory,
            string key,
            PersistentQueueOptions options)
        {
            this.fileInfoFactory = fileInfoFactory ?? throw new ArgumentNullException(nameof(fileInfoFactory));
            this.directoryInfoFactory = directoryInfoFactory ?? throw new ArgumentNullException(nameof(directoryInfoFactory));
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            var baseDirectoryInfo = this.CreateDirectoryIfNotExists(options.BaseDirectory);
            this.fileInfo = fileInfoFactory.FromPath(Path.Combine(baseDirectoryInfo.FullName, this.options.FileNameSelector((typeof(T), key))));
            Debug.WriteLine($"XXXXXX - fileInfo: {this.fileInfo.FullName}");
            this.internalQueue = this.ReadQueueFile(this.fileInfo);
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

        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.Count" />
        public int Count
        {
            get
            {
                lock (this.lockObj)
                {
                    return this.internalQueue.Count;
                }
            }
        }

        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.Clear()"/>
        public void Clear()
        {
            lock (this.lockObj)
            {
                this.fileInfo.Delete();
                this.internalQueue.Clear();
            }
        }

        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.Enqueue(T)"/>
        public void Enqueue(T item)
        {
            lock (this.lockObj)
            {
                this.internalQueue.Enqueue(item);
                this.WriteQueueFile(this.fileInfo, this.internalQueue);
            }
        }

        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.TryDequeue(out T)"/>
        public bool TryDequeue([MaybeNullWhen(false)] out T result)
        {
            lock (this.lockObj)
            {
                var success = this.internalQueue.TryDequeue(out result);
                this.WriteQueueFile(this.fileInfo, this.internalQueue);
                return success;
            }
        }

        /// <inheritdoc />
        public IEnumerable<T> TryDequeueAll()
        {
            lock (this.lockObj)
            {
                var items = this.TryDequeueAllInternal().ToArray();
                this.WriteQueueFile(this.fileInfo, this.internalQueue);
                return items;
            }
        }

        private IEnumerable<T> TryDequeueAllInternal()
        {
            while (this.internalQueue.TryDequeue(out var item))
            {
                yield return item;
            }
        }

        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.TryPeek(out T)"/>
        public bool TryPeek([MaybeNullWhen(false)] out T result)
        {
            lock (this.lockObj)
            {
                return this.internalQueue.TryPeek(out result);
            }
        }

        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.ToArray()"/>
        public T[] ToArray()
        {
            lock (this.lockObj)
            {
                return this.internalQueue.ToArray();
            }
        }

        private IQueue<T> ReadQueueFile(IFileInfo fileInfo)
        {
            IEnumerable<T> items = null;

            if (fileInfo.Exists)
            {
                try
                {
                    using (var streamReader = fileInfo.OpenText())
                    {
                        var json = streamReader.ReadToEnd();
                        if (!string.IsNullOrEmpty(json))
                        {
                            items = JsonConvert.DeserializeObject<List<T>>(json, this.jsonSerializerSettings);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ReadQueueFile failed with exception: {ex}");
                }
            }

            return items == null
                ? new Queue<T>()
                : new Queue<T>(items);
        }

        private void WriteQueueFile(IFileInfo fileInfo, IQueue<T> queue)
        {
            var targetDirectory = fileInfo.Directory;
            if (!targetDirectory.Exists)
            {
                targetDirectory.Create();
            }

            using (var writer = fileInfo.CreateText())
            {
                var array = queue.ToArray();
                var json = JsonConvert.SerializeObject(array, this.jsonSerializerSettings);
                writer.Write(json);
            }
        }
    }
}