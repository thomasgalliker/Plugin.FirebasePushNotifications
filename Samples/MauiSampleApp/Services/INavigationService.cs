namespace MauiSampleApp.Services
{
    public interface INavigationService
    {
        /// <summary>
        /// Pushes page <typeparamref name="TPage"/> to the navigation stack.
        /// </summary>
        /// <typeparam name="TPage">The page to be navigated to.</typeparam>
        Task PushAsync<TPage>() where TPage : Page;

        /// <summary>
        /// Pops back from the current page.
        /// </summary>
        Task PopAsync();
    }
}