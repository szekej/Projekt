namespace WarehouseManagementSystem.Models;

public sealed class CashPayment : Payment
{
    public decimal ReceivedAmount { get; set; }

    public CashPayment() : this(0, 0)
    {
    }

    public CashPayment(decimal amount, decimal receivedAmount) : base(amount)
    {
        ReceivedAmount = receivedAmount;
    }

    public override string Method => "Cash";

    public override async Task<bool> ProcessAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        return ReceivedAmount >= Amount;
    }
}
