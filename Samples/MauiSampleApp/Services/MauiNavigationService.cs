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
            await Application.Current.MainPage.Navigation.PushAsync(page);
        }

        public async Task PopAsync()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}