using System.Diagnostics;

namespace Plugin.FirebasePushNotifications.Internals
{
    [DebuggerDisplay("{this.Name}{this.Extension}")]
    internal class FileInfoWrapper : IFileInfo
    {
        private readonly FileInfo fileInfo;

        public FileInfoWrapper(string path)
            : this(new FileInfo(path))
        {
        }

        public FileInfoWrapper(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }
        
        public string Name => this.fileInfo.Name;

        public string FullName => this.fileInfo.FullName;

        public string Extension => this.fileInfo.Extension;

        public bool Exists => this.fileInfo.Exists;

        public IDirectoryInfo Directory => new DirectoryInfoWrapper(this.fileInfo.Directory);

        public StreamWriter CreateText()
        {
            return this.fileInfo.CreateText();
        }

        public StreamReader OpenText()
        {
            return this.fileInfo.OpenText();
        }

        public void Delete()
        {
            this.fileInfo.Delete();
        }
    }
}