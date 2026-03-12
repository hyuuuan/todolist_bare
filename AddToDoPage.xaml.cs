namespace ToDoMaui_Listview;

public partial class AddToDoPage : ContentPage
{
    private readonly ToDoStore _store = ToDoStore.Instance;

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
        if (!AuthService.Instance.IsSignedIn)
        {
            AppNavigator.ShowSignIn();
            return;
        }

        var title = (TitleEntry.Text ?? string.Empty).Trim();
        var details = (DetailsEditor.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Add", "Please enter a title.", "OK");
            return;
        }

        _store.AddItem(title, details, AuthService.Instance.CurrentUserId);
        await Shell.Current.GoToAsync("..");
    }
}
