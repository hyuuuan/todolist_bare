namespace ToDoMaui_Listview;

using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;

public sealed class AuthService
{
    private const string RegisteredNameKey = "auth_registered_name_v1";
    private const string RegisteredEmailKey = "auth_registered_email_v1";
    private const string RegisteredPasswordHashKey = "auth_registered_password_hash_v1";
    private const string RegisteredPasswordSaltKey = "auth_registered_password_salt_v1";
    private const string SignedInKey = "auth_signed_in_v1";

    private static readonly Lazy<AuthService> LazyService = new(() => new AuthService());

    public static AuthService Instance => LazyService.Value;

    private string _registeredName = string.Empty;
    private string _registeredEmail = string.Empty;
    private string _registeredPasswordHash = string.Empty;
    private string _registeredPasswordSalt = string.Empty;

    public int CurrentUserId { get; private set; }

    public string CurrentUserName { get; private set; } = string.Empty;

    public string CurrentUserEmail { get; private set; } = string.Empty;

    public bool IsSignedIn => CurrentUserId > 0;

    public bool HasRegisteredUser => !string.IsNullOrWhiteSpace(_registeredEmail)
                                     && !string.IsNullOrWhiteSpace(_registeredPasswordHash)
                                     && !string.IsNullOrWhiteSpace(_registeredPasswordSalt);

    private AuthService()
    {
        LoadRegisteredUser();
        RestoreSession();
    }

    public bool SignIn(string email, string password)
    {
        return SignIn(email, password, out _);
    }

    public bool SignIn(string email, string password, out string errorMessage)
    {
        var cleanEmail = (email ?? string.Empty).Trim();
        var cleanPassword = password ?? string.Empty;

        if (!HasRegisteredUser)
        {
            errorMessage = "No account found. Please sign up first.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(cleanEmail) || string.IsNullOrWhiteSpace(cleanPassword))
        {
            errorMessage = "Please enter your email and password.";
            return false;
        }

        var emailMatches = string.Equals(cleanEmail, _registeredEmail, StringComparison.OrdinalIgnoreCase);
        var passwordMatches = string.Equals(
            HashPassword(cleanPassword, _registeredPasswordSalt),
            _registeredPasswordHash,
            StringComparison.Ordinal);

        if (!emailMatches || !passwordMatches)
        {
            errorMessage = "Invalid email or password.";
            return false;
        }

        SetSignedInUser();
        errorMessage = string.Empty;
        return true;
    }

    public bool SignUp(string userName, string email, string password, string confirmPassword, out string errorMessage)
    {
        var cleanName = (userName ?? string.Empty).Trim();
        var cleanEmail = (email ?? string.Empty).Trim();

        if (HasRegisteredUser)
        {
            errorMessage = "An account already exists. Please sign in.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(cleanName))
        {
            errorMessage = "Please enter your name.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(cleanEmail) || !IsValidEmail(cleanEmail))
        {
            errorMessage = "Please enter a valid email address.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            errorMessage = "Password must be at least 6 characters.";
            return false;
        }

        if (password != confirmPassword)
        {
            errorMessage = "Passwords do not match.";
            return false;
        }

        _registeredName = cleanName;
        _registeredEmail = cleanEmail;
        _registeredPasswordSalt = CreateSalt();
        _registeredPasswordHash = HashPassword(password, _registeredPasswordSalt);
        SaveRegisteredUser();

        SetSignedInUser();
        errorMessage = string.Empty;
        return true;
    }

    public void SignOut()
    {
        CurrentUserId = 0;
        CurrentUserName = string.Empty;
        CurrentUserEmail = string.Empty;
        Preferences.Default.Set(SignedInKey, false);
    }

    private void LoadRegisteredUser()
    {
        _registeredName = Preferences.Default.Get(RegisteredNameKey, string.Empty);
        _registeredEmail = Preferences.Default.Get(RegisteredEmailKey, string.Empty);
        _registeredPasswordHash = Preferences.Default.Get(RegisteredPasswordHashKey, string.Empty);
        _registeredPasswordSalt = Preferences.Default.Get(RegisteredPasswordSaltKey, string.Empty);
    }

    private void SaveRegisteredUser()
    {
        Preferences.Default.Set(RegisteredNameKey, _registeredName);
        Preferences.Default.Set(RegisteredEmailKey, _registeredEmail);
        Preferences.Default.Set(RegisteredPasswordHashKey, _registeredPasswordHash);
        Preferences.Default.Set(RegisteredPasswordSaltKey, _registeredPasswordSalt);
    }

    private void RestoreSession()
    {
        if (!HasRegisteredUser)
        {
            SignOut();
            return;
        }

        var wasSignedIn = Preferences.Default.Get(SignedInKey, false);
        if (wasSignedIn)
        {
            SetSignedInUser();
        }
    }

    private void SetSignedInUser()
    {
        CurrentUserId = 1;
        CurrentUserName = _registeredName;
        CurrentUserEmail = _registeredEmail;
        Preferences.Default.Set(SignedInKey, true);
    }

    private static string CreateSalt()
    {
        Span<byte> saltBytes = stackalloc byte[16];
        RandomNumberGenerator.Fill(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
    {
        using var sha = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes($"{salt}:{password}");
        var hashBytes = sha.ComputeHash(inputBytes);
        return Convert.ToBase64String(hashBytes);
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
