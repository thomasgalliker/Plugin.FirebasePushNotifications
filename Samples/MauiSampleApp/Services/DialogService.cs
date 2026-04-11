namespace MauiSampleApp.Services
{
    public class DialogService : IDialogService
    {
        public Task ShowDialogAsync(string title, string message, string cancel)
        {
            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var application = Application.Current ?? throw new InvalidOperationException("Application.Current is not available.");
                await application.GetCurrentPage().DisplayAlertAsync(title, message, cancel);
            });
        }
    }
}
