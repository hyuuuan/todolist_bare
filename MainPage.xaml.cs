namespace ToDoMaui_Listview;

public partial class MainPage : ContentPage
{
    private readonly ToDoStore _store = ToDoStore.Instance;
    private ToDoClass? _selectedItem;
    private bool _isSubmitting;

    public MainPage()
    {
        InitializeComponent();
        todoLV.ItemsSource = _store.ActiveItems;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!AppNavigator.EnsureSignedIn())
        {
            return;
        }

        var (loaded, errorMessage) = await _store.RefreshActiveAsync();
        if (!loaded && !string.IsNullOrWhiteSpace(errorMessage))
        {
            await DisplayAlertAsync("Tasks", errorMessage, "OK");
        }
    }

    private async void AddToDoItem(object? sender, EventArgs e)
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

        var title = titleEntry.Text?.Trim() ?? string.Empty;
        var detail = detailsEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
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
            var (added, errorMessage) = await _store.AddItemAsync(title, detail, AuthService.Instance.CurrentUserId);
            if (!added)
            {
                await DisplayAlertAsync("Tasks", errorMessage, "OK");
                return;
            }

            ClearInputs();
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

    private async void EditToDoItem(object? sender, EventArgs e)
    {
        if (_isSubmitting || _selectedItem == null)
        {
            return;
        }

        var title = titleEntry.Text?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        var detail = detailsEditor.Text?.Trim() ?? string.Empty;

        _isSubmitting = true;
        if (sender is Button button)
        {
            button.IsEnabled = false;
        }

        try
        {
            var (updated, errorMessage) = await _store.UpdateItemAsync(_selectedItem, title, detail);
            if (!updated)
            {
                await DisplayAlertAsync("Tasks", errorMessage, "OK");
                return;
            }

            todoLV.SelectedItem = null;
            CancelEditMode();
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

    private void CancelEdit(object? sender, EventArgs e)
    {
        todoLV.SelectedItem = null;
        CancelEditMode();
    }

    private async void DeleteToDoItem(object? sender, EventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.ClassId, out int id))
        {
            var item = _store.ActiveItems.FirstOrDefault(t => t.ItemId == id);
            if (item != null)
            {
                var (deleted, errorMessage) = await _store.DeleteItemAsync(item);
                if (!deleted)
                {
                    await DisplayAlertAsync("Tasks", errorMessage, "OK");
                    return;
                }

                if (_selectedItem?.ItemId == id)
                {
                    todoLV.SelectedItem = null;
                    CancelEditMode();
                }
            }
        }
    }

    private void TodoLV_OnItemSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ToDoClass item)
        {
            _selectedItem      = item;
            titleEntry.Text    = item.title;
            detailsEditor.Text = item.detail;
            SetEditMode(true);
        }
        // Clear the selection immediately so iOS doesn't show the checkmark
        todoLV.SelectedItem = null;
    }

    // ── Helpers ──────────────────────────────────────────────

    private void SetEditMode(bool editing)
    {
        addBtn.IsVisible    = !editing;
        editBtn.IsVisible   =  editing;
        cancelBtn.IsVisible =  editing;
    }

    private void CancelEditMode()
    {
        _selectedItem = null;
        SetEditMode(false);
        ClearInputs();
    }

    private void ClearInputs()
    {
        titleEntry.Text    = string.Empty;
        detailsEditor.Text = string.Empty;
    }
}
