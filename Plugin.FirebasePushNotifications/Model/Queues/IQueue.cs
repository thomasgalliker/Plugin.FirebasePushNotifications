using System.Diagnostics.CodeAnalysis;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    /// <summary>
    /// Abstraction of a queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IQueue<T>
    {
        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.Count"/>
        int Count { get; }

        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.Clear"/>
        void Clear();

        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.Enqueue"/>
        void Enqueue(T item);

        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.TryDequeue(out T)"/>
        bool TryDequeue([MaybeNullWhen(false)] out T result);

        /// <summary>
        /// Removes all objects and returns copies of them in the return value.
        /// </summary>
        /// <returns>All queued objects.</returns>
        IEnumerable<T> TryDequeueAll();

        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.TryPeek(out T)"/>
        bool TryPeek([MaybeNullWhen(false)] out T result);

        /// <inheritdoc cref="System.Collections.Generic.Queue{T}.ToArray()"/>
        T[] ToArray();
    }
}