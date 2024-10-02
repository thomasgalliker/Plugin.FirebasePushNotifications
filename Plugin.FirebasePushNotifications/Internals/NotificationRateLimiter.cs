using System.Collections.Concurrent;

namespace Plugin.FirebasePushNotifications.Internals
{
    internal interface INotificationRateLimiter
    {
        bool HasReachedLimit(string identifier);
    }

    internal class NotificationRateLimiter : INotificationRateLimiter
    {
        private readonly ConcurrentDictionary<string, DateTime> cache = new();
        private readonly TimeSpan expirationPeriod;

        public NotificationRateLimiter(TimeSpan expirationPeriod)
        {
            this.expirationPeriod = expirationPeriod;
        }

        public bool HasReachedLimit(string identifier)
        {
            var utcNow = DateTime.UtcNow;

            if (this.cache.TryGetValue(identifier, out var expirationTime))
            {
                if (expirationTime > utcNow)
                {
                    return true;
                }
            }

            foreach (var item in this.cache)
            {
                if (item.Value > utcNow)
                {
                    this.cache.TryRemove(item.Key, out _);
                }
            }

            this.cache[identifier] = utcNow.Add(this.expirationPeriod);
            return false;
        }

        public int Count => this.cache.Count();
    }
}