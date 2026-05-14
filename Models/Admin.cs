namespace WarehouseManagementSystem.Models;

public sealed class Admin : Employee
{
    public string PermissionLevel { get; set; } = "Full";

    public Admin() : this("System", "Admin", "admin@local", 0, "Full")
    {
    }

    public Admin(string firstName, string lastName, string email, decimal salary, string permissionLevel)
        : base(firstName, lastName, email, salary)
    {
        PermissionLevel = permissionLevel;
    }

    public override string GetRole() => "Admin";
}
