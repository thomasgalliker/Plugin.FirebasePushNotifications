namespace Plugin.FirebasePushNotifications.Internals
{
    internal interface IFileInfoFactory
    {
        IFileInfo FromPath(string path);
    }
}