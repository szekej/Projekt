using WarehouseManagementSystem.Interfaces;
using WarehouseManagementSystem.Utilities;

namespace WarehouseManagementSystem.Services;

public sealed class ReportService
{
    private readonly ILogger _logger;
    private readonly IReadOnlyList<IReportGenerator> _generators;

    public ReportService(ILogger logger)
    {
        _logger = logger;
        _generators = ReflectionHelper.DiscoverReportGenerators();
    }

    public async Task GenerateAllAsync(CancellationToken cancellationToken = default)
    {
        foreach (var generator in _generators)
        {
            var report = await generator.GenerateAsync(cancellationToken);
            var reportPath = Path.Combine(AppConfig.DataDirectory, $"report_{generator.Name}.txt");
            await File.WriteAllTextAsync(reportPath, report, cancellationToken);
            await _logger.LogAsync($"Report generated: {generator.Name}", cancellationToken);
        }
    }
}
