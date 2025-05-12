namespace Plugin.FirebasePushNotifications.Platforms.Channels
{
    internal enum NotificationChannelCheckResult
    {
        Success,
        DuplicateChannelIds,
        MultipleDefaultChannels,
        NoDefaultChannel,
    }

    internal static class NotificationChannelHelper
    {
        internal static (NotificationChannelCheckResult Result, Exception Exception) CheckNotificationChannelRequests(
            (string ChannelId, bool IsDefault)[] notificationChannels,
            string methodName, string paramName)
        {
            var duplicateChannelIds = notificationChannels
                .Select(c => c.ChannelId)
                .GroupBy(c => c)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            if (duplicateChannelIds.Any())
            {
                var exception = new ArgumentException(
                    $"{methodName} failed: {paramName} contains {nameof(NotificationChannelRequest)} with duplicate {nameof(NotificationChannelRequest.ChannelId)}: " +
                    $"[{string.Join(", ", duplicateChannelIds.Select(id => $"\"{id}\""))}]",
                    paramName);

                return (NotificationChannelCheckResult.DuplicateChannelIds, exception);
            }

            var defaultNotificationChannels = notificationChannels.Where(c => c.IsDefault).ToArray();
            if (defaultNotificationChannels.Length > 1)
            {
                var exception = new ArgumentException(
                    $"{methodName} failed: {paramName} contains more than one active {nameof(NotificationChannelRequest)} with {nameof(NotificationChannelRequest.IsDefault)}=true" +
                    $"[{string.Join(", ", defaultNotificationChannels.Select(c => $"\"{c.ChannelId}\""))}]",
                    paramName);

                return (NotificationChannelCheckResult.MultipleDefaultChannels, exception);
            }

            if (defaultNotificationChannels.Length < 1)
            {
                var exception = new ArgumentException(
                    $"{methodName} failed: {paramName} does not contain any active {nameof(NotificationChannelRequest)} with {nameof(NotificationChannelRequest.IsDefault)}=true",
                    paramName);

                return (NotificationChannelCheckResult.NoDefaultChannel, exception);
            }

            return (NotificationChannelCheckResult.Success, null);
        }

        internal static void EnsureNotificationChannelRequests(
            (string ChannelId, bool IsDefault)[] notificationChannels,
            string methodName, string paramName)
        {
            var checkResult = CheckNotificationChannelRequests(notificationChannels, methodName, paramName);
            if (checkResult.Result != NotificationChannelCheckResult.Success)
            {
                throw checkResult.Exception;
            }
        }
    }
}