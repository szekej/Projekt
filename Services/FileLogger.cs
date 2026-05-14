using WarehouseManagementSystem.Interfaces;
using WarehouseManagementSystem.Utilities;

namespace WarehouseManagementSystem.Services;

public sealed class FileLogger : ILogger
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public async Task LogAsync(string message, CancellationToken cancellationToken = default)
    {
        var line = $"[{DateTime.UtcNow:O}] {message}{Environment.NewLine}";
        await _gate.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(AppConfig.LogFilePath, line, cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }
}
