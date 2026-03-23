namespace ToDoMaui_Listview;

public partial class ProfilePage : ContentPage
{
    private readonly ToDoStore _store = ToDoStore.Instance;

    public ProfilePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!AppNavigator.EnsureSignedIn())
        {
            return;
        }

        var (loaded, errorMessage) = await _store.RefreshAllAsync();
        if (!loaded && !string.IsNullOrWhiteSpace(errorMessage))
        {
            await DisplayAlertAsync("Profile", errorMessage, "OK");
        }

        var auth = AuthService.Instance;
        NameLabel.Text = string.IsNullOrWhiteSpace(auth.CurrentUserName) ? "User" : auth.CurrentUserName;
        EmailLabel.Text = string.IsNullOrWhiteSpace(auth.CurrentUserEmail) ? "-" : auth.CurrentUserEmail;
        PendingCountLabel.Text = _store.ActiveItems.Count.ToString();
        CompletedCountLabel.Text = _store.CompletedItems.Count.ToString();
    }

    private void OnSignOutClicked(object? sender, EventArgs e)
    {
        AuthService.Instance.SignOut();
        AppNavigator.ShowSignIn();
    }
}
