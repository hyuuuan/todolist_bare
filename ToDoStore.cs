using System.Collections.ObjectModel;

namespace ToDoMaui_Listview;

public sealed class ToDoStore
{
    private static readonly Lazy<ToDoStore> LazyStore = new(() => new ToDoStore());

    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private readonly ToDoApiClient _apiClient = ToDoApiClient.Instance;

    public static ToDoStore Instance => LazyStore.Value;

    public ObservableCollection<ToDoClass> ActiveItems { get; } = new();

    public ObservableCollection<ToDoClass> CompletedItems { get; } = new();

    private ToDoStore()
    {
    }

    public async Task<(bool Success, string ErrorMessage)> RefreshActiveAsync()
    {
        return await RefreshByStatusAsync("active", ActiveItems);
    }

    public async Task<(bool Success, string ErrorMessage)> RefreshCompletedAsync()
    {
        return await RefreshByStatusAsync("inactive", CompletedItems);
    }

    public async Task<(bool Success, string ErrorMessage)> RefreshAllAsync()
    {
        if (!AuthService.Instance.IsSignedIn)
        {
            Clear();
            return (false, "Please sign in first.");
        }

        var userId = AuthService.Instance.CurrentUserId;

        var activeResult = await _apiClient.GetItemsAsync("active", userId);
        if (!activeResult.Success)
        {
            return (false, activeResult.Message);
        }

        var completedResult = await _apiClient.GetItemsAsync("inactive", userId);
        if (!completedResult.Success)
        {
            return (false, completedResult.Message);
        }

        await _syncLock.WaitAsync();
        try
        {
            ReplaceItems(ActiveItems, activeResult.Data ?? []);
            ReplaceItems(CompletedItems, completedResult.Data ?? []);
        }
        finally
        {
            _syncLock.Release();
        }

        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> AddItemAsync(string name, string description, int userId)
    {
        if (userId <= 0)
        {
            return (false, "Please sign in first.");
        }

        var response = await _apiClient.AddItemAsync(name, description, userId);
        if (!response.Success || response.Data == null)
        {
            return (false, response.Message);
        }

        await _syncLock.WaitAsync();
        try
        {
            UpsertItem(response.Data);
        }
        finally
        {
            _syncLock.Release();
        }

        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> UpdateItemAsync(ToDoClass? item, string name, string description)
    {
        if (item == null)
        {
            return (false, "Task not found.");
        }

        var response = await _apiClient.EditItemAsync(item.ItemId, name, description);
        if (!response.Success)
        {
            return (false, response.Message);
        }

        item.ItemName = name;
        item.ItemDescription = description;
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> DeleteItemAsync(ToDoClass? item)
    {
        if (item == null)
        {
            return (false, "Task not found.");
        }

        var response = await _apiClient.DeleteItemAsync(item.ItemId);
        if (!response.Success)
        {
            return (false, response.Message);
        }

        await _syncLock.WaitAsync();
        try
        {
            RemoveItemById(item.ItemId);
        }
        finally
        {
            _syncLock.Release();
        }

        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> MoveToCompletedAsync(ToDoClass? item)
    {
        if (item == null)
        {
            return (false, "Task not found.");
        }

        var response = await _apiClient.ChangeItemStatusAsync(item.ItemId, "inactive");
        if (!response.Success)
        {
            return (false, response.Message);
        }

        await _syncLock.WaitAsync();
        try
        {
            item.Status = "inactive";
            UpsertItem(item);
        }
        finally
        {
            _syncLock.Release();
        }

        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> MoveToActiveAsync(ToDoClass? item)
    {
        if (item == null)
        {
            return (false, "Task not found.");
        }

        var response = await _apiClient.ChangeItemStatusAsync(item.ItemId, "active");
        if (!response.Success)
        {
            return (false, response.Message);
        }

        await _syncLock.WaitAsync();
        try
        {
            item.Status = "active";
            UpsertItem(item);
        }
        finally
        {
            _syncLock.Release();
        }

        return (true, string.Empty);
    }

    public ToDoClass? FindById(int id)
    {
        return ActiveItems.FirstOrDefault(x => x.ItemId == id)
               ?? CompletedItems.FirstOrDefault(x => x.ItemId == id);
    }

    public void Clear()
    {
        ActiveItems.Clear();
        CompletedItems.Clear();
    }

    private async Task<(bool Success, string ErrorMessage)> RefreshByStatusAsync(
        string status,
        ObservableCollection<ToDoClass> targetCollection)
    {
        if (!AuthService.Instance.IsSignedIn)
        {
            targetCollection.Clear();
            return (false, "Please sign in first.");
        }

        var userId = AuthService.Instance.CurrentUserId;
        var response = await _apiClient.GetItemsAsync(status, userId);
        if (!response.Success)
        {
            return (false, response.Message);
        }

        await _syncLock.WaitAsync();
        try
        {
            ReplaceItems(targetCollection, response.Data ?? []);
        }
        finally
        {
            _syncLock.Release();
        }

        return (true, string.Empty);
    }

    private void ReplaceItems(ObservableCollection<ToDoClass> target, IReadOnlyCollection<ToDoClass> source)
    {
        target.Clear();
        foreach (var item in source.OrderByDescending(x => x.ItemId))
        {
            target.Add(item);
        }
    }

    private void UpsertItem(ToDoClass item)
    {
        RemoveItemById(item.ItemId);

        if (string.Equals(item.Status, "inactive", StringComparison.OrdinalIgnoreCase))
        {
            CompletedItems.Insert(0, item);
            return;
        }

        item.Status = "active";
        ActiveItems.Insert(0, item);
    }

    private void RemoveItemById(int itemId)
    {
        var activeItem = ActiveItems.FirstOrDefault(x => x.ItemId == itemId);
        if (activeItem != null)
        {
            ActiveItems.Remove(activeItem);
        }

        var completedItem = CompletedItems.FirstOrDefault(x => x.ItemId == itemId);
        if (completedItem != null)
        {
            CompletedItems.Remove(completedItem);
        }
    }
}
