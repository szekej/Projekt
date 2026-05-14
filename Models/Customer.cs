namespace WarehouseManagementSystem.Models;

public sealed class Customer : User
{
    public int LoyaltyPoints { get; private set; }

    public Customer() : this("New", "Customer", "customer@local")
    {
    }

    public Customer(string firstName, string lastName, string email) : base(firstName, lastName, email)
    {
    }

    public void AddLoyaltyPoints(int points)
    {
        if (points > 0)
        {
            LoyaltyPoints += points;
        }
    }

    public override string GetRole() => "Customer";
}
