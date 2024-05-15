namespace MauiSampleApp.Services.Logging
{
    public interface ILogFileReader
    {
        string FilePath { get; }

        string ReadLogFile(string filePath);

        Task<string> ReadLogFileAsync(long numberOfLines);

        IEnumerable<string> EnumerateLogFiles();

        int DeleteLogFiles();
    }
}