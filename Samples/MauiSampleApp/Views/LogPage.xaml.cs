using MauiSampleApp.ViewModels;

namespace MauiSampleApp.Views;

public partial class LogPage : ContentPage
{
    public LogPage(LogViewModel logViewModel)
    {
        this.InitializeComponent();
        this.BindingContext = logViewModel;
    }
}