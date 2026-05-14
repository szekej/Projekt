using WarehouseManagementSystem.Interfaces;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Utilities;

namespace WarehouseManagementSystem.Plugins;

public sealed class SalesReportGenerator : IReportGenerator
{
    public string Name => "sales";

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var orders = await JsonStorage.LoadAsync<Order>(AppConfig.OrdersFilePath, cancellationToken);
        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var totalOrders = orders.Count;
        return $"SALES REPORT{Environment.NewLine}Orders: {totalOrders}{Environment.NewLine}Revenue: {totalRevenue:C}";
    }
}
