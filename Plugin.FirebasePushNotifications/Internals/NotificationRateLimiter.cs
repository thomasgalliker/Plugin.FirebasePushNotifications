using System.Collections.Concurrent;

namespace Plugin.FirebasePushNotifications.Internals
{
    internal class NotificationRateLimiter
    {
        private readonly ConcurrentDictionary<string, DateTime> cache = new();

        public bool HasReachedLimit(string identifier, TimeSpan expirationTime)
        {
            var utcNow = DateTime.UtcNow;

            if (this.cache.TryGetValue(identifier, out var existingExpirationTime))
            {
                if (existingExpirationTime > utcNow)
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

            this.cache[identifier] = utcNow.Add(expirationTime);
            return false;
        }

        public int Count => this.cache.Count();
    }
}