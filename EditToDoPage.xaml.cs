namespace ToDoMaui_Listview;

[QueryProperty(nameof(ItemId), "itemId")]
public partial class EditToDoPage : ContentPage
{
    private readonly ToDoStore _store = ToDoStore.Instance;
    private ToDoClass? _currentItem;
    private bool _isSubmitting;

    private string _itemId = string.Empty;

    public string ItemId
    {
        get => _itemId;
        set
        {
            _itemId = Uri.UnescapeDataString(value ?? string.Empty);
        }
    }

    public EditToDoPage()
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

        await EnsureItemLoadedAsync();
    }

    private void LoadItemFromStore()
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

    private async Task EnsureItemLoadedAsync()
    {
        LoadItemFromStore();
        if (_currentItem != null)
        {
            return;
        }

        var (loaded, errorMessage) = await _store.RefreshActiveAsync();
        if (!loaded)
        {
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                await DisplayAlertAsync("Edit", errorMessage, "OK");
            }

            return;
        }

        LoadItemFromStore();
        if (_currentItem == null)
        {
            await DisplayAlertAsync("Edit", "Task not found.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private async void OnUpdateClicked(object? sender, EventArgs e)
    {
        if (_isSubmitting || _currentItem == null)
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
            var title = (TitleEntry.Text ?? string.Empty).Trim();
            var details = (DetailsEditor.Text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                await DisplayAlertAsync("Edit", "Please enter a title.", "OK");
                return;
            }

            var (updated, errorMessage) = await _store.UpdateItemAsync(_currentItem, title, details);
            if (!updated)
            {
                await DisplayAlertAsync("Edit", errorMessage, "OK");
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

    private async void OnCompleteClicked(object? sender, EventArgs e)
    {
        if (_isSubmitting || _currentItem == null)
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
            var (completed, errorMessage) = await _store.MoveToCompletedAsync(_currentItem);
            if (!completed)
            {
                await DisplayAlertAsync("Edit", errorMessage, "OK");
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

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_isSubmitting || _currentItem == null)
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
            var (deleted, errorMessage) = await _store.DeleteItemAsync(_currentItem);
            if (!deleted)
            {
                await DisplayAlertAsync("Edit", errorMessage, "OK");
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
