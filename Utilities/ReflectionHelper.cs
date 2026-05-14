using System.Reflection;
using WarehouseManagementSystem.Interfaces;

namespace WarehouseManagementSystem.Utilities;

public static class ReflectionHelper
{
    public static IReadOnlyList<IReportGenerator> DiscoverReportGenerators()
    {
        var generatorTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IReportGenerator).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        var generators = new List<IReportGenerator>();
        foreach (var type in generatorTypes)
        {
            if (Activator.CreateInstance(type) is IReportGenerator generator)
            {
                generators.Add(generator);
            }
        }

        return generators;
    }

    public static IReadOnlyList<string> InspectModelMetadata<T>()
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => $"{p.Name}: {p.PropertyType.Name}")
            .ToList();
    }

    public static Dictionary<Type, Type> AutoRegisterServices()
    {
        var map = new Dictionary<Type, Type>();
        var services = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace?.Contains("Services") == true);

        foreach (var implementation in services)
        {
            foreach (var contract in implementation.GetInterfaces())
            {
                map[contract] = implementation;
            }
        }

        return map;
    }
}
