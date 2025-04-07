using Plugin.FirebasePushNotifications.Model.Queues;
using Plugin.FirebasePushNotifications.Platforms;

namespace Plugin.FirebasePushNotifications
{
    public class FirebasePushNotificationOptions
    {
        /// <summary>
        /// Indicates if the underlying Firebase library should be initialized automatically.
        /// Default: <c>true</c>
        /// </summary>
        public virtual bool AutoInitEnabled { get; set; } = true;

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
        /// <summary>
        /// Android-specific options.
        /// </summary>
        public virtual FirebasePushNotificationAndroidOptions Android { get; } = new FirebasePushNotificationAndroidOptions();
#elif IOS
        /// <summary>
        /// iOS-specific options.
        /// </summary>
        public virtual FirebasePushNotificationiOSOptions iOS { get; } = new FirebasePushNotificationiOSOptions();
#endif

        public override string ToString()
        {
            return $"[{nameof(FirebasePushNotificationOptions)}: " +
                   $"{nameof(this.AutoInitEnabled)}={this.AutoInitEnabled}," +
                   $"{nameof(this.QueueFactory)}={this.QueueFactory?.GetType().FullName}]"
                   ;
        }
    }
}