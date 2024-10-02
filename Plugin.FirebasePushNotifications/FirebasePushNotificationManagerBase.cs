using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Extensions;
using Plugin.FirebasePushNotifications.Internals;
using Plugin.FirebasePushNotifications.Model.Queues;

namespace Plugin.FirebasePushNotifications.Platforms
{
    [Preserve(AllMembers = true)]
    public abstract class FirebasePushNotificationManagerBase : IDisposable
    {
        private readonly string instanceId = Guid.NewGuid().ToString()[..5];
        protected readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        protected readonly FirebasePushNotificationOptions options;
        protected readonly IFirebasePushNotificationPreferences preferences;

        private string[] subscribedTopics;
        private NotificationCategory[] notificationCategories = null;
        private bool disposed;

        private IQueue<FirebasePushNotificationTokenEventArgs> tokenRefreshQueue;
        private IQueue<FirebasePushNotificationDataEventArgs> notificationReceivedQueue;
        private IQueue<FirebasePushNotificationDataEventArgs> notificationDeletedQueue;
        private IQueue<FirebasePushNotificationResponseEventArgs> notificationOpenedQueue;
        private IQueue<FirebasePushNotificationActionEventArgs> notificationActionQueue;

        private EventHandler<FirebasePushNotificationTokenEventArgs> tokenRefreshEventHandler;
        private EventHandler<FirebasePushNotificationActionEventArgs> notificationActionEventHandler;
        private EventHandler<FirebasePushNotificationDataEventArgs> notificationReceivedEventHandler;
        private EventHandler<FirebasePushNotificationDataEventArgs> notificationDeletedEventHandler;
        private EventHandler<FirebasePushNotificationResponseEventArgs> notificationOpenedEventHandler;

        protected internal FirebasePushNotificationManagerBase(
         ILogger<IFirebasePushNotification> logger,
         ILoggerFactory loggerFactory,
         FirebasePushNotificationOptions options,
         IPushNotificationHandler pushNotificationHandler,
         IFirebasePushNotificationPreferences preferences)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
            this.options = options;
            this.NotificationHandler = pushNotificationHandler;
            this.preferences = preferences;

            this.CreateOrUpdateQueues(options.QueueFactory);
        }

        private void CreateOrUpdateQueues(IQueueFactory queueFactory)
        {
            // Clear existing queues (if any exist)
            this.ClearQueuesInternal();

            if (queueFactory != null)
            {
                // Create new queues
                this.tokenRefreshQueue = queueFactory.Create<FirebasePushNotificationTokenEventArgs>("tokenRefreshQueue", this.loggerFactory);
                this.notificationReceivedQueue = queueFactory.Create<FirebasePushNotificationDataEventArgs>("notificationReceivedQueue", this.loggerFactory);
                this.notificationDeletedQueue = queueFactory.Create<FirebasePushNotificationDataEventArgs>("notificationDeletedQueue", this.loggerFactory);
                this.notificationOpenedQueue = queueFactory.Create<FirebasePushNotificationResponseEventArgs>("notificationOpenedQueue", this.loggerFactory);
                this.notificationActionQueue = queueFactory.Create<FirebasePushNotificationActionEventArgs>("notificationActionQueue", this.loggerFactory);
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

        /// <inheritdoc cref="IFirebasePushNotification.ClearQueues"/>
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

        /// <inheritdoc cref="IFirebasePushNotification.NotificationHandler"/>
        public IPushNotificationHandler NotificationHandler { get; set; }

        /// <inheritdoc cref="IFirebasePushNotification.NotificationCategories"/>
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

        /// <inheritdoc cref="IFirebasePushNotification.RegisterNotificationCategories"/>
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

        /// <inheritdoc cref="IFirebasePushNotification.ClearNotificationCategories"/>
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

        /// <inheritdoc cref="IFirebasePushNotification.SubscribedTopics"/>
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
                nameof(this.TokenRefreshed));
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
                nameof(this.NotificationReceived));

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
                nameof(this.NotificationDeleted));

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

        public void HandleNotificationOpened(IDictionary<string, object> data, NotificationCategoryType notificationCategoryType)
        {
            this.logger.LogDebug("HandleNotificationOpened");

            this.RaiseOrQueueEvent(
                this.notificationOpenedEventHandler,
                () => new FirebasePushNotificationResponseEventArgs(data, notificationCategoryType),
                this.notificationOpenedQueue,
                nameof(this.NotificationOpened));

            this.OnNotificationOpened(data, notificationCategoryType);

            this.NotificationHandler?.OnOpened(data, notificationCategoryType);
        }

        protected virtual void OnNotificationOpened(IDictionary<string, object> data, NotificationCategoryType notificationCategoryType)
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

        public void HandleNotificationAction(IDictionary<string, object> data, string categoryId, string actionId, NotificationCategoryType notificationCategoryType)
        {
            this.logger.LogDebug("HandleNotificationAction");

            var notificationAction = this.GetNotificationAction(categoryId, actionId);

            this.RaiseOrQueueEvent(
                this.notificationActionEventHandler,
                () => new FirebasePushNotificationActionEventArgs(data, notificationAction, notificationCategoryType),
                this.notificationActionQueue,
                nameof(this.NotificationAction));

            this.OnNotificationAction(data);

            // TODO: Extend interface
            //this.NotificationHandler?.OnAction(data);
        }

        private NotificationAction GetNotificationAction(string categoryId, string actionId)
        {
            var notificationCategory = this.NotificationCategories
                .SingleOrDefault(c => string.Equals(c.CategoryId, categoryId, StringComparison.InvariantCultureIgnoreCase));

            if (notificationCategory != null)
            {
                var notificationAction = notificationCategory.Actions
                    .SingleOrDefault(a => string.Equals(a.Id, actionId, StringComparison.InvariantCultureIgnoreCase));

                if (notificationAction != null)
                {
                    return notificationAction;
                }

                throw new InvalidOperationException(
                    $"Notification action with Id=\"{actionId}\" " +
                    $"could not be found in category with {nameof(categoryId)}=\"{categoryId}\"");
            }

            throw new InvalidOperationException(
                $"Notification category with {nameof(categoryId)}=\"{categoryId}\" " +
                $"could not be found in {nameof(this.NotificationCategories)}");
        }

        protected virtual void OnNotificationAction(IDictionary<string, object> data)
        {
        }

        public event EventHandler<FirebasePushNotificationActionEventArgs> NotificationAction
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
            if (eventHandler != null && eventHandler.GetInvocationList().Length is var subscribersCount and > 0)
            {
                // If subscribers are present, invoke the event handler
                this.logger.LogDebug(
                    $"{callerName ?? nameof(this.RaiseOrQueueEvent)} raises event \"{eventName}\" " +
                    $"to {subscribersCount} subscriber{(subscribersCount != 1 ? "s" : "")}");

                var args = getEventArgs();
                eventHandler.Invoke(this, args);
            }
            else
            {
                if (queue != null)
                {
                    // If no subscribers are present, queue the event args
                    this.logger.LogDebug(
                        $"{callerName ?? nameof(this.RaiseOrQueueEvent)} queues event \"{eventName}\" " +
                        $"into {queue.GetType().GetFormattedName()} for deferred delivery");

                    var args = getEventArgs();
                    queue.Enqueue(args);
                }
                else
                {
                    // If no subscribers are present and no queue is present, we just drop the event...
                    this.logger.LogWarning(
                        $"{callerName ?? nameof(this.RaiseOrQueueEvent)} drops event \"{eventName}\" " +
                        $"(no event subscribers / no queue present).");
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
                        $"{nameof(this.DequeueAndSubscribe)} dequeues {queue.Count} event{(queue.Count == 1 ? "" : "s")} \"{eventName}\" " +
                        $"from {queue.GetType().GetFormattedName()} for deferred delivery");

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