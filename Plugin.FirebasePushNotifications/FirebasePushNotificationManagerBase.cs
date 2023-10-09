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
        protected IQueueFactory queueFactory;

        protected FirebasePushNotificationManagerBase(
            ILogger<FirebasePushNotificationManager> logger,
            IQueueFactory queueFactory)
        {
            this.logger = logger;
            this.queueFactory = queueFactory;
        }

        public virtual void Configure(FirebasePushNotificationOptions options)
        {
            this.logger.LogDebug("Configure");
        }

        /// <inheritdoc/>
        public ILogger<FirebasePushNotificationManager> Logger
        {
            set => this.logger = value;
        }

        protected FirebasePushNotificationTokenEventHandler onTokenRefresh;
        public event FirebasePushNotificationTokenEventHandler OnTokenRefresh
        {
            add
            {
                this.onTokenRefresh += value;
            }
            remove
            {
                this.onTokenRefresh -= value;
            }
        }


        protected internal FirebasePushNotificationDataEventHandler onNotificationReceived;
        public event FirebasePushNotificationDataEventHandler OnNotificationReceived
        {
            add
            {
                this.onNotificationReceived += value;
            }
            remove
            {
                this.onNotificationReceived -= value;
            }
        }


        protected FirebasePushNotificationDataEventHandler onNotificationDeleted;
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

        protected FirebasePushNotificationErrorEventHandler onNotificationError;
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

        protected FirebasePushNotificationResponseEventHandler onNotificationOpened;
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

        protected FirebasePushNotificationResponseEventHandler onNotificationAction;
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
