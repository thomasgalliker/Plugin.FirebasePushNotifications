namespace MauiSampleApp.Services
{
    internal static class ApplicationExtensions
    {
        public static Page GetRootPage(this Application application)
        {
            return application.Windows.FirstOrDefault()?.Page
                ?? throw new InvalidOperationException("The application does not have an active window page.");
        }

        public static Page GetCurrentPage(this Application application)
        {
            var page = application.GetRootPage();
            return page is NavigationPage navigationPage ? navigationPage.CurrentPage : page;
        }
    }
}
