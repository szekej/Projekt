using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Events;

public delegate void OrderCreatedHandler(object? sender, Order order);
public delegate void LowStockHandler(object? sender, Product product);
public delegate void PaymentCompletedHandler(object? sender, Payment payment);
