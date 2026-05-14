namespace WarehouseManagementSystem.Models;

public class Order : BaseEntity
{
    private static int _orderCounter;

    public static void InitializeCounter(int maxExistingId)
    {
        if (maxExistingId > _orderCounter)
            Volatile.Write(ref _orderCounter, maxExistingId);
    }

    public int CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public bool IsPaid { get; set; }

    public decimal TotalAmount => Items.Sum(i => i.LineTotal);

    public Order() : this(0)
    {
    }

    public Order(int customerId)
    {
        Id = Interlocked.Increment(ref _orderCounter);
        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;
    }

    public virtual void AddProduct(Product product, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

        var existing = Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing is null)
        {
            Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = product.Price
            });
            return;
        }

        existing.Quantity += quantity;
    }

    public static Order operator +(Order left, Order right)
    {
        var merged = new Order(left.CustomerId);
        foreach (var item in left.Items.Concat(right.Items))
        {
            var existing = merged.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existing is null)
            {
                merged.Items.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }
            else
            {
                existing.Quantity += item.Quantity;
            }
        }

        return merged;
    }

    public override string ToString() => $"Order #{Id}, Customer: {CustomerId}, Total: {TotalAmount:C}";
}
