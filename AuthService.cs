namespace ToDoMaui_Listview;

public sealed class AuthService
{
    private static readonly Lazy<AuthService> LazyService = new(() => new AuthService());

    public static AuthService Instance => LazyService.Value;

    private string _registeredName = "Student";
    private string _registeredEmail = "student@todo.app";
    private string _registeredPassword = "123456";

    public int CurrentUserId { get; private set; }

    public string CurrentUserName { get; private set; } = string.Empty;

    public string CurrentUserEmail { get; private set; } = string.Empty;

    public bool IsSignedIn => CurrentUserId > 0;

    private AuthService()
    {
    }

    public bool SignIn(string email, string password)
    {
        var cleanEmail = (email ?? string.Empty).Trim();
        var cleanPassword = password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cleanEmail) || string.IsNullOrWhiteSpace(cleanPassword))
        {
            return false;
        }

        var credentialsMatch = string.Equals(cleanEmail, _registeredEmail, StringComparison.OrdinalIgnoreCase)
                               && cleanPassword == _registeredPassword;

        // Allow signing in with any non-empty credentials before registration
        // to keep this sample app frictionless.
        if (!credentialsMatch && _registeredEmail != "student@todo.app")
        {
            return false;
        }

        CurrentUserId = 1;
        CurrentUserEmail = cleanEmail;
        CurrentUserName = credentialsMatch
            ? _registeredName
            : cleanEmail.Split('@').FirstOrDefault() ?? "Student";

        return true;
    }

    public bool SignUp(string userName, string email, string password, string confirmPassword, out string errorMessage)
    {
        var cleanName = (userName ?? string.Empty).Trim();
        var cleanEmail = (email ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(cleanName) || string.IsNullOrWhiteSpace(cleanEmail))
        {
            errorMessage = "Please enter your name and email.";
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
        _registeredPassword = password;

        CurrentUserId = 1;
        CurrentUserName = cleanName;
        CurrentUserEmail = cleanEmail;
        errorMessage = string.Empty;
        return true;
    }

    public void SignOut()
    {
        CurrentUserId = 0;
        CurrentUserName = string.Empty;
        CurrentUserEmail = string.Empty;
    }
}
