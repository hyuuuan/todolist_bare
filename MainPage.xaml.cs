using System.Collections.ObjectModel;

namespace ToDoMaui_Listview;

public partial class MainPage : ContentPage
{
    private ObservableCollection<ToDoClass> ToDoList = new ObservableCollection<ToDoClass>();
    private ToDoClass? _selectedItem = null;
    private int _nextId = 1;

    public MainPage()
    {
        InitializeComponent();
        todoLV.ItemsSource = ToDoList;
    }

    private void AddToDoItem(object? sender, EventArgs e)
    {
        string title  = titleEntry.Text?.Trim() ?? "";
        string detail = detailsEditor.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(title)) return;

        ToDoList.Add(new ToDoClass
        {
            id     = _nextId++,
            title  = title,
            detail = detail
        });

        ClearInputs();
    }

    private void EditToDoItem(object? sender, EventArgs e)
    {
        if (_selectedItem == null) return;

        string title = titleEntry.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(title)) return;

        _selectedItem.title  = title;
        _selectedItem.detail = detailsEditor.Text?.Trim() ?? "";

        todoLV.SelectedItem = null;
        CancelEditMode();
    }

    private void CancelEdit(object? sender, EventArgs e)
    {
        todoLV.SelectedItem = null;
        CancelEditMode();
    }

    private void DeleteToDoItem(object? sender, EventArgs e)
    {
        if (sender is Button btn && int.TryParse(btn.ClassId, out int id))
        {
            var item = ToDoList.FirstOrDefault(t => t.id == id);
            if (item != null)
            {
                ToDoList.Remove(item);
                if (_selectedItem?.id == id)
                {
                    todoLV.SelectedItem = null;
                    CancelEditMode();
                }
            }
        }
    }

    private void TodoLV_OnItemSelected(object? sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is ToDoClass item)
        {
            _selectedItem      = item;
            titleEntry.Text    = item.title;
            detailsEditor.Text = item.detail;
            SetEditMode(true);
        }
        // Clear the selection immediately so iOS doesn't show the checkmark
        todoLV.SelectedItem = null;
    }

    private void todoLV_ItemTapped(object? sender, ItemTappedEventArgs e)
    {
        // Selection is handled by ItemSelected
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
