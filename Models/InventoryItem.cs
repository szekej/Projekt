namespace WarehouseManagementSystem.Models;

public sealed class InventoryItem : BaseEntity
{
    private int _quantity;

    public int ProductId { get; set; }

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Quantity cannot be negative.");
            }

            _quantity = value;
        }
    }

    public int LowStockThreshold { get; set; } = 5;

    public InventoryItem() : this(0, 0)
    {
    }

    public InventoryItem(int productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
        Id = productId;
    }

    public static InventoryItem operator +(InventoryItem item, int amount)
    {
        item.Quantity += amount;
        return item;
    }

    public static InventoryItem operator -(InventoryItem item, int amount)
    {
        item.Quantity = Math.Max(0, item.Quantity - amount);
        return item;
    }
}
