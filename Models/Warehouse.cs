namespace WarehouseManagementSystem.Models;

public sealed class Warehouse
{
    private readonly Dictionary<int, InventoryItem> _stock = new();
    private readonly Queue<Order> _orderQueue = new();

    public string Name { get; set; } = "Main Warehouse";

    // Indexer gives fast access to stock item by ProductId.
    public InventoryItem? this[int productId]
    {
        get => _stock.GetValueOrDefault(productId);
        set
        {
            if (value is null) return;
            _stock[productId] = value;
        }
    }

    public IReadOnlyCollection<InventoryItem> Inventory => _stock.Values;

    public void SyncInventory(IEnumerable<InventoryItem> items)
    {
        _stock.Clear();
        foreach (var item in items)
        {
            _stock[item.ProductId] = item;
        }
    }

    public void EnqueueOrder(Order order) => _orderQueue.Enqueue(order);

    public Order? DequeueOrder() => _orderQueue.TryDequeue(out var order) ? order : null;
}
