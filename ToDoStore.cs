using System.Collections.ObjectModel;

namespace ToDoMaui_Listview;

public sealed class ToDoStore
{
    private static readonly Lazy<ToDoStore> LazyStore = new(() => new ToDoStore());

    public static ToDoStore Instance => LazyStore.Value;

    private int _nextItemId = 1;

    public ObservableCollection<ToDoClass> ActiveItems { get; } = new();

    public ObservableCollection<ToDoClass> CompletedItems { get; } = new();

    private ToDoStore()
    {
        SeedData();
    }

    public ToDoClass AddItem(string name, string description, int userId)
    {
        var item = new ToDoClass
        {
            ItemId = _nextItemId++,
            ItemName = name,
            ItemDescription = description,
            Status = "todo",
            UserId = userId
        };

        ActiveItems.Insert(0, item);
        return item;
    }

    public bool UpdateItem(ToDoClass item, string name, string description)
    {
        if (item == null)
        {
            return false;
        }

        item.ItemName = name;
        item.ItemDescription = description;
        return true;
    }

    public bool DeleteItem(ToDoClass item)
    {
        if (item == null)
        {
            return false;
        }

        if (ActiveItems.Remove(item))
        {
            return true;
        }

        return CompletedItems.Remove(item);
    }

    public bool MoveToCompleted(ToDoClass item)
    {
        if (item == null || !ActiveItems.Remove(item))
        {
            return false;
        }

        item.Status = "completed";
        CompletedItems.Insert(0, item);
        return true;
    }

    public bool MoveToActive(ToDoClass item)
    {
        if (item == null || !CompletedItems.Remove(item))
        {
            return false;
        }

        item.Status = "todo";
        ActiveItems.Insert(0, item);
        return true;
    }

    public ToDoClass? FindById(int id)
    {
        return ActiveItems.FirstOrDefault(x => x.ItemId == id)
               ?? CompletedItems.FirstOrDefault(x => x.ItemId == id);
    }

    private void SeedData()
    {
        for (var i = 1; i <= 5; i++)
        {
            AddItem($"title {i}", $"detail {i}", 1);
        }

        for (var i = 1; i <= 5; i++)
        {
            var completed = AddItem($"completed {i}", $"completed detail {i}", 1);
            MoveToCompleted(completed);
        }
    }
}
