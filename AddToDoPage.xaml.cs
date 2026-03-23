namespace ToDoMaui_Listview;

public partial class AddToDoPage : ContentPage
{
    private readonly ToDoStore _store = ToDoStore.Instance;
    private bool _isSubmitting;

    public AddToDoPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        AppNavigator.EnsureSignedIn();
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        if (_isSubmitting)
        {
            return;
        }

        if (!AuthService.Instance.IsSignedIn)
        {
            AppNavigator.ShowSignIn();
            return;
        }

        _isSubmitting = true;
        if (sender is Button button)
        {
            button.IsEnabled = false;
        }

        try
        {
            var title = (TitleEntry.Text ?? string.Empty).Trim();
            var details = (DetailsEditor.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                await DisplayAlertAsync("Add", "Please enter a title.", "OK");
                return;
            }

            var (added, errorMessage) = await _store.AddItemAsync(title, details, AuthService.Instance.CurrentUserId);
            if (!added)
            {
                await DisplayAlertAsync("Add", errorMessage, "OK");
                return;
            }

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            _isSubmitting = false;
            if (sender is Button enabledButton)
            {
                enabledButton.IsEnabled = true;
            }
        }
    }
}
