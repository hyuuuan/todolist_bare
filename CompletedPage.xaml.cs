namespace ToDoMaui_Listview;

public partial class CompletedPage : ContentPage
{
    private readonly ToDoStore _store = ToDoStore.Instance;
    private bool _isLoading;

    public CompletedPage()
    {
        InitializeComponent();
        CompletedListView.ItemsSource = _store.CompletedItems;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!AppNavigator.EnsureSignedIn())
        {
            return;
        }

        await LoadCompletedItemsAsync();
    }

    private async void OnItemTapped(object? sender, ItemTappedEventArgs e)
    {
        if (e.Item is ToDoClass item)
        {
            await Shell.Current.GoToAsync($"{nameof(EditCompletedPage)}?itemId={item.ItemId}");
        }

        if (sender is ListView listView)
        {
            listView.SelectedItem = null;
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || !int.TryParse(button.ClassId, out var id))
        {
            return;
        }

        var item = _store.CompletedItems.FirstOrDefault(x => x.ItemId == id);
        if (item == null)
        {
            return;
        }

        var (deleted, errorMessage) = await _store.DeleteItemAsync(item);
        if (!deleted)
        {
            await DisplayAlertAsync("Completed", errorMessage, "OK");
        }
    }

    private async Task LoadCompletedItemsAsync()
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        try
        {
            var (loaded, errorMessage) = await _store.RefreshCompletedAsync();
            if (!loaded && !string.IsNullOrWhiteSpace(errorMessage))
            {
                await DisplayAlertAsync("Completed", errorMessage, "OK");
            }
        }
        finally
        {
            _isLoading = false;
        }
    }
}
