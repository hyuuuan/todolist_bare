namespace ToDoMaui_Listview;

public partial class SignInPage : ContentPage
{
    public SignInPage()
    {
        InitializeComponent();
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;

        if (!AuthService.Instance.SignIn(email, password))
        {
            await DisplayAlertAsync("Sign in", "Enter valid credentials to continue.", "OK");
            return;
        }

        AppNavigator.ShowMainShell();
    }

    private async void OnGoToSignUpClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }
}
