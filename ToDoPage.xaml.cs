namespace ToDoMaui_Listview;

public partial class ToDoPage : ContentPage
{
    private readonly ToDoStore _store = ToDoStore.Instance;

    public ToDoPage()
    {
        InitializeComponent();
        ToDoListView.ItemsSource = _store.ActiveItems;
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddToDoPage));
    }

    private async void OnItemTapped(object? sender, ItemTappedEventArgs e)
    {
        if (e.Item is ToDoClass item)
        {
            await Shell.Current.GoToAsync($"{nameof(EditToDoPage)}?itemId={item.ItemId}");
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

        var item = _store.ActiveItems.FirstOrDefault(x => x.ItemId == id);
        if (item != null)
        {
            _store.DeleteItem(item);
        }
    }

    private void OnCompleteClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || !int.TryParse(button.ClassId, out var id))
        {
            return;
        }

        var item = _store.ActiveItems.FirstOrDefault(x => x.ItemId == id);
        if (item != null)
        {
            _store.MoveToCompleted(item);
        }
    }
}
