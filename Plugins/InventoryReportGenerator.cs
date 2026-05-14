using WarehouseManagementSystem.Interfaces;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Utilities;

namespace WarehouseManagementSystem.Plugins;

public sealed class InventoryReportGenerator : IReportGenerator
{
    public string Name => "inventory";

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var inventory = await JsonStorage.LoadAsync<InventoryItem>(AppConfig.InventoryFilePath, cancellationToken);
        var lines = inventory.Select(i => $"ProductId: {i.ProductId}, Qty: {i.Quantity}");
        return "INVENTORY REPORT" + Environment.NewLine + string.Join(Environment.NewLine, lines);
    }
}
