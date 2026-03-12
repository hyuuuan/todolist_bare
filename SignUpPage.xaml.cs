namespace ToDoMaui_Listview;

public partial class SignUpPage : ContentPage
{
    public SignUpPage()
    {
        InitializeComponent();
    }

    private async void OnSignUpClicked(object? sender, EventArgs e)
    {
        var userName = UserNameEntry.Text ?? string.Empty;
        var email = EmailEntry.Text ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;
        var confirmPassword = ConfirmPasswordEntry.Text ?? string.Empty;

        var signedUp = AuthService.Instance.SignUp(userName, email, password, confirmPassword, out var errorMessage);
        if (!signedUp)
        {
            await DisplayAlertAsync("Sign up", errorMessage, "OK");
            return;
        }

        AppNavigator.ShowMainShell();
    }

    private async void OnGoToSignInClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
