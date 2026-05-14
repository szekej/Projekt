using System.Text.Json;
using WarehouseManagementSystem.Interfaces;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Utilities;

namespace WarehouseManagementSystem.Repositories;

public class JsonRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    public JsonRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await JsonStorage.LoadAsync<T>(_filePath, cancellationToken);
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var list = await JsonStorage.LoadAsync<T>(_filePath, cancellationToken);
        return list.FirstOrDefault(x => x.Id == id);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var list = (await JsonStorage.LoadAsync<T>(_filePath, cancellationToken)).ToList();
        list.Add(entity);
        await SaveAsync(list, cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        var list = (await JsonStorage.LoadAsync<T>(_filePath, cancellationToken)).ToList();
        var index = list.FindIndex(x => x.Id == entity.Id);
        if (index < 0)
        {
            throw new DomainException($"Entity with id {entity.Id} not found.");
        }

        list[index] = entity;
        await SaveAsync(list, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var list = (await JsonStorage.LoadAsync<T>(_filePath, cancellationToken)).ToList();
        list.RemoveAll(x => x.Id == id);
        await SaveAsync(list, cancellationToken);
    }

    private async Task SaveAsync(List<T> entities, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(entities, _serializerOptions);
        await File.WriteAllTextAsync(_filePath, json, cancellationToken);
    }
}
