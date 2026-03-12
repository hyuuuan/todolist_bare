namespace ToDoMaui_Listview;

public static class AppNavigator
{
    public static void ShowMainShell()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window != null)
        {
            window.Page = new AppShell();
        }
    }

    public static void ShowSignIn()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window == null)
        {
            return;
        }

        var signInPage = new SignInPage();
        NavigationPage.SetHasNavigationBar(signInPage, false);
        window.Page = new NavigationPage(signInPage);
    }
}
