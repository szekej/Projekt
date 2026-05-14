namespace WarehouseManagementSystem.Models;

public sealed class CardPayment : Payment
{
    public string MaskedCardNumber { get; set; } = "****-****-****-0000";

    public CardPayment() : this(0, "0000")
    {
    }

    public CardPayment(decimal amount, string last4Digits) : base(amount)
    {
        MaskedCardNumber = $"****-****-****-{last4Digits}";
    }

    public override string Method => "Card";

    public override async Task<bool> ProcessAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(250, cancellationToken);
        return Amount > 0;
    }
}
