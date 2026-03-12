namespace ToDoMaui_Listview;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var signInPage = new SignInPage();
		NavigationPage.SetHasNavigationBar(signInPage, false);
		return new Window(new NavigationPage(signInPage));
	}
}
