using System.Text;

namespace MauiSampleApp.Services.Logging
{
    public class NLogFileReader : ILogFileReader
    {
        public NLogFileReader(string filePath)
        {
            this.FilePath = filePath;
        }

        public string FilePath { get; }

        public string ReadLogFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            using (var streamReader = new StreamReader(filePath))
            {
                var content = streamReader.ReadToEnd();
                return content;
            }
        }

        public Task<string> ReadLogFileAsync(long numberOfLines)
        {
            return this.ReadLogFileAsync(this.FilePath, numberOfLines);
        }

        public async Task<string> ReadLogFileAsync(string filePath, long numberOfLines)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(filePath);
            }

            string fileContent;

            if (numberOfLines >= 0)
            {
                fileContent = ReadLastLines(filePath, numberOfLines);
            }
            else
            {
                using (var streamReader = new StreamReader(filePath))
                {
                    fileContent = await streamReader.ReadToEndAsync();
                }
            }

            return fileContent;
        }

        public IEnumerable<string> EnumerateLogFiles()
        {
            var logDir = Path.GetDirectoryName(this.FilePath);
            var logDirInfo = new DirectoryInfo(logDir);
            if (logDirInfo.Exists)
            {
                var logFiles = logDirInfo.EnumerateFiles("*.log", SearchOption.AllDirectories);
                return logFiles.Select(f => f.FullName);
            }

            return Enumerable.Empty<string>();
        }

        public int DeleteLogFiles()
        {
            var logFiles = this.EnumerateLogFiles().ToArray();
            foreach (var logFile in logFiles)
            {
                File.Delete(logFile);
            }

            return logFiles.Length;
        }

        /// <summary>
        /// Returns the last N lines from a text file <paramref name="path"/>.
        /// </summary>
        private static string ReadLastLines(string path, long numberOfTokens, Encoding encoding = default, string tokenSeparator = "\n")
        {
            encoding ??= Encoding.UTF8;

            var sizeOfChar = encoding.GetByteCount("\n");
            var buffer = encoding.GetBytes(tokenSeparator);

            using (var fs = new FileStream(path, FileMode.Open))
            {
                var tokenCount = 0L;
                var endPosition = fs.Length / sizeOfChar;

                for (long position = sizeOfChar; position < endPosition; position += sizeOfChar)
                {
                    fs.Seek(-position, SeekOrigin.End);
                    _ = fs.Read(buffer, 0, buffer.Length);

                    if (encoding.GetString(buffer) == tokenSeparator)
                    {
                        if (tokenCount++ == numberOfTokens)
                        {
                            var returnBuffer = new byte[fs.Length - fs.Position];
                            _ = fs.Read(returnBuffer, 0, returnBuffer.Length);
                            return encoding.GetString(returnBuffer);
                        }
                    }
                }

                // handle case where number of tokens in file is less than numberOfTokens
                fs.Seek(0, SeekOrigin.Begin);
                buffer = new byte[fs.Length];
                _ = fs.Read(buffer, 0, buffer.Length);
                return encoding.GetString(buffer);
            }
        }
    }
}