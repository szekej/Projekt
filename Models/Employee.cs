namespace WarehouseManagementSystem.Models;

public class Employee : User
{
    private decimal _salary;

    public decimal Salary
    {
        get => _salary;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Salary cannot be negative.");
            }

            _salary = value;
        }
    }

    public Employee() : this("Warehouse", "Employee", "employee@local", 0)
    {
    }

    public Employee(string firstName, string lastName, string email, decimal salary) : base(firstName, lastName, email)
    {
        Salary = salary;
    }

    public override string GetRole() => "Employee";
}
