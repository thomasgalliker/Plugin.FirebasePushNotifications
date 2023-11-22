namespace Plugin.FirebasePushNotifications.Internals
{
    internal interface IFileInfo
    {
        string Name { get; }

        string FullName { get; }

        string Extension { get; }

        bool Exists { get; }

        IDirectoryInfo Directory { get; }

        StreamWriter CreateText();

        StreamReader OpenText();

        void Delete();
    }
}