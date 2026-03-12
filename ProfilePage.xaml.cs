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

        var auth = AuthService.Instance;
        NameLabel.Text = string.IsNullOrWhiteSpace(auth.CurrentUserName) ? "Student" : auth.CurrentUserName;
        EmailLabel.Text = string.IsNullOrWhiteSpace(auth.CurrentUserEmail) ? "student@todo.app" : auth.CurrentUserEmail;
    }

    private void OnSignOutClicked(object? sender, EventArgs e)
    {
        AuthService.Instance.SignOut();
        AppNavigator.ShowSignIn();
    }
}
