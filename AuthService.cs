namespace ToDoMaui_Listview;

using System.Net.Mail;
using Microsoft.Maui.Storage;

public sealed class AuthService
{
    private const string SignedInKey = "auth_signed_in_v2";
    private const string UserIdKey = "auth_user_id_v2";
    private const string UserNameKey = "auth_user_name_v2";
    private const string UserEmailKey = "auth_user_email_v2";

    private static readonly Lazy<AuthService> LazyService = new(() => new AuthService());

    private readonly ToDoApiClient _apiClient = ToDoApiClient.Instance;

    public static AuthService Instance => LazyService.Value;

    public int CurrentUserId { get; private set; }

    public string CurrentUserName { get; private set; } = string.Empty;

    public string CurrentUserEmail { get; private set; } = string.Empty;

    public bool IsSignedIn => CurrentUserId > 0;

    private AuthService()
    {
        RestoreSession();
    }

    public async Task<(bool Success, string ErrorMessage)> SignInAsync(string email, string password)
    {
        var cleanEmail = (email ?? string.Empty).Trim();
        var cleanPassword = password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cleanEmail) || string.IsNullOrWhiteSpace(cleanPassword))
        {
            return (false, "Please enter your email and password.");
        }

        if (!IsValidEmail(cleanEmail))
        {
            return (false, "Please enter a valid email address.");
        }

        var response = await _apiClient.SignInAsync(cleanEmail, cleanPassword);
        if (!response.Success || response.Data == null)
        {
            return (false, string.IsNullOrWhiteSpace(response.Message)
                ? "Invalid email or password."
                : response.Message);
        }

        SetSignedInUser(response.Data);
        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> SignUpAsync(
        string userName,
        string email,
        string password,
        string confirmPassword)
    {
        var cleanName = (userName ?? string.Empty).Trim();
        var cleanEmail = (email ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(cleanName))
        {
            return (false, "Please enter your name.");
        }

        if (string.IsNullOrWhiteSpace(cleanEmail) || !IsValidEmail(cleanEmail))
        {
            return (false, "Please enter a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            return (false, "Password must be at least 6 characters.");
        }

        if (password != confirmPassword)
        {
            return (false, "Passwords do not match.");
        }

        var (firstName, lastName) = SplitName(cleanName);
        var signUpResponse = await _apiClient.SignUpAsync(
            firstName,
            lastName,
            cleanEmail,
            password,
            confirmPassword);

        if (!signUpResponse.Success)
        {
            return (false, string.IsNullOrWhiteSpace(signUpResponse.Message)
                ? "Unable to create account."
                : signUpResponse.Message);
        }

        return await SignInAsync(cleanEmail, password);
    }

    public void SignOut()
    {
        CurrentUserId = 0;
        CurrentUserName = string.Empty;
        CurrentUserEmail = string.Empty;

        Preferences.Default.Set(SignedInKey, false);
        Preferences.Default.Remove(UserIdKey);
        Preferences.Default.Remove(UserNameKey);
        Preferences.Default.Remove(UserEmailKey);

        ToDoStore.Instance.Clear();
    }

    private void RestoreSession()
    {
        var wasSignedIn = Preferences.Default.Get(SignedInKey, false);
        if (!wasSignedIn)
        {
            SignOut();
            return;
        }

        CurrentUserId = Preferences.Default.Get(UserIdKey, 0);
        CurrentUserName = Preferences.Default.Get(UserNameKey, string.Empty);
        CurrentUserEmail = Preferences.Default.Get(UserEmailKey, string.Empty);

        if (CurrentUserId <= 0 || string.IsNullOrWhiteSpace(CurrentUserEmail))
        {
            SignOut();
        }
    }

    private void SetSignedInUser(ApiUser user)
    {
        CurrentUserId = user.Id;
        CurrentUserName = BuildDisplayName(user.FirstName, user.LastName);
        CurrentUserEmail = user.Email;

        Preferences.Default.Set(SignedInKey, true);
        Preferences.Default.Set(UserIdKey, CurrentUserId);
        Preferences.Default.Set(UserNameKey, CurrentUserName);
        Preferences.Default.Set(UserEmailKey, CurrentUserEmail);
    }

    private static string BuildDisplayName(string firstName, string lastName)
    {
        var first = (firstName ?? string.Empty).Trim();
        var last = (lastName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(first) && string.IsNullOrWhiteSpace(last))
        {
            return "User";
        }

        if (string.IsNullOrWhiteSpace(last))
        {
            return first;
        }

        return $"{first} {last}".Trim();
    }

    private static (string FirstName, string LastName) SplitName(string fullName)
    {
        var trimmedName = (fullName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return ("User", string.Empty);
        }

        var parts = trimmedName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return (parts[0], string.Empty);
        }

        return (parts[0], parts[1]);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
