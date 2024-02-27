namespace Plugin.FirebasePushNotifications.Internals
{
    internal interface IDirectoryInfoFactory
    {
        IDirectoryInfo FromPath(string path);
    }
}