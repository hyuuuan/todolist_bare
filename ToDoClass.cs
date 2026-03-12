namespace ToDoMaui_Listview;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ToDoClass : INotifyPropertyChanged
{
    private int _itemId;
    private string _itemName = string.Empty;
    private string _itemDescription = string.Empty;
    private string _status = "todo";
    private int _userId;

    // Compatibility aliases for the original single-page implementation.
    public int id
    {
        get => ItemId;
        set => ItemId = value;
    }

    public string title
    {
        get => ItemName;
        set => ItemName = value;
    }

    public string detail
    {
        get => ItemDescription;
        set => ItemDescription = value;
    }

    public int ItemId
    {
        get => _itemId;
        set => SetField(ref _itemId, value);
    }

    public string ItemName
    {
        get => _itemName;
        set => SetField(ref _itemName, value);
    }

    public string ItemDescription
    {
        get => _itemDescription;
        set => SetField(ref _itemDescription, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public int UserId
    {
        get => _userId;
        set => SetField(ref _userId, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
