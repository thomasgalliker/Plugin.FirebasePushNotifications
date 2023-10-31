namespace Plugin.FirebasePushNotifications.Internals
{
    internal interface IDirectoryInfo
    {
        /// <inheritdoc cref="DirectoryInfo.Exists"/>
        bool Exists { get; }

        string FullName { get; }

        /// <inheritdoc cref="DirectoryInfo.Create"/>
        void Create();
    }
}