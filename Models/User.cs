namespace WarehouseManagementSystem.Models;

public abstract class User : BaseEntity
{
    private static int _userCounter;
    private string _email = string.Empty;

    public static int UserCounter => _userCounter;

    public string FirstName { get; set; } = "Unknown";
    public string LastName { get; set; } = "User";

    public string Email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
            {
                throw new ArgumentException("Invalid email address.");
            }

            _email = value;
        }
    }

    protected User() : this("Unknown", "User", "unknown@system.local")
    {
    }

    protected User(string firstName, string lastName, string email)
    {
        Id = Interlocked.Increment(ref _userCounter);
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    public virtual string GetRole() => "User";
}
