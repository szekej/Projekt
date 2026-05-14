namespace WarehouseManagementSystem.Interfaces;

public interface ILogger
{
    Task LogAsync(string message, CancellationToken cancellationToken = default);
}
