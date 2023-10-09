using System.Diagnostics.CodeAnalysis;

namespace Plugin.FirebasePushNotifications.Model.Queues
{
    public interface IQueue<T>
    {
        int Count { get; }

        void Clear();

        void Enqueue(T item);

        bool TryDequeue([MaybeNullWhen(false)] out T result);
        
        IEnumerable<T> TryDequeueAll();

        bool TryPeek([MaybeNullWhen(false)] out T result);
    }
}