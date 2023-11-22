namespace Plugin.FirebasePushNotifications.Internals
{
    internal class DirectoryInfoFactory : IDirectoryInfoFactory
    {
        private static readonly Lazy<IDirectoryInfoFactory> Implementation = new Lazy<IDirectoryInfoFactory>(
            () => new DirectoryInfoFactory(), 
            LazyThreadSafetyMode.PublicationOnly);

        internal static IDirectoryInfoFactory Current
        {
            get => Implementation.Value;
        }

        private DirectoryInfoFactory()
        {
        }

        public IDirectoryInfo FromPath(string path)
        {
            return new DirectoryInfoWrapper(path);
        }
    }
}