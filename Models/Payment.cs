namespace WarehouseManagementSystem.Models;

public abstract class Payment : BaseEntity
{
    private static int _paymentCounter;

    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }

    protected Payment() : this(0)
    {
    }

    protected Payment(decimal amount)
    {
        Id = Interlocked.Increment(ref _paymentCounter);
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
    }

    public abstract string Method { get; }

    public virtual async Task<bool> ProcessAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(150, cancellationToken);
        return Amount >= 0;
    }
}
