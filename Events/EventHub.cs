using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Events;

public static class EventHub
{
    public static event OrderCreatedHandler? OnOrderCreated;
    public static event LowStockHandler? OnLowStock;
    public static event PaymentCompletedHandler? OnPaymentCompleted;

    public static void RaiseOrderCreated(Order order) => OnOrderCreated?.Invoke(null, order);
    public static void RaiseLowStock(Product product) => OnLowStock?.Invoke(null, product);
    public static void RaisePaymentCompleted(Payment payment) => OnPaymentCompleted?.Invoke(null, payment);
}
