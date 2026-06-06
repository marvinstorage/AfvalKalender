using AfvalKalender.AndroidUI.ViewModels;

namespace AfvalKalender.AndroidUI.Views;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
