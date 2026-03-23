namespace ToDoMaui_Listview;

public partial class SignUpPage : ContentPage
{
    private bool _isSubmitting;

    public SignUpPage()
    {
        InitializeComponent();
    }

    private async void OnSignUpClicked(object? sender, EventArgs e)
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
            var firstName = FirstNameEntry.Text ?? string.Empty;
            var lastName = LastNameEntry.Text ?? string.Empty;
            var email = EmailEntry.Text ?? string.Empty;
            var password = PasswordEntry.Text ?? string.Empty;
            var confirmPassword = ConfirmPasswordEntry.Text ?? string.Empty;

            var (signedUp, errorMessage) = await AuthService.Instance.SignUpAsync(
                firstName,
                lastName,
                email,
                password,
                confirmPassword);

            if (!signedUp)
            {
                await DisplayAlertAsync("Sign up", errorMessage, "OK");
                return;
            }

            var (loaded, loadErrorMessage) = await ToDoStore.Instance.RefreshAllAsync();
            if (!loaded && !string.IsNullOrWhiteSpace(loadErrorMessage))
            {
                await DisplayAlertAsync("Sign up", $"Account created, but tasks failed to load: {loadErrorMessage}", "OK");
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
            ConfirmPasswordEntry.Text = string.Empty;
        }
    }

    private async void OnGoToSignInClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
