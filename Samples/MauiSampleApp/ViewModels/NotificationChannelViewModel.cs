#if ANDROID
using Plugin.FirebasePushNotifications.Platforms.Channels;
using Android.App;
using NotificationImportance = Android.App.NotificationImportance;
using NotificationVisibility = Android.App.NotificationVisibility;
#else
using NotificationImportance = object;
using NotificationVisibility = object;
#endif

namespace MauiSampleApp.ViewModels
{
    public class NotificationChannelViewModel
    {
#if ANDROID
        public NotificationChannelViewModel(NotificationChannel notificationChannel)
        {
            this.ChannelId = notificationChannel.Id;
            this.ChannelName = notificationChannel.Name;
            this.Description = notificationChannel.Description;
            this.LockscreenVisibility = notificationChannel.LockscreenVisibility;
            this.Group = notificationChannel.Group;
            this.Importance = notificationChannel.Importance;
        }
#endif

        public string ChannelId { get; }

        public string ChannelName { get; }

        public string Description { get; }

        public NotificationVisibility LockscreenVisibility { get; }

        public string Group { get; }

        public NotificationImportance Importance { get; }
    }
}