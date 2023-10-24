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
    /// Internally is uses <see cref="Queue{T}"/>, so Enqueue can be O(n). Dequeue is O(1).
    /// </remarks>
    [DebuggerDisplay("PersistentQueue<{typeof(T).Name,nq}> Count={this.Count}")]
    public class PersistentQueue<T> : IQueue<T> //TODO: Mark internal
    {
        private readonly Queue<T> queue;
        private readonly IFileInfo fileInfo;
        private readonly PersistentQueueOptions options;

        /// <summary>
        /// Creates a new instance of <see cref="PersistentQueue{T}"/> with options.
        /// </summary>
        public PersistentQueue()
            : this(PersistentQueueOptions.Default)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="PersistentQueue{T}"/> with options.
        /// </summary>
        /// <param name="options">The options.</param>
        public PersistentQueue(PersistentQueueOptions options)
            : this(new FileInfoFactory(), options)
        {
        }

        internal PersistentQueue(IFileInfoFactory fileInfoFactory, PersistentQueueOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            var baseDirectoryInfo = this.CreateDirectoryIfNotExists(options.BaseDirectory);

            this.fileInfo = fileInfoFactory.FromPath(Path.Combine(baseDirectoryInfo.FullName, this.options.FileNameSelector(typeof(T))));
            this.queue = ReadQueueFile(this.fileInfo);
        }

        private DirectoryInfo CreateDirectoryIfNotExists(string path)
        {
            var directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            return directoryInfo;
        }

        /// <inheritdoc cref="Queue{T}.Count" />
        public int Count
        {
            get
            {
                lock (this)
                {
                    return this.queue.Count;
                }
            }
        }

        /// <inheritdoc cref="Queue{T}.Clear()"/>
        public void Clear()
        {
            lock (this)
            {
                this.fileInfo.Delete();
                this.queue.Clear();
            }
        }

        /// <inheritdoc cref="Queue{T}.Enqueue(T)"/>
        public void Enqueue(T item)
        {
            lock (this)
            {
                this.queue.Enqueue(item);
                WriteQueueFile(this.fileInfo, this.queue);
            }
        }

        /// <inheritdoc cref="Queue{T}.Dequeue()"/>
        public T Dequeue()
        {
            lock (this)
            {
                var item = this.queue.Dequeue();
                WriteQueueFile(this.fileInfo, this.queue);
                return item;
            }
        }

        /// <inheritdoc cref="Queue{T}.TryDequeue(out T)"/>
        public bool TryDequeue([MaybeNullWhen(false)] out T result)
        {
            lock (this)
            {
                var success = this.queue.TryDequeue(out result);
                WriteQueueFile(this.fileInfo, this.queue);
                return success;
            }
        }

        public IEnumerable<T> TryDequeueAll()
        {
            lock (this)
            {
                var items = this.TryDequeueAllInternal().ToArray();
                WriteQueueFile(this.fileInfo, this.queue);
                return items;
            }
        }

        private IEnumerable<T> TryDequeueAllInternal()
        {
            while (this.queue.TryDequeue(out var item))
            {
                yield return item;
            }
        }

        /// <inheritdoc cref="Queue{T}.Peek()"/>
        public T Peek()
        {
            lock (this)
            {
                return this.queue.Peek();
            }
        }

        /// <inheritdoc cref="Queue{T}.TryPeek(out T)"/>
        public bool TryPeek([MaybeNullWhen(false)] out T result)
        {
            lock (this)
            {
                return this.queue.TryPeek(out result);
            }
        }

        private static Queue<T> ReadQueueFile(IFileInfo fileInfo)
        {
            if (fileInfo.Exists)
            {
                try
                {
                    using (var streamReader = fileInfo.OpenText())
                    {
                        var json = streamReader.ReadToEnd();
                        if (!string.IsNullOrEmpty(json))
                        {
                            var items = JsonConvert.DeserializeObject<List<T>>(json);
                            if (items != null)
                            {
                                return new Queue<T>(items);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"ReadQueueFile failed with exception: {ex}");
                }

                return new Queue<T>();
            }
            else
            {
                return new Queue<T>();
            }
        }

        private static void WriteQueueFile(IFileInfo fileInfo, Queue<T> queue)
        {
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            if (!fileInfo.Directory.Exists)
            {
                if (File.Exists(fileInfo.Directory.FullName))
                {
                    File.Delete(fileInfo.Directory.FullName);
                }

                fileInfo.Directory.Create();
            }

            using (var writer = fileInfo.CreateText())
            {
                var json = JsonConvert.SerializeObject(queue);
                writer.Write(json);
            }
        }
    }
}