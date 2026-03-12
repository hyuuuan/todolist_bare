namespace ToDoMaui_Listview;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!AppNavigator.EnsureSignedIn())
        {
            return;
        }

        var auth = AuthService.Instance;
        NameLabel.Text = string.IsNullOrWhiteSpace(auth.CurrentUserName) ? "User" : auth.CurrentUserName;
        EmailLabel.Text = string.IsNullOrWhiteSpace(auth.CurrentUserEmail) ? "-" : auth.CurrentUserEmail;
    }

    private void OnSignOutClicked(object? sender, EventArgs e)
    {
        AuthService.Instance.SignOut();
        AppNavigator.ShowSignIn();
    }
}
