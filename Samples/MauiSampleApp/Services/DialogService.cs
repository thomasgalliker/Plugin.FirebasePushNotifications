namespace MauiSampleApp.Services
{
    public class DialogService : IDialogService
    {
        public Task ShowDialogAsync(string title, string message, string cancel)
        {
            return MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(title, message, cancel);
            });
        }
    }
}