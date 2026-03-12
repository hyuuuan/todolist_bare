namespace ToDoMaui_Listview;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		if (AuthService.Instance.IsSignedIn)
		{
			return new Window(new AppShell());
		}

		var signInPage = new SignInPage();
		NavigationPage.SetHasNavigationBar(signInPage, false);
		return new Window(new NavigationPage(signInPage));
	}
}
