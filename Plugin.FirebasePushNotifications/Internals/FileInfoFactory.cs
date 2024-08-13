namespace Plugin.FirebasePushNotifications.Internals
{
    internal class FileInfoFactory : IFileInfoFactory
    {
        private static readonly Lazy<IFileInfoFactory> Implementation = new Lazy<IFileInfoFactory>(
           () => new FileInfoFactory(),
           LazyThreadSafetyMode.PublicationOnly);

        internal static IFileInfoFactory Current
        {
            get => Implementation.Value;
        }

        private FileInfoFactory()
        {
        }

        public IFileInfo FromPath(string path)
        {
            return new FileInfoWrapper(path);
        }
    }
}
