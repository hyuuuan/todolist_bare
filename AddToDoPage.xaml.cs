namespace ToDoMaui_Listview;

public partial class AddToDoPage : ContentPage
{
    private readonly ToDoStore _store = ToDoStore.Instance;

    public AddToDoPage()
    {
        InitializeComponent();
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        var title = (TitleEntry.Text ?? string.Empty).Trim();
        var details = (DetailsEditor.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Add", "Please enter a title.", "OK");
            return;
        }

        _store.AddItem(title, details, AuthService.Instance.CurrentUserId > 0 ? AuthService.Instance.CurrentUserId : 1);
        await Shell.Current.GoToAsync("..");
    }
}
