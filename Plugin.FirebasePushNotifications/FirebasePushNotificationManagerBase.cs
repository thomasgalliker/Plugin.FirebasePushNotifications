using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Extensions;
using Plugin.FirebasePushNotifications.Internals;
using Plugin.FirebasePushNotifications.Model.Queues;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public abstract class FirebasePushNotificationManagerBase : IDisposable
    {
        private readonly string instanceId = Guid.NewGuid().ToString()[..5];

        private string[] subscribedTopics;
        private NotificationCategory[] notificationCategories = null;

        protected ILogger<IFirebasePushNotification> logger;
        protected IFirebasePushNotificationPreferences preferences;
        private bool disposed;

        private IQueue<FirebasePushNotificationTokenEventArgs> tokenRefreshQueue;
        private IQueue<FirebasePushNotificationDataEventArgs> notificationReceivedQueue;
        private IQueue<FirebasePushNotificationDataEventArgs> notificationDeletedQueue;
        private IQueue<FirebasePushNotificationResponseEventArgs> notificationOpenedQueue;
        private IQueue<FirebasePushNotificationResponseEventArgs> notificationActionQueue;

        private EventHandler<FirebasePushNotificationTokenEventArgs> tokenRefreshEventHandler;
        private EventHandler<FirebasePushNotificationResponseEventArgs> notificationActionEventHandler;
        private EventHandler<FirebasePushNotificationDataEventArgs> notificationReceivedEventHandler;
        private EventHandler<FirebasePushNotificationDataEventArgs> notificationDeletedEventHandler;
        private EventHandler<FirebasePushNotificationResponseEventArgs> notificationOpenedEventHandler;

        protected FirebasePushNotificationManagerBase()
        {
        }

        /// <inheritdoc />
        internal void Configure(FirebasePushNotificationOptions options)
        {
            this.logger.LogDebug("Configure");

            this.preferences = options.Preferences;

            this.CreateOrUpdateQueues(options.QueueFactory);

            this.ConfigurePlatform(options);
        }

        private void CreateOrUpdateQueues(IQueueFactory queueFactory)
        {
            // Clear existing queues (if any exist)
            this.ClearQueuesInternal();

            if (queueFactory != null)
            {
                // Create new queues
                this.tokenRefreshQueue = queueFactory.Create<FirebasePushNotificationTokenEventArgs>("tokenRefreshQueue");
                this.notificationReceivedQueue = queueFactory.Create<FirebasePushNotificationDataEventArgs>("notificationReceivedQueue");
                this.notificationDeletedQueue = queueFactory.Create<FirebasePushNotificationDataEventArgs>("notificationDeletedQueue");
                this.notificationOpenedQueue = queueFactory.Create<FirebasePushNotificationResponseEventArgs>("notificationOpenedQueue");
                this.notificationActionQueue = queueFactory.Create<FirebasePushNotificationResponseEventArgs>("notificationActionQueue");
            }
            else
            {
                // Remove queues
                this.tokenRefreshQueue = null;
                this.notificationReceivedQueue = null;
                this.notificationDeletedQueue = null;
                this.notificationOpenedQueue = null;
                this.notificationActionQueue = null;
            }
        }

        /// <inheritdoc />
        public void ClearQueues()
        {
            this.logger.LogDebug("ClearQueues");
            this.ClearQueuesInternal();
        }

        private void ClearQueuesInternal()
        {
            this.tokenRefreshQueue?.Clear();
            this.notificationReceivedQueue?.Clear();
            this.notificationDeletedQueue?.Clear();
            this.notificationOpenedQueue?.Clear();
            this.notificationActionQueue?.Clear();
        }

        /// <summary>
        /// Platform-specific additions to <see cref="Configure(FirebasePushNotificationOptions)"/>.
        /// </summary>
        protected abstract void ConfigurePlatform(FirebasePushNotificationOptions options);

        /// <inheritdoc/>
        public ILogger<IFirebasePushNotification> Logger
        {
            set => this.logger = value;
        }

        public IPushNotificationHandler NotificationHandler { get; set; }

        /// <inheritdoc />
        public NotificationCategory[] NotificationCategories
        {
            get
            {
                this.notificationCategories ??= this.preferences.Get(Constants.Preferences.NotificationCategoriesKey, Array.Empty<NotificationCategory>());
                return this.notificationCategories;
            }
            protected set
            {
                if (value != null)
                {
                    this.preferences.Set(Constants.Preferences.NotificationCategoriesKey, value);
                }
                else
                {
                    this.preferences.Remove(Constants.Preferences.NotificationCategoriesKey);
                }

                this.notificationCategories = value;
            }
        }

        /// <inheritdoc />
        public void RegisterNotificationCategories(NotificationCategory[] notificationCategories)
        {
            if (notificationCategories == null)
            {
                throw new ArgumentNullException(nameof(notificationCategories));
            }

            if (notificationCategories.Length == 0)
            {
                throw new ArgumentException($"{nameof(notificationCategories)} must not be empty", nameof(notificationCategories));
            }

            this.RegisterNotificationCategoriesPlatform(notificationCategories);

            this.NotificationCategories = notificationCategories;
        }

        /// <summary>
        /// Platform-specific additions to <see cref="RegisterNotificationCategories"/>.
        /// </summary>
        protected virtual void RegisterNotificationCategoriesPlatform(NotificationCategory[] notificationCategories)
        {
        }

        /// <inheritdoc />
        public void ClearNotificationCategories()
        {
            this.NotificationCategories = null;

            this.ClearNotificationCategoriesPlatform();
        }

        /// <summary>
        /// Platform-specific additions to <see cref="ClearNotificationCategories"/>.
        /// </summary>
        protected virtual void ClearNotificationCategoriesPlatform()
        {
        }

        /// <inheritdoc />
        public string[] SubscribedTopics
        {
            get
            {
                this.subscribedTopics ??= this.preferences.Get(Constants.Preferences.SubscribedTopicsKey, Array.Empty<string>());
                return this.subscribedTopics;
            }
            protected set
            {
                if (value != null)
                {
                    this.preferences.Set(Constants.Preferences.SubscribedTopicsKey, value);
                }
                else
                {
                    this.preferences.Remove(Constants.Preferences.SubscribedTopicsKey);
                }

                this.subscribedTopics = value;
            }
        }

        public void HandleTokenRefresh(string token)
        {
            this.logger.LogDebug($"HandleTokenRefresh: \"{TokenFormatter.AnonymizeToken(token)}\"");

            // TODO: Move Token property to base class and add virtual/protected setter with this code:
            if (!string.IsNullOrEmpty(token))
            {
                this.preferences.Set(Constants.Preferences.TokenKey, token);
            }
            else
            {
                this.preferences.Remove(Constants.Preferences.TokenKey);
            }

            this.HandleTokenRefreshPlatform(token);

            this.RaiseOrQueueEvent(
                this.tokenRefreshEventHandler,
                () => new FirebasePushNotificationTokenEventArgs(token),
                this.tokenRefreshQueue,
                nameof(TokenRefreshed));
        }

        /// <summary>
        /// Platform-specific additions to <see cref="HandleTokenRefresh(string)"/>.
        /// </summary>
        protected virtual void HandleTokenRefreshPlatform(string token)
        {
        }

        public event EventHandler<FirebasePushNotificationTokenEventArgs> TokenRefreshed
        {
            add
            {
                this.DequeueAndSubscribe(
                    value,
                    ref this.tokenRefreshEventHandler,
                    this.tokenRefreshQueue);
            }
            remove => this.tokenRefreshEventHandler -= value;
        }

        public void HandleNotificationReceived(IDictionary<string, object> data)
        {
            this.logger.LogDebug("HandleNotificationReceived");

            this.RaiseOrQueueEvent(
                this.notificationReceivedEventHandler,
                () => new FirebasePushNotificationDataEventArgs(data),
                this.notificationReceivedQueue,
                nameof(NotificationReceived));

            this.OnNotificationReceived(data);

            this.NotificationHandler?.OnReceived(data);
        }

        protected virtual void OnNotificationReceived(IDictionary<string, object> data)
        {
        }

        public event EventHandler<FirebasePushNotificationDataEventArgs> NotificationReceived
        {
            add
            {
                this.DequeueAndSubscribe(
                    value,
                    ref this.notificationReceivedEventHandler,
                    this.notificationReceivedQueue);
            }
            remove => this.notificationReceivedEventHandler -= value;
        }

        public void HandleNotificationDeleted(IDictionary<string, object> data)
        {
            this.logger.LogDebug("HandleNotificationDeleted");

            this.RaiseOrQueueEvent(
                this.notificationDeletedEventHandler,
                () => new FirebasePushNotificationDataEventArgs(data),
                this.notificationDeletedQueue,
                nameof(NotificationDeleted));

            this.OnNotificationDeleted(data);

            // TODO: Extend interface
            //this.NotificationHandler?.OnDeleted(data);
        }

        protected virtual void OnNotificationDeleted(IDictionary<string, object> data)
        {
        }

        public event EventHandler<FirebasePushNotificationDataEventArgs> NotificationDeleted
        {
            add
            {
                this.DequeueAndSubscribe(
                    value,
                    ref this.notificationDeletedEventHandler,
                    this.notificationDeletedQueue);
            }
            remove => this.notificationDeletedEventHandler -= value;
        }

        public void HandleNotificationOpened(IDictionary<string, object> data, string notificationActionId, NotificationCategoryType notificationCategoryType)
        {
            this.logger.LogDebug("HandleNotificationOpened");

            var notificationAction = this.GetNotificationAction(notificationActionId);

            this.RaiseOrQueueEvent(
                this.notificationOpenedEventHandler,
                () => new FirebasePushNotificationResponseEventArgs(data, notificationAction, notificationCategoryType),
                this.notificationOpenedQueue,
                nameof(NotificationOpened));

            this.OnNotificationOpened(data, notificationAction, notificationCategoryType);

            this.NotificationHandler?.OnOpened(data, notificationAction, notificationCategoryType);
        }

        private NotificationAction GetNotificationAction(string notificationActionId)
        {
            return this.NotificationCategories
                .SelectMany(c => c.Actions)
                .SingleOrDefault(a => a.Id == notificationActionId);
        }

        protected virtual void OnNotificationOpened(IDictionary<string, object> data, NotificationAction notificationAction, NotificationCategoryType notificationCategoryType)
        {
        }

        public event EventHandler<FirebasePushNotificationResponseEventArgs> NotificationOpened
        {
            add
            {
                this.DequeueAndSubscribe(
                    value,
                    ref this.notificationOpenedEventHandler,
                    this.notificationOpenedQueue);
            }
            remove
            {
                this.notificationOpenedEventHandler -= value;
            }
        }

        public void HandleNotificationAction(IDictionary<string, object> data, string notificationActionId, NotificationCategoryType notificationCategoryType)
        {
            this.logger.LogDebug("HandleNotificationAction");

            var notificationAction = this.GetNotificationAction(notificationActionId);

            this.RaiseOrQueueEvent(
                this.notificationActionEventHandler,
                () => new FirebasePushNotificationResponseEventArgs(data, notificationAction, notificationCategoryType),
                this.notificationActionQueue,
                nameof(NotificationAction));

            this.OnNotificationAction(data);

            // TODO: Extend interface
            //this.NotificationHandler?.OnAction(data);
        }

        protected virtual void OnNotificationAction(IDictionary<string, object> data)
        {
        }

        public event EventHandler<FirebasePushNotificationResponseEventArgs> NotificationAction
        {
            add
            {
                this.DequeueAndSubscribe(
                    value,
                    ref this.notificationActionEventHandler,
                    this.notificationActionQueue);
            }
            remove
            {
                this.notificationActionEventHandler -= value;
            }
        }

        /// <summary>
        /// Raises the given <paramref name="eventHandler"/> with the event args created in <paramref name="getEventArgs"/>
        /// (if the event handler is not null).
        /// Alternatively (if the event handler is null) it queues the event args in the <paramref name="queue"/>.
        /// If no event handler and no queue is present, the event is dropped.
        /// </summary>
        private void RaiseOrQueueEvent<TEventArgs>(
            EventHandler<TEventArgs> eventHandler,
            Func<TEventArgs> getEventArgs,
            IQueue<TEventArgs> queue,
            string eventName,
            [CallerMemberName] string callerName = null) where TEventArgs : EventArgs
        {
            if (eventHandler != null && eventHandler.GetInvocationList().Length is int subscribersCount && subscribersCount > 0)
            {
                // If subscribers are present, invoke the event handler
                this.logger.LogDebug(
                     $"{callerName ?? nameof(RaiseOrQueueEvent)} raises event \"{eventName}\" to {subscribersCount} subscriber{(subscribersCount != 1 ? "s" : "")}");

                var args = getEventArgs();
                eventHandler.Invoke(this, args);
            }
            else
            {
                if (queue != null)
                {
                    // If no subscribers are present, queue the event args
                    this.logger.LogDebug(
                        $"{callerName ?? nameof(RaiseOrQueueEvent)} queues event \"{eventName}\" into {queue.GetType().GetFormattedName()} for deferred delivery");

                    var args = getEventArgs();
                    queue.Enqueue(args);
                }
                else
                {
                    // If no subscribers are present and no queue is present, we just drop the event...
                    this.logger.LogWarning(
                        $"{callerName ?? nameof(RaiseOrQueueEvent)} drops event \"{eventName}\" (no event subscribers / no queue present).");
                }
            }
        }

        /// <summary>
        /// Dequeues queued event args (if a queue exists for the given event) 
        /// and subscribes to <paramref name="eventHandler"/> with <paramref name="value"/>.
        /// </summary>
        private void DequeueAndSubscribe<TEventArgs>(
            EventHandler<TEventArgs> value,
            ref EventHandler<TEventArgs> eventHandler,
            IQueue<TEventArgs> queue,
            [CallerMemberName] string eventName = null) where TEventArgs : EventArgs
        {
            if (queue != null)
            {
                var previousSubscriptions = eventHandler;
                eventHandler += value;

                if (previousSubscriptions == null && eventHandler != null)
                {
                    this.logger.LogDebug(
                        $"{nameof(DequeueAndSubscribe)} dequeues {queue.Count} event{(queue.Count == 1 ? "" : "s")} \"{eventName}\" from {queue.GetType().GetFormattedName()} for deferred delivery");

                    foreach (var args in queue.TryDequeueAll())
                    {
                        eventHandler.Invoke(this, args);
                    }
                }
            }
            else
            {
                eventHandler += value;
            }
        }

        /// <inheritdoc />
        public void ClearAllNotifications()
        {
            this.ClearQueues();
            this.ClearAllNotificationsPlatform();
        }

        protected virtual void ClearAllNotificationsPlatform() // TODO: Needs to become abstract
        {
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: set large fields to null
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
