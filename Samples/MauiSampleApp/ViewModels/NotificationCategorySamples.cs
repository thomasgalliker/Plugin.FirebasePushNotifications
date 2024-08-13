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
            yield return new NotificationCategory("contract", new[]
            {
                new NotificationAction("Accept", "Accept", NotificationActionType.Default, "accept"),
                new NotificationAction("Reject", "Reject", NotificationActionType.Default, "reject")
            });
            yield return new NotificationCategory("dismiss",new []
            {
                new NotificationAction("dismiss","Dismiss", NotificationActionType.Default),
            });
            yield return new NotificationCategory("navigate", new []
            {
                new NotificationAction("dismiss", "Dismiss", NotificationActionType.Default),
                new NotificationAction("navigate", "Navigate To", NotificationActionType.Foreground)
            });
        }
    }
}