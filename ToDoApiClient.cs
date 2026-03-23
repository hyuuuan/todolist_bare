namespace ToDoMaui_Listview;

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

public sealed record ApiCallResult(bool Success, string Message);

public sealed record ApiCallResult<T>(bool Success, T? Data, string Message);

public sealed class ApiUser
{
    public int Id { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}

public sealed class ToDoApiClient
{
    private static readonly Lazy<ToDoApiClient> LazyClient = new(() => new ToDoApiClient());

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public static ToDoApiClient Instance => LazyClient.Value;

    private ToDoApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://todo-list.dcism.org/"),
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    public async Task<ApiCallResult> SignUpAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string confirmPassword)
    {
        var payload = new
        {
            first_name = firstName,
            last_name = lastName,
            email,
            password,
            confirm_password = confirmPassword
        };

        var requestResult = await SendJsonAsync(HttpMethod.Post, "signup_action.php", payload);
        if (!requestResult.Success)
        {
            return new ApiCallResult(false, requestResult.Message);
        }

        return ParseStatusResponse(requestResult.Root, "Sign up failed.");
    }

    public async Task<ApiCallResult<ApiUser>> SignInAsync(string email, string password)
    {
        var route = BuildRoute(
            "signin_action.php",
            new Dictionary<string, string>
            {
                ["email"] = email,
                ["password"] = password
            });

        var requestResult = await SendJsonAsync(HttpMethod.Get, route, payload: null);
        if (!requestResult.Success)
        {
            return new ApiCallResult<ApiUser>(false, null, requestResult.Message);
        }

        var status = GetStatusCode(requestResult.Root);
        if (status != 200)
        {
            return new ApiCallResult<ApiUser>(
                false,
                null,
                GetMessage(requestResult.Root, "Invalid email or password."));
        }

        if (!requestResult.Root.TryGetProperty("data", out var dataElement)
            || dataElement.ValueKind != JsonValueKind.Object)
        {
            return new ApiCallResult<ApiUser>(false, null, "Sign in response is missing user data.");
        }

        var user = ParseUser(dataElement);
        if (user == null)
        {
            return new ApiCallResult<ApiUser>(false, null, "Unable to parse the account response.");
        }

        return new ApiCallResult<ApiUser>(true, user, string.Empty);
    }

    public async Task<ApiCallResult<List<ToDoClass>>> GetItemsAsync(string status, int userId)
    {
        var route = BuildRoute(
            "getItems_action.php",
            new Dictionary<string, string>
            {
                ["status"] = status,
                ["user_id"] = userId.ToString()
            });

        var requestResult = await SendJsonAsync(HttpMethod.Get, route, payload: null);
        if (!requestResult.Success)
        {
            return new ApiCallResult<List<ToDoClass>>(false, null, requestResult.Message);
        }

        var statusCode = GetStatusCode(requestResult.Root);
        if (statusCode != 200)
        {
            return new ApiCallResult<List<ToDoClass>>(
                false,
                null,
                GetMessage(requestResult.Root, "Unable to load tasks."));
        }

        var items = new List<ToDoClass>();

        if (requestResult.Root.TryGetProperty("data", out var dataElement))
        {
            items.AddRange(ParseItems(dataElement));
        }

        items = items
            .OrderByDescending(x => x.ItemId)
            .ToList();

        return new ApiCallResult<List<ToDoClass>>(true, items, string.Empty);
    }

    public async Task<ApiCallResult<ToDoClass>> AddItemAsync(string itemName, string itemDescription, int userId)
    {
        var payload = new
        {
            item_name = itemName,
            item_description = itemDescription,
            user_id = userId
        };

        var requestResult = await SendJsonAsync(HttpMethod.Post, "addItem_action.php", payload);
        if (!requestResult.Success)
        {
            return new ApiCallResult<ToDoClass>(false, null, requestResult.Message);
        }

        var status = GetStatusCode(requestResult.Root);
        if (status != 200)
        {
            return new ApiCallResult<ToDoClass>(
                false,
                null,
                GetMessage(requestResult.Root, "Unable to add task."));
        }

        if (!requestResult.Root.TryGetProperty("data", out var dataElement)
            || dataElement.ValueKind != JsonValueKind.Object)
        {
            return new ApiCallResult<ToDoClass>(false, null, "Add item response is missing task data.");
        }

        var item = ParseItem(dataElement);
        if (item == null)
        {
            return new ApiCallResult<ToDoClass>(false, null, "Unable to parse the created task.");
        }

        return new ApiCallResult<ToDoClass>(true, item, string.Empty);
    }

    public async Task<ApiCallResult> EditItemAsync(int itemId, string itemName, string itemDescription)
    {
        var payload = new
        {
            item_name = itemName,
            item_description = itemDescription,
            item_id = itemId
        };

        var primaryResult = await SendJsonAsync(HttpMethod.Put, "editItem_action.php", payload);
        if (primaryResult.Success)
        {
            var parsedPrimary = ParseStatusResponse(primaryResult.Root, "Unable to update task.");
            if (parsedPrimary.Success)
            {
                return parsedPrimary;
            }
        }

        // Fallback: this endpoint can be inconsistent and sometimes only accepts form payloads.
        var fallbackResult = await SendFormAsync(
            HttpMethod.Put,
            "editItem_action.php",
            new Dictionary<string, string>
            {
                ["item_name"] = itemName,
                ["item_description"] = itemDescription,
                ["item_id"] = itemId.ToString()
            });

        if (fallbackResult.Success)
        {
            var parsedFallback = ParseStatusResponse(fallbackResult.Root, "Unable to update task.");
            if (parsedFallback.Success)
            {
                return parsedFallback;
            }

            return parsedFallback;
        }

        return new ApiCallResult(false, primaryResult.Message);
    }

    public async Task<ApiCallResult> ChangeItemStatusAsync(int itemId, string status)
    {
        var payload = new
        {
            status,
            item_id = itemId
        };

        var requestResult = await SendJsonAsync(HttpMethod.Put, "statusItem_action.php", payload);
        if (!requestResult.Success)
        {
            return new ApiCallResult(false, requestResult.Message);
        }

        return ParseStatusResponse(requestResult.Root, "Unable to change task status.");
    }

    public async Task<ApiCallResult> DeleteItemAsync(int itemId)
    {
        var route = BuildRoute(
            "deleteItem_action.php",
            new Dictionary<string, string>
            {
                ["item_id"] = itemId.ToString()
            });

        var requestResult = await SendJsonAsync(HttpMethod.Delete, route, payload: null);
        if (!requestResult.Success)
        {
            return new ApiCallResult(false, requestResult.Message);
        }

        return ParseStatusResponse(requestResult.Root, "Unable to delete task.");
    }

    private static IEnumerable<ToDoClass> ParseItems(JsonElement dataElement)
    {
        if (dataElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var itemElement in dataElement.EnumerateArray())
            {
                var item = ParseItem(itemElement);
                if (item != null)
                {
                    yield return item;
                }
            }

            yield break;
        }

        if (dataElement.ValueKind != JsonValueKind.Object)
        {
            yield break;
        }

        foreach (var property in dataElement.EnumerateObject())
        {
            if (property.Value.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var item = ParseItem(property.Value);
            if (item != null)
            {
                yield return item;
            }
        }
    }

    private static ToDoClass? ParseItem(JsonElement itemElement)
    {
        var itemId = ReadInt(itemElement, "item_id");
        if (itemId <= 0)
        {
            return null;
        }

        return new ToDoClass
        {
            ItemId = itemId,
            ItemName = ReadString(itemElement, "item_name"),
            ItemDescription = ReadString(itemElement, "item_description"),
            Status = NormalizeStatus(ReadString(itemElement, "status")),
            UserId = ReadInt(itemElement, "user_id")
        };
    }

    private static ApiUser? ParseUser(JsonElement dataElement)
    {
        var id = ReadInt(dataElement, "id");
        if (id <= 0)
        {
            return null;
        }

        return new ApiUser
        {
            Id = id,
            FirstName = ReadString(dataElement, "fname"),
            LastName = ReadString(dataElement, "lname"),
            Email = ReadString(dataElement, "email")
        };
    }

    private async Task<(bool Success, string Message, JsonElement Root)> SendJsonAsync(
        HttpMethod method,
        string route,
        object? payload)
    {
        try
        {
            using var request = new HttpRequestMessage(method, route);
            if (payload != null)
            {
                request.Content = JsonContent.Create(payload, options: JsonOptions);
            }

            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var root = ParseJson(responseBody);
            if (root == null)
            {
                return (false, "Server returned an invalid response.", default);
            }

            return (true, string.Empty, root.Value);
        }
        catch (TaskCanceledException)
        {
            return (false, "The request timed out. Please try again.", default);
        }
        catch (HttpRequestException)
        {
            return (false, "Unable to reach the server. Check your internet connection.", default);
        }
        catch (Exception)
        {
            return (false, "Unexpected error while calling the server.", default);
        }
    }

    private async Task<(bool Success, string Message, JsonElement Root)> SendFormAsync(
        HttpMethod method,
        string route,
        IReadOnlyDictionary<string, string> formValues)
    {
        try
        {
            using var request = new HttpRequestMessage(method, route)
            {
                Content = new FormUrlEncodedContent(formValues)
            };

            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            var root = ParseJson(responseBody);
            if (root == null)
            {
                return (false, "Server returned an invalid response.", default);
            }

            return (true, string.Empty, root.Value);
        }
        catch (TaskCanceledException)
        {
            return (false, "The request timed out. Please try again.", default);
        }
        catch (HttpRequestException)
        {
            return (false, "Unable to reach the server. Check your internet connection.", default);
        }
        catch (Exception)
        {
            return (false, "Unexpected error while calling the server.", default);
        }
    }

    private static JsonElement? ParseJson(string rawResponse)
    {
        var jsonPayload = ExtractJsonPayload(rawResponse);
        if (string.IsNullOrWhiteSpace(jsonPayload))
        {
            return null;
        }

        using var document = JsonDocument.Parse(jsonPayload);
        return document.RootElement.Clone();
    }

    private static string ExtractJsonPayload(string rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            return string.Empty;
        }

        var firstJsonBracket = rawResponse.IndexOf('{');
        var lastJsonBracket = rawResponse.LastIndexOf('}');
        if (firstJsonBracket < 0 || lastJsonBracket <= firstJsonBracket)
        {
            return string.Empty;
        }

        return rawResponse[firstJsonBracket..(lastJsonBracket + 1)];
    }

    private static ApiCallResult ParseStatusResponse(JsonElement root, string fallbackMessage)
    {
        var statusCode = GetStatusCode(root);
        if (statusCode == 200)
        {
            return new ApiCallResult(true, string.Empty);
        }

        return new ApiCallResult(false, GetMessage(root, fallbackMessage));
    }

    private static int GetStatusCode(JsonElement root)
    {
        if (!root.TryGetProperty("status", out var statusElement))
        {
            return 0;
        }

        if (statusElement.ValueKind == JsonValueKind.Number && statusElement.TryGetInt32(out var numericStatus))
        {
            return numericStatus;
        }

        if (statusElement.ValueKind == JsonValueKind.String
            && int.TryParse(statusElement.GetString(), out var stringStatus))
        {
            return stringStatus;
        }

        return 0;
    }

    private static string GetMessage(JsonElement root, string fallbackMessage)
    {
        if (root.TryGetProperty("message", out var messageElement)
            && messageElement.ValueKind == JsonValueKind.String)
        {
            var message = messageElement.GetString();
            if (!string.IsNullOrWhiteSpace(message))
            {
                return message.Trim();
            }
        }

        return fallbackMessage;
    }

    private static int ReadInt(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var valueElement))
        {
            return 0;
        }

        if (valueElement.ValueKind == JsonValueKind.Number && valueElement.TryGetInt32(out var numberValue))
        {
            return numberValue;
        }

        if (valueElement.ValueKind == JsonValueKind.String
            && int.TryParse(valueElement.GetString(), out var stringValue))
        {
            return stringValue;
        }

        return 0;
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var valueElement)
            || valueElement.ValueKind != JsonValueKind.String)
        {
            return string.Empty;
        }

        return valueElement.GetString()?.Trim() ?? string.Empty;
    }

    private static string NormalizeStatus(string status)
    {
        if (string.Equals(status, "inactive", StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, "completed", StringComparison.OrdinalIgnoreCase))
        {
            return "inactive";
        }

        return "active";
    }

    private static string BuildRoute(string route, IReadOnlyDictionary<string, string> queryParams)
    {
        if (queryParams.Count == 0)
        {
            return route;
        }

        var builder = new StringBuilder(route);
        builder.Append('?');

        var first = true;
        foreach (var (key, value) in queryParams)
        {
            if (!first)
            {
                builder.Append('&');
            }

            builder
                .Append(Uri.EscapeDataString(key))
                .Append('=')
                .Append(Uri.EscapeDataString(value));

            first = false;
        }

        return builder.ToString();
    }
}
