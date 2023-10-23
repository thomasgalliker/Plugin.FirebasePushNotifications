using MauiSampleApp.Views;

namespace MauiSampleApp
{
    public partial class App : Application
    {
        public App(MainPage mainPage)
        {
            this.InitializeComponent();

            this.MainPage = new NavigationPage(mainPage);
        }
    }
}