namespace Plugin.FirebasePushNotifications.Internals
{
    internal class FileInfoFactory : IFileInfoFactory
    {
        public IFileInfo FromPath(string path)
        {
            return new FileInfoWrapper(path);
        }
    }
}
