using WarehouseManagementSystem.Events;
using WarehouseManagementSystem.Interfaces;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Services;

public sealed class OrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<InventoryItem> _inventoryRepository;
    private readonly ILogger _logger;

    public OrderService(
        IRepository<Order> orderRepository,
        IRepository<Product> productRepository,
        IRepository<InventoryItem> inventoryRepository,
        ILogger logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(int customerId, Dictionary<int, int> request, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        var inventory = (await _inventoryRepository.GetAllAsync(cancellationToken)).ToDictionary(i => i.ProductId);

        var order = new Order(customerId);

        foreach (var pair in request)
        {
            var product = products.FirstOrDefault(p => p.Id == pair.Key)
                ?? throw new DomainException($"Product {pair.Key} not found.");

            if (!inventory.TryGetValue(pair.Key, out var item) || item.Quantity < pair.Value)
            {
                throw new DomainException($"Insufficient stock for {product.Name}.");
            }

            order.AddProduct(product, pair.Value);
            item -= pair.Value;

            if (item.Quantity <= item.LowStockThreshold)
            {
                EventHub.RaiseLowStock(product);
            }

            await _inventoryRepository.UpdateAsync(item, cancellationToken);
        }

        await _orderRepository.AddAsync(order, cancellationToken);
        await _logger.LogAsync($"Order {order.Id} created for customer {customerId}.", cancellationToken);
        EventHub.RaiseOrderCreated(order);
        return order;
    }
}
