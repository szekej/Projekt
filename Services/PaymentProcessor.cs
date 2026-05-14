using WarehouseManagementSystem.Events;
using WarehouseManagementSystem.Interfaces;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem.Services;

public sealed class PaymentProcessor : IPaymentProcessor
{
    private readonly ILogger _logger;

    public PaymentProcessor(ILogger logger)
    {
        _logger = logger;
    }

    public async Task ProcessAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        var success = await payment.ProcessAsync(cancellationToken);
        if (!success)
        {
            throw new DomainException("Payment failed.");
        }

        await _logger.LogAsync($"Payment processed: {payment.Method} {payment.Amount:C}", cancellationToken);
        EventHub.RaisePaymentCompleted(payment);
    }
}
