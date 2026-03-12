namespace ToDoMaui_Listview;

public partial class SignUpPage : ContentPage
{
    public SignUpPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (AuthService.Instance.HasRegisteredUser && !AuthService.Instance.IsSignedIn)
        {
            await DisplayAlertAsync("Sign up", "An account already exists. Please sign in.", "OK");
            await Navigation.PopAsync();
        }
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
