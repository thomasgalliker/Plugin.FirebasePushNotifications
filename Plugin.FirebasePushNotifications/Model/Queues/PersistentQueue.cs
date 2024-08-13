using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    internal class PersistentQueue<T> : IQueue<T>
    {
        private readonly ILogger logger;
        private readonly IQueue<T> internalQueue;
        private readonly IFileInfo fileInfo;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly object lockObj = new object();

        internal PersistentQueue(
            ILogger<PersistentQueue<T>> logger,
            IFileInfo fileInfo)
        {
            this.logger = logger ?? new NullLogger<PersistentQueue<T>>();
            this.fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));

            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };

            this.internalQueue = this.ReadQueueFile(this.fileInfo);
            this.logger = logger;
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
                    this.logger.LogDebug($"ReadQueueFile: fileInfo={fileInfo.FullName}");

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
                    this.logger.LogError(ex, $"ReadQueueFile failed with exception: {ex}");
                }
            }

            return items == null
                ? new Queue<T>()
                : new Queue<T>(items);
        }

        private void WriteQueueFile(IFileInfo fileInfo, IQueue<T> queue)
        {
            try
            {
                var targetDirectory = fileInfo.Directory;
                if (!targetDirectory.Exists)
                {
                    this.logger.LogDebug($"WriteQueueFile: Creating target directory {targetDirectory.FullName}");
                    targetDirectory.Create();
                }

                this.logger.LogDebug($"WriteQueueFile: fileInfo={fileInfo.FullName}");

                using (var writer = fileInfo.CreateText())
                {
                    var array = queue.ToArray();
                    var json = JsonConvert.SerializeObject(array, this.jsonSerializerSettings);
                    writer.Write(json);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"WriteQueueFile failed with exception: {ex}");
            }
        }
    }
}