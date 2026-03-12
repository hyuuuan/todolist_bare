namespace ToDoMaui_Listview;

public partial class CompletedPage : ContentPage
{
    private readonly ToDoStore _store = ToDoStore.Instance;

    public CompletedPage()
    {
        InitializeComponent();
        CompletedListView.ItemsSource = _store.CompletedItems;
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

    private void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || !int.TryParse(button.ClassId, out var id))
        {
            return;
        }

        var item = _store.CompletedItems.FirstOrDefault(x => x.ItemId == id);
        if (item != null)
        {
            _store.DeleteItem(item);
        }
    }
}
