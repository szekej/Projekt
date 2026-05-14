namespace WarehouseManagementSystem.Models;

public sealed class Product : BaseEntity
{
    private static int _productCounter;
    private decimal _price;

    public static int ProductCounter => _productCounter;

    public string Name { get; set; } = "Unnamed";

    public decimal Price
    {
        get => _price;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Price cannot be negative.");
            }

            _price = value;
        }
    }

    public Product() : this("Unnamed", 0)
    {
    }

    public Product(string name, decimal price)
    {
        Id = Interlocked.Increment(ref _productCounter);
        Name = name;
        Price = price;
    }

    public static bool operator >(Product left, Product right) => left.Price > right.Price;

    public static bool operator <(Product left, Product right) => left.Price < right.Price;

    public static bool operator ==(Product? left, Product? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Name == right.Name && left.Price == right.Price;
    }

    public static bool operator !=(Product? left, Product? right) => !(left == right);

    public override bool Equals(object? obj) => obj is Product other && this == other;

    public override int GetHashCode() => HashCode.Combine(Name, Price);
}
