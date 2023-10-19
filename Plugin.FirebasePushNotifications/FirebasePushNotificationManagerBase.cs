#if IOS
using Foundation;
#endif

using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Plugin.FirebasePushNotifications.Extensions;
using Plugin.FirebasePushNotifications.Model.Queues;

namespace Plugin.FirebasePushNotifications.Platforms
{
    public abstract class FirebasePushNotificationManagerBase
#if IOS
    : NSObject
#endif
    {
        private readonly string instanceId = Guid.NewGuid().ToString()[..5];

        protected ILogger<FirebasePushNotificationManager> logger;

        private IQueue<FirebasePushNotificationTokenEventArgs> tokenRefreshQueue;
        private IQueue<FirebasePushNotificationDataEventArgs> notificationReceivedQueue;
        private IQueue<FirebasePushNotificationDataEventArgs> notificationDeletedQueue;
        private IQueue<FirebasePushNotificationResponseEventArgs> notificationOpenedQueue;
        private IQueue<FirebasePushNotificationResponseEventArgs> notificationActionQueue;
        private IQueue<FirebasePushNotificationErrorEventArgs> notificationErrorQueue;

        protected FirebasePushNotificationManagerBase(
            ILogger<FirebasePushNotificationManager> logger,
            IQueueFactory queueFactory)
        {
            this.logger = logger;
            this.CreateOrUpdateQueues(queueFactory);
        }

        private void CreateOrUpdateQueues(IQueueFactory queueFactory)
        {
            // Clear existing queues (if any exist)
            this.tokenRefreshQueue?.Clear();
            this.notificationReceivedQueue?.Clear();
            this.notificationDeletedQueue?.Clear();
            this.notificationOpenedQueue?.Clear();
            this.notificationActionQueue?.Clear();
            this.notificationErrorQueue?.Clear();

            if (queueFactory != null)
            {
                // Create new queues
                this.tokenRefreshQueue = queueFactory.Create<FirebasePushNotificationTokenEventArgs>();
                this.notificationReceivedQueue = queueFactory.Create<FirebasePushNotificationDataEventArgs>();
                this.notificationDeletedQueue = queueFactory.Create<FirebasePushNotificationDataEventArgs>();
                this.notificationOpenedQueue = queueFactory.Create<FirebasePushNotificationResponseEventArgs>();
                this.notificationActionQueue = queueFactory.Create<FirebasePushNotificationResponseEventArgs>();
                this.notificationErrorQueue = queueFactory.Create<FirebasePushNotificationErrorEventArgs>();
            }
            else
            {
                // Remove queues
                this.tokenRefreshQueue = null;
                this.notificationReceivedQueue = null;
                this.notificationDeletedQueue = null;
                this.notificationOpenedQueue = null;
                this.notificationActionQueue = null;
                this.notificationErrorQueue = null;
            }
        }

        public void Configure(FirebasePushNotificationOptions options)
        {
            this.logger.LogDebug("Configure");

            this.CreateOrUpdateQueues(options.QueueFactory);

            this.OnConfigure(options);
        }

        protected virtual void OnConfigure(FirebasePushNotificationOptions options)
        {
        }

        /// <inheritdoc/>
        public ILogger<FirebasePushNotificationManager> Logger
        {
            set => this.logger = value;
        }

        public IPushNotificationHandler NotificationHandler { get; set; }

        public void HandleTokenRefresh(string token)
        {
            this.logger.LogDebug($"HandleTokenRefresh: \"{FormatTokenForLogging(token)}\"");

            this.OnTokenRefresh(token);

            this.RaiseOrQueueEvent(
                this.tokenRefreshEventHandler,
                () => new FirebasePushNotificationTokenEventArgs(token),
                this.tokenRefreshQueue,
                nameof(TokenRefreshed));
        }

        protected virtual void OnTokenRefresh(string token)
        {
        }

        private static string FormatTokenForLogging(string token)
        {
            if (token == null)
            {
                return "null";
            }

            try
            {
                var substringLength =  Math.Min(10, token.Length / 10);
                if (substringLength < 5)
                {
                    return Constants.SuppressedString;
                }

                return $"{Constants.SuppressedString}...{token.Substring(token.Length - substringLength)}";
            }
            catch
            {
                return Constants.SuppressedString;
            }
        }

        private EventHandler<FirebasePushNotificationTokenEventArgs> tokenRefreshEventHandler;

        public event EventHandler<FirebasePushNotificationTokenEventArgs> TokenRefreshed
        {
            add
            {
                this.DequeueAndSubscribe(value, ref this.tokenRefreshEventHandler, this.tokenRefreshQueue);
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

        private EventHandler<FirebasePushNotificationDataEventArgs> notificationReceivedEventHandler;

        public event EventHandler<FirebasePushNotificationDataEventArgs> NotificationReceived
        {
            add
            {
                this.DequeueAndSubscribe(value, ref this.notificationReceivedEventHandler, this.notificationReceivedQueue);
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

        private EventHandler<FirebasePushNotificationDataEventArgs> notificationDeletedEventHandler;

        public event EventHandler<FirebasePushNotificationDataEventArgs> NotificationDeleted
        {
            add
            {
                this.DequeueAndSubscribe(value, ref this.notificationDeletedEventHandler, this.notificationDeletedQueue);
            }
            remove => this.notificationDeletedEventHandler -= value;
        }

        public void HandleNotificationError(FirebasePushNotificationErrorType type, string message)
        {
            this.logger.LogDebug("HandleNotificationError");

            this.RaiseOrQueueEvent(
                this.notificationErrorEventHandler,
                () => new FirebasePushNotificationErrorEventArgs(type, message),
                this.notificationErrorQueue,
                nameof(NotificationError));

            // TODO: Extend interface
            this.NotificationHandler?.OnError(/*type,*/ message);
        }

        private EventHandler<FirebasePushNotificationErrorEventArgs> notificationErrorEventHandler;

        public event EventHandler<FirebasePushNotificationErrorEventArgs> NotificationError
        {
            add
            {
                this.DequeueAndSubscribe(value, ref this.notificationErrorEventHandler, this.notificationErrorQueue);
            }
            remove
            {
                this.notificationErrorEventHandler -= value;
            }
        }

        public void HandleNotificationOpened(IDictionary<string, object> data, string identifier, NotificationCategoryType notificationCategoryType)
        {
            this.logger.LogDebug("HandleNotificationOpened");

            this.RaiseOrQueueEvent(
                this.notificationOpenedEventHandler,
                () => new FirebasePushNotificationResponseEventArgs(data, identifier, notificationCategoryType),
                this.notificationOpenedQueue,
                nameof(NotificationOpened));

            this.OnNotificationOpened(data, identifier, notificationCategoryType);

            this.NotificationHandler?.OnOpened(data, identifier, notificationCategoryType);
        }

        protected virtual void OnNotificationOpened(IDictionary<string, object> data, string identifier, NotificationCategoryType notificationCategoryType)
        {
        }

        private EventHandler<FirebasePushNotificationResponseEventArgs> notificationOpenedEventHandler;

        public event EventHandler<FirebasePushNotificationResponseEventArgs> NotificationOpened
        {
            add
            {
                this.DequeueAndSubscribe(value, ref this.notificationOpenedEventHandler, this.notificationOpenedQueue);
            }
            remove
            {
                this.notificationOpenedEventHandler -= value;
            }
        }

        public void HandleNotificationAction(IDictionary<string, object> data, string identifier, NotificationCategoryType notificationCategoryType)
        {
            this.logger.LogDebug("HandleNotificationAction");

            this.RaiseOrQueueEvent(
                this.notificationActionEventHandler,
                () => new FirebasePushNotificationResponseEventArgs(data, identifier, notificationCategoryType),
                this.notificationActionQueue,
                nameof(NotificationAction));

            this.OnNotificationAction(data);
            
            // TODO: Extend interface
            //this.NotificationHandler?.OnAction(data);
        }

        protected virtual void OnNotificationAction(IDictionary<string, object> data)
        {
        }

        private EventHandler<FirebasePushNotificationResponseEventArgs> notificationActionEventHandler;

        public event EventHandler<FirebasePushNotificationResponseEventArgs> NotificationAction
        {
            add
            {
                this.DequeueAndSubscribe(value, ref this.notificationActionEventHandler, this.notificationActionQueue);
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
            IQueue<TEventArgs> queue) where TEventArgs : EventArgs
        {
            if (queue != null)
            {
                var previousSubscriptions = eventHandler;
                eventHandler += value;

                if (previousSubscriptions == null && eventHandler != null)
                {
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
    }
}
