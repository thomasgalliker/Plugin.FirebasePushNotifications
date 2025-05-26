using MauiSampleApp.ViewModels;
using MauiSampleApp.Views;

namespace MauiSampleApp
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            this.InitializeComponent();

            var mainPage = serviceProvider.GetRequiredService<MainPage>();
            this.MainPage = new NavigationPage(mainPage);
        }

        protected override void OnResume()
        {
            if (this.MainPage is NavigationPage { CurrentPage: MainPage { BindingContext: MainViewModel mainViewModel } })
            {
                mainViewModel.OnResume();
            }
        }
    }
}