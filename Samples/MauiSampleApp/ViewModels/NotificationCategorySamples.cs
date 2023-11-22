using Plugin.FirebasePushNotifications;

namespace MauiSampleApp.ViewModels
{
    public static class NotificationCategorySamples
    {
        public static IEnumerable<NotificationCategory> GetAll()
        {
            yield return new NotificationCategory("medication_intake", new[]
            {
                new NotificationAction("take_medication", "Take medicine", NotificationActionType.Foreground),
                new NotificationAction("skip_medication", "Skip medicine", NotificationActionType.Foreground),
            });
            yield return new NotificationCategory("meeting_invitation", new[]
            {
                new NotificationAction("accept", "Accept", NotificationActionType.Foreground),
                new NotificationAction("decline", "Decline", NotificationActionType.Destructive),
            });
            yield return new NotificationCategory("message", new[]
            {
                new NotificationAction("Reply", "Reply", NotificationActionType.Foreground),
                new NotificationAction("Forward", "Forward", NotificationActionType.Foreground)
            });
            yield return new NotificationCategory("request", new[]
            {
                new NotificationAction("Accept", "Accept", NotificationActionType.Default, "check"),
                new NotificationAction("Reject", "Reject", NotificationActionType.Default, "cancel")
             });
        }
    }
}