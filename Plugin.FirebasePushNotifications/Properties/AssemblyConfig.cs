using System.Runtime.CompilerServices;
using Plugin.FirebasePushNotifications;

[assembly: InternalsVisibleTo("Plugin.FirebasePushNotifications.Tests")]
[assembly: InternalsVisibleTo("MauiSampleApp")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

[assembly: Preserve(typeof(IFirebasePushNotification), AllMembers = true)]
[assembly: Preserve(typeof(INotificationPermissions), AllMembers = true)]
[assembly: Preserve(typeof(IFirebasePushNotificationPreferences), AllMembers = true)]