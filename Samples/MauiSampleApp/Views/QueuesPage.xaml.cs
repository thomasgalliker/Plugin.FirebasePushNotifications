using MauiSampleApp.ViewModels;

namespace MauiSampleApp.Views;

public partial class QueuesPage : ContentPage
{
	public QueuesPage(QueuesViewModel queuesViewModel)
	{
        this.InitializeComponent();
        this.BindingContext = queuesViewModel;
	}
}