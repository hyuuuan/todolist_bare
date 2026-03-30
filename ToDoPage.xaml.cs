namespace ToDoMaui_Listview;

public partial class ToDoPage : ContentPage
{
    private readonly ToDoStore _store = ToDoStore.Instance;
    private bool _isLoading;

    public ToDoPage()
    {
        InitializeComponent();
        ToDoListView.ItemsSource = _store.ActiveItems;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!AppNavigator.EnsureSignedIn())
        {
            return;
        }

        await LoadActiveItemsAsync();
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddToDoPage));
    }

    private async void OnItemTapped(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is ToDoClass item)
        {
            await Shell.Current.GoToAsync($"{nameof(EditToDoPage)}?itemId={item.ItemId}");
        }

        if (sender is CollectionView collectionView)
        {
            collectionView.SelectedItem = null;
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || !int.TryParse(button.ClassId, out var id))
        {
            return;
        }

        var item = _store.ActiveItems.FirstOrDefault(x => x.ItemId == id);
        if (item == null)
        {
            return;
        }

        var (deleted, errorMessage) = await _store.DeleteItemAsync(item);
        if (!deleted)
        {
            await DisplayAlertAsync("Tasks", errorMessage, "OK");
        }
    }

    private async void OnCompleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || !int.TryParse(button.ClassId, out var id))
        {
            return;
        }

        var item = _store.ActiveItems.FirstOrDefault(x => x.ItemId == id);
        if (item == null)
        {
            return;
        }

        var (completed, errorMessage) = await _store.MoveToCompletedAsync(item);
        if (!completed)
        {
            await DisplayAlertAsync("Tasks", errorMessage, "OK");
        }
    }

    private async Task LoadActiveItemsAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        try
        {
            var (loaded, errorMessage) = await _store.RefreshActiveAsync();
            if (!loaded && !string.IsNullOrWhiteSpace(errorMessage))
            {
                await DisplayAlertAsync("Tasks", errorMessage, "OK");
            }
        }
        finally
        {
            _isLoading = false;
        }
    }
}
