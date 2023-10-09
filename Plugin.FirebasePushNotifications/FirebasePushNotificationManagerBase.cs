#if IOS
using Foundation;
#endif

using Microsoft.Extensions.Logging;
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

        private IQueue<FirebasePushNotificationDataEventArgs> onReceivedQueue;
        private IQueue<FirebasePushNotificationTokenEventArgs> onTokenRefreshQueue;

        protected FirebasePushNotificationManagerBase(
            ILogger<FirebasePushNotificationManager> logger,
            IQueueFactory queueFactory)
        {
            this.logger = logger;
            this.CreateOrUpdateQueues(queueFactory);
        }

        private void CreateOrUpdateQueues(IQueueFactory queueFactory)
        {
            // Clear queues (if any exist)
            this.onReceivedQueue?.Clear();
            this.onTokenRefreshQueue?.Clear();

            if (queueFactory != null)
            {
                // Create new queues
                this.onReceivedQueue = queueFactory.Create<FirebasePushNotificationDataEventArgs>();
                this.onTokenRefreshQueue = queueFactory.Create<FirebasePushNotificationTokenEventArgs>();
            }
            else
            {
                // Remove queues
                this.onReceivedQueue = null;
                this.onTokenRefreshQueue = null;
            }
        }

        public virtual void Configure(FirebasePushNotificationOptions options)
        {
            this.logger.LogDebug("Configure");

            this.CreateOrUpdateQueues(options.QueueFactory);
        }

        /// <inheritdoc/>
        public ILogger<FirebasePushNotificationManager> Logger
        {
            set => this.logger = value;
        }

        protected internal FirebasePushNotificationTokenEventHandler onTokenRefresh;
        public event FirebasePushNotificationTokenEventHandler OnTokenRefresh
        {
            add
            {
                if (this.onTokenRefreshQueue is IQueue<FirebasePushNotificationTokenEventArgs> queue)
                {
                    var previousSubscriptions = this.onTokenRefresh;
                    this.onTokenRefresh += value;

                    if (previousSubscriptions == null && this.onTokenRefresh is FirebasePushNotificationTokenEventHandler eventHandler)
                    {
                        foreach (var data in queue.TryDequeueAll())
                        {
                            eventHandler.Invoke(this, data);
                        }
                    }
                }
                else
                {
                    this.onTokenRefresh += value;
                }
            }
            remove => this.onTokenRefresh -= value;
        }

        private FirebasePushNotificationDataEventHandler notificationReceivedEventHandler;

        protected FirebasePushNotificationDataEventHandler NotificationReceivedEventHandler
        {
            get
            {
                return this.notificationReceivedEventHandler ?? this.Enqueue;
            }
        }

        private void Enqueue(object source, FirebasePushNotificationDataEventArgs args)
        {
            if (this.onReceivedQueue is IQueue<FirebasePushNotificationDataEventArgs> queue)
            {
                queue.Enqueue(args);
            }
        }

        public event FirebasePushNotificationDataEventHandler OnNotificationReceived
        {
            add
            {
                if (this.onReceivedQueue is IQueue<FirebasePushNotificationDataEventArgs> queue)
                {
                    var previousSubscriptions = this.notificationReceivedEventHandler;
                    this.notificationReceivedEventHandler += value;

                    if (previousSubscriptions == null && 
                        this.notificationReceivedEventHandler is FirebasePushNotificationDataEventHandler eventHandler)
                    {
                        foreach (var data in queue.TryDequeueAll())
                        {
                            eventHandler.Invoke(this, data);
                        }
                    }
                }
                else
                {
                    this.notificationReceivedEventHandler += value;
                }
            }
            remove => this.notificationReceivedEventHandler -= value;
        }


        protected internal FirebasePushNotificationDataEventHandler onNotificationDeleted;
        public event FirebasePushNotificationDataEventHandler OnNotificationDeleted
        {
            add
            {
                this.onNotificationDeleted += value;
            }
            remove
            {
                this.onNotificationDeleted -= value;
            }
        }

        protected internal FirebasePushNotificationErrorEventHandler onNotificationError;
        public event FirebasePushNotificationErrorEventHandler OnNotificationError
        {
            add
            {
                this.onNotificationError += value;
            }
            remove
            {
                this.onNotificationError -= value;
            }
        }

        protected internal FirebasePushNotificationResponseEventHandler onNotificationOpened;
        public event FirebasePushNotificationResponseEventHandler OnNotificationOpened
        {
            add
            {
                this.onNotificationOpened += value;
            }
            remove
            {
                this.onNotificationOpened -= value;
            }
        }

        protected internal FirebasePushNotificationResponseEventHandler onNotificationAction;

        public event FirebasePushNotificationResponseEventHandler OnNotificationAction
        {
            add
            {
                this.onNotificationAction += value;
            }
            remove
            {
                this.onNotificationAction -= value;
            }
        }

    }
}
