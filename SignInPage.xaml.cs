namespace ToDoMaui_Listview;

public partial class SignInPage : ContentPage
{
    private bool _isSubmitting;

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
        if (_isSubmitting)
        {
            return;
        }

        _isSubmitting = true;
        if (sender is Button button)
        {
            button.IsEnabled = false;
        }

        try
        {
            var email = EmailEntry.Text ?? string.Empty;
            var password = PasswordEntry.Text ?? string.Empty;

            var (signedIn, errorMessage) = await AuthService.Instance.SignInAsync(email, password);
            if (!signedIn)
            {
                await DisplayAlertAsync("Sign in", errorMessage, "OK");
                return;
            }

            var (loaded, loadErrorMessage) = await ToDoStore.Instance.RefreshAllAsync();
            if (!loaded && !string.IsNullOrWhiteSpace(loadErrorMessage))
            {
                await DisplayAlertAsync("Sign in", $"Signed in, but tasks failed to load: {loadErrorMessage}", "OK");
            }

            AppNavigator.ShowMainShell();
        }
        finally
        {
            _isSubmitting = false;
            if (sender is Button enabledButton)
            {
                enabledButton.IsEnabled = true;
            }

            PasswordEntry.Text = string.Empty;
        }
    }

    private async void OnGoToSignUpClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());
    }
}
