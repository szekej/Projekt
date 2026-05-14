namespace WarehouseManagementSystem.Interfaces;

public interface IReportGenerator
{
    string Name { get; }
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
