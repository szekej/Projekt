namespace WarehouseManagementSystem.Utilities;

public static class AppConfig
{
    public const string DataDirectory = "Data";

    public static string ProductsFilePath => Path.Combine(DataDirectory, "products.json");
    public static string CustomersFilePath => Path.Combine(DataDirectory, "customers.json");
    public static string OrdersFilePath => Path.Combine(DataDirectory, "orders.json");
    public static string InventoryFilePath => Path.Combine(DataDirectory, "inventory.json");
    public static string LogFilePath => Path.Combine(DataDirectory, "logs.txt");
}
