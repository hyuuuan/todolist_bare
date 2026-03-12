namespace ToDoMaui_Listview;

public partial class SignInPage : ContentPage
{
    public SignInPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (AuthService.Instance.IsSignedIn)
        {
            AppNavigator.ShowMainShell();
        }
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text ?? string.Empty;
        var password = PasswordEntry.Text ?? string.Empty;

        if (!AuthService.Instance.SignIn(email, password, out var errorMessage))
        {
            await DisplayAlertAsync("Sign in", errorMessage, "OK");
            return;
        }

        AppNavigator.ShowMainShell();
    }

    private async void OnGoToSignUpClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }
}
