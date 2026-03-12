namespace ToDoMaui_Listview;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class EditToDoPage : ContentPage
{
    private readonly ToDoStore _store = ToDoStore.Instance;
    private ToDoClass? _currentItem;

    private string _itemId = string.Empty;

    public string ItemId
    {
        get => _itemId;
        set
        {
            _itemId = Uri.UnescapeDataString(value ?? string.Empty);
            LoadItem();
        }
    }

    public EditToDoPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!AppNavigator.EnsureSignedIn())
        {
            return;
        }

        LoadItem();
    }

    private void LoadItem()
    {
        if (!int.TryParse(_itemId, out var id))
        {
            return;
        }

        var item = _store.ActiveItems.FirstOrDefault(x => x.ItemId == id);
        if (item == null)
        {
            return;
        }

        _currentItem = item;
        TitleEntry.Text = item.ItemName;
        DetailsEditor.Text = item.ItemDescription;
    }

    private async void OnUpdateClicked(object? sender, EventArgs e)
    {
        if (_currentItem == null)
        {
            return;
        }

        var title = (TitleEntry.Text ?? string.Empty).Trim();
        var details = (DetailsEditor.Text ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Edit", "Please enter a title.", "OK");
            return;
        }

        _store.UpdateItem(_currentItem, title, details);
        await Shell.Current.GoToAsync("..");
    }

    private async void OnCompleteClicked(object? sender, EventArgs e)
    {
        if (_currentItem == null)
        {
            return;
        }

        _store.MoveToCompleted(_currentItem);
        await Shell.Current.GoToAsync("..");
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_currentItem == null)
        {
            return;
        }

        _store.DeleteItem(_currentItem);
        await Shell.Current.GoToAsync("..");
    }
}
