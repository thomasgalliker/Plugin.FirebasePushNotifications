﻿using Plugin.FirebasePushNotifications.Model.Queues;

#if ANDROID
using Plugin.FirebasePushNotifications.Platforms;
#endif

namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationOptions
    {
        public virtual bool AutoInitEnabled { get; set; }

        /// <summary>
        /// The factory used to create new queues to intercept push notification events.
        /// Default: <c>null</c> (No queuing is used).
        /// </summary>
        /// <remarks>
        /// Use one of the predefined queues or create your own implementation of <see cref="IQueueFactory"/>.
        ///<list type="bullet">
        ///<item><see cref="InMemoryQueueFactory"/></item>
        ///<item><see cref="PersistentQueueFactory"/></item>
        ///</list>
        /// </remarks>
        public virtual IQueueFactory QueueFactory { get; set; }

#if ANDROID
        public virtual FirebasePushNotificationAndroidOptions Android { get; set; } = new FirebasePushNotificationAndroidOptions();
#endif

        public override string ToString()
        {
            return $"[{nameof(FirebasePushNotificationOptions)}: " +
                   $"{nameof(this.AutoInitEnabled)}={this.AutoInitEnabled}," +
                   $"{nameof(this.QueueFactory)}={this.QueueFactory?.GetType().FullName}"
                   ;
        }
    }
}