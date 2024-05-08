namespace MauiSampleApp.Services.Logging
{
    public class NLogFileReader : ILogFileReader
    {
        public NLogFileReader(string filePath)
        {
            this.FilePath = filePath;
        }

        public string FilePath { get; }

        public async Task<string> ReadLogFileAsync()
        {
            if (!File.Exists(this.FilePath))
            {
                throw new FileNotFoundException(this.FilePath);
            }

            using (var streamReader = new StreamReader(this.FilePath))
            {
                var content = await streamReader.ReadToEndAsync();
                return content;
            }
        }

        public async Task FlushLogFileAsync()
        {
            if (!File.Exists(this.FilePath))
            {
                throw new FileNotFoundException(this.FilePath);
            }

            using (var streamWriter = new StreamWriter(this.FilePath))
            {
                await streamWriter.FlushAsync();
            }
        }
    }
}