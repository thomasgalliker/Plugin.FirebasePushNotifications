namespace MauiSampleApp.Services
{
    public class MauiNavigationService : INavigationService
    {
        private readonly IServiceProvider serviceProvider;

        public MauiNavigationService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task PushAsync<TPage>() where TPage : Page
        {
            var page = this.serviceProvider.GetRequiredService<TPage>();
            var application = Application.Current ?? throw new InvalidOperationException("Application.Current is not available.");
            await application.GetRootPage().Navigation.PushAsync(page);
        }

        public async Task PopAsync()
        {
            var application = Application.Current ?? throw new InvalidOperationException("Application.Current is not available.");
            await application.GetRootPage().Navigation.PopAsync();
        }
    }
}
