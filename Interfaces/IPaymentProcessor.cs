using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Interfaces;

public interface IPaymentProcessor
{
    Task ProcessAsync(Payment payment, CancellationToken cancellationToken = default);
}
