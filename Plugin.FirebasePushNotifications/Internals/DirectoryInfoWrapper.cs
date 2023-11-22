using System.Diagnostics;

namespace Plugin.FirebasePushNotifications.Internals
{
    [DebuggerDisplay("{this.directoryInfo.FullName}")]
    internal class DirectoryInfoWrapper : IDirectoryInfo
    {
        private readonly DirectoryInfo directoryInfo;

        public DirectoryInfoWrapper(string path)
            : this(new DirectoryInfo(path))
        {
        }
        
        public DirectoryInfoWrapper(DirectoryInfo directoryInfo)
        {
            this.directoryInfo = directoryInfo;
        }

        public bool Exists => this.directoryInfo.Exists;

        public string FullName => this.directoryInfo.FullName;

        public void Create()
        {
            this.directoryInfo.Create();
        }
    }
}