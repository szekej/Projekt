using System.Text.Json;

namespace WarehouseManagementSystem.Utilities;

public static class JsonStorage
{
    public static async Task<IReadOnlyList<T>> LoadAsync<T>(string filePath, CancellationToken cancellationToken = default)
    {
        EnsureEnvironment();

        if (!File.Exists(filePath))
        {
            await File.WriteAllTextAsync(filePath, "[]", cancellationToken);
        }

        await using var stream = File.OpenRead(filePath);
        var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, cancellationToken: cancellationToken);
        return items ?? new List<T>();
    }

    public static void EnsureEnvironment()
    {
        if (!Directory.Exists(AppConfig.DataDirectory))
        {
            Directory.CreateDirectory(AppConfig.DataDirectory);
        }

        if (!File.Exists(AppConfig.LogFilePath))
        {
            File.WriteAllText(AppConfig.LogFilePath, string.Empty);
        }
    }
}
