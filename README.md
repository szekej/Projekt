# Warehouse Management System (.NET 8)

Edukacyjna aplikacja backendowa (konsola) pokazująca zaawansowane OOP w C#.

## Struktura projektu

- `Models` - obiekty domenowe, dziedziczenie, klasy abstrakcyjne
- `Interfaces` - kontrakty (`ILogger`, `IRepository<T>`, `IPaymentProcessor`, `IReportGenerator`)
- `Repositories` - generyczne repozytorium JSON
- `Services` - logika biznesowa i orkiestracja
- `Events` - delegaty i centrum zdarzeń
- `Utilities` - konfiguracja, obsługa JSON, refleksja
- `Plugins` - generatory raportów wykrywane przez refleksję
- `Data` - trwałe dane JSON i logi
## Jak uruchomić

1. W katalogu projektu uruchom: `dotnet run`
2. Aplikacja załaduje dane startowe i pokaże menu.
3. Wybierz opcję, wpisując numer i naciskając Enter.

## Instrukcja użycia menu (co robi każda funkcja)

- `1. Lista produktów`  
  Wyświetla wszystkie produkty zapisane w `Data/products.json`.

- `2. Utwórz zamówienie (ręcznie)`  
  Pozwala wpisać pozycje zamówienia z klawiatury w formacie `produktId ilość` (np. `2 3`).  
  Pusta linia kończy wprowadzanie i tworzy zamówienie, zapisując je oraz aktualizując stan magazynu.

- `3. Przetwórz płatność`  
  Pobiera ostatnie zamówienie i wykonuje płatność. Typ płatności jest wybierany polimorficznie (`CardPayment` lub `CashPayment`).

- `4. Generuj raporty`  
  Uruchamia wszystkie generatory raportów odkryte refleksją i zapisuje wyniki do plików w folderze `Data`.

- `5. Pokaż metadane produktu (refleksja)`  
  Pokazuje właściwości klasy `Product` odczytane dynamicznie przez refleksję.

- `0. Wyjście`  
  Kończy działanie aplikacji.

## Co powinno pojawić się w folderze `Data`

Tak - po dodawaniu zamówień, przetwarzaniu płatności i generowaniu raportów pliki w `Data` powinny się zmieniać.

- `Data/products.json`  
  Lista produktów (tworzona przy seed danych).

- `Data/customers.json`  
  Lista klientów (tworzona przy seed danych).

- `Data/inventory.json`  
  Stan magazynowy.  
  Po opcji `2` (tworzenie zamówienia) ilości produktów są zmniejszane.

- `Data/orders.json`  
  Zamówienia.  
  Po opcji `2` dodawane są nowe rekordy zamówień.  
  Po opcji `3` ostatnie zamówienie otrzymuje `IsPaid = true`.

- `Data/logs.txt`  
  Logi aplikacji, zdarzeń i płatności.

- `Data/report_inventory.txt` i `Data/report_sales.txt`  
  Raporty tworzone po opcji `4`.

## Dokładny format i cykl życia plików `.json`

> Ważne: ścieżki w `AppConfig` są względne (`Data/...`).  
> Przy uruchamianiu z Visual Studio dane zwykle trafiają do `bin/Debug/net8.0/Data`, a przy `dotnet run` do `./Data` w katalogu projektu.

### 1) `products.json`

**Co przechowuje:** listę `Product`.

Każdy rekord zawiera:
- `Id` (`int`) - identyfikator produktu
- `Name` (`string`) - nazwa
- `Price` (`decimal`) - cena

**Kiedy jest odczytywany:**
- startup (`SeedDataAsync`),
- opcja `1` (lista produktów),
- opcja `2` (ręczne tworzenie zamówienia),
- wewnętrznie także w `OrderService.CreateOrderAsync`.

**Kiedy jest zapisywany:**
- podczas seedowania, jeśli plik jest pusty (`[]`).

### 2) `customers.json`

**Co przechowuje:** listę `Customer` (dziedziczy po `User`).

Każdy rekord zawiera:
- `Id` (`int`)
- `FirstName` (`string`)
- `LastName` (`string`)
- `Email` (`string`)
- `LoyaltyPoints` (`int`)

**Kiedy jest odczytywany:**
- startup (`SeedDataAsync`),
- opcja `2` (wybór pierwszego klienta do zamówienia).

**Kiedy jest zapisywany:**
- podczas seedowania, jeśli plik jest pusty (`[]`) - dodawany jest klient startowy.

### 3) `inventory.json`

**Co przechowuje:** listę `InventoryItem` (stan magazynowy).

Każdy rekord zawiera:
- `Id` (`int`) - w tym projekcie ustawiany na `ProductId`
- `ProductId` (`int`) - powiązanie z produktem
- `Quantity` (`int`) - aktualny stan
- `LowStockThreshold` (`int`) - próg niskiego stanu

**Kiedy jest odczytywany:**
- startup (`SeedDataAsync`, `SyncWarehouseAsync`),
- opcja `2` (walidacja i realizacja zamówienia),
- opcja `4` (raport magazynowy).

**Kiedy jest zapisywany:**
- podczas seedowania (uzupełnianie brakujących wpisów dla produktów),
- przy tworzeniu zamówienia w `OrderService` (zmniejszenie `Quantity` i `UpdateAsync`).

### 4) `orders.json`

**Co przechowuje:** listę `Order`.

Każdy rekord zawiera:
- `Id` (`int`)
- `CustomerId` (`int`)
- `CreatedAt` (`DateTime`)
- `Items` (`List<OrderItem>`) z polami: `ProductId`, `ProductName`, `Quantity`, `UnitPrice` (+ wyliczane `LineTotal`)
- `IsPaid` (`bool`)
- `TotalAmount` jest wyliczane z `Items` (getter), nie jako trwałe pole wejściowe użytkownika.

**Kiedy jest odczytywany:**
- opcja `3` (pobranie ostatniego zamówienia do płatności),
- opcja `4` (raport sprzedaży),
- ewentualnie do podglądu historii.

**Kiedy jest zapisywany:**
- opcja `2`: `AddAsync` nowego zamówienia,
- opcja `3`: `UpdateAsync` ostatniego zamówienia (`IsPaid = true`).


## Implementacja wymaganych koncepcji OOP

### 1. **Klasy** 
Modele domenowe z różnym przeznaczeniem.

- **`Product`** (`Models/Product.cs`) - reprezentuje produkt magazynowy z ID, nazwą i ceną.
- **`Customer`** (`Models/Customer.cs`) - dziedziczy po `User`, dodaje `LoyaltyPoints`.
- **`Order`** (`Models/Order.cs`) - zamówienie z listą pozycji, datą i statusem płatności.
- **`Warehouse`** (`Models/Warehouse.cs`) - zarządzanie stanem magazynu i kolejką zamówień.
- **`Employee`** (`Models/Employee.cs`) - dziedziczy po `User`, dodaje `Salary`.
- **`Admin`** (`Models/Admin.cs`) - dziedziczy po `Employee`, dodaje `PermissionLevel`.
- **`Payment`** (`Models/Payment.cs`) - klasa abstrakcyjna dla płatności.
- **`InventoryItem`** (`Models/InventoryItem.cs`) - stan magazynowy per produkt z progiem niskiego stanu.

### 2. **Konstruktory**
Domyślne, parametryzowane i łańcuchowanie.

```csharp
// Models/User.cs - domyślny i parametryzowany
protected User() : this("Unknown", "User", "unknown@system.local") { }
protected User(string firstName, string lastName, string email)
{
    Id = Interlocked.Increment(ref _userCounter);
    FirstName = firstName;
    LastName = lastName;
    Email = email;
}

// Models/Customer.cs - łańcuchowanie do klasy bazowej
public Customer() : this("New", "Customer", "customer@local") { }
public Customer(string firstName, string lastName, string email) 
    : base(firstName, lastName, email) { }

// Models/Admin.cs - łańcuchowanie wielopoziomowe
public Admin() : this("System", "Admin", "admin@local", 0, "Full") { }
public Admin(string firstName, string lastName, string email, decimal salary, string permissionLevel)
    : base(firstName, lastName, email, salary) { }
```

### 3. **Właściwości i Indeksery**

**Właściwości z walidacją:**
```csharp
// Models/Product.cs - walidacja ceny
public decimal Price
{
    get => _price;
    set
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Price cannot be negative.");
        _price = value;
    }
}

// Models/User.cs - walidacja emaila
public string Email
{
    get => _email;
    set
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
            throw new ArgumentException("Invalid email address.");
        _email = value;
    }
}
```

**Indekser:**
```csharp
// Models/Warehouse.cs - szybki dostęp do stanu magazynowego
public InventoryItem? this[int productId]
{
    get => _stock.GetValueOrDefault(productId);
    set
    {
        if (value is null) return;
        _stock[productId] = value;
    }
}
```

### 4. **Elementy Statyczne**

**Liczniki w klasach:**
```csharp
// Models/User.cs
private static int _userCounter;
public static int UserCounter => _userCounter;

protected User(...) 
{
    Id = Interlocked.Increment(ref _userCounter);  // Thread-safe counter
    ...
}

// Models/Product.cs
private static int _productCounter;
public static int ProductCounter => _productCounter;
```

**Konfiguracja statyczna:**
```csharp
// Utilities/AppConfig.cs
public static class AppConfig
{
    public const string DataDirectory = "Data";
    public static string ProductsFilePath => Path.Combine(DataDirectory, "products.json");
    public static string OrdersFilePath => Path.Combine(DataDirectory, "orders.json");
    // ...
}
```

**Helper statyczny:**
```csharp
// Utilities/JsonStorage.cs
public static class JsonStorage
{
    public static async Task<IReadOnlyList<T>> LoadAsync<T>(string filePath, CancellationToken cancellationToken = default)
    {
        // Wczytuje JSON z pliku
    }
}
```

### 5. **Dziedziczenie**

**Hierarchia `User`:**
```
User (abstrakcyjna, Models/User.cs)
  ?? Customer (Models/Customer.cs) - konkretna
  ?? Employee (Models/Employee.cs) - konkretna
       ?? Admin (Models/Admin.cs) - dziedziczy po Employee
```

**Hierarchia `Payment`:**
```
Payment (abstrakcyjna, Models/Payment.cs)
  ?? CardPayment (Models/CardPayment.cs) - konkretna
  ?? CashPayment (Models/CashPayment.cs) - konkretna
```

```csharp
// Models/Customer.cs - rozszerza User
public sealed class Customer : User
{
    public int LoyaltyPoints { get; private set; }
    public override string GetRole() => "Customer";
}

// Models/Admin.cs - wielopoziomowe dziedziczenie
public sealed class Admin : Employee
{
    public string PermissionLevel { get; set; }
    public override string GetRole() => "Admin";
}
```

### 6. **Polimorfizm**

Metody `virtual`/`override` w hierarchii `Payment`:

```csharp
// Models/Payment.cs - metoda wirtualna
public abstract class Payment : BaseEntity
{
    public abstract string Method { get; }
    public virtual async Task<bool> ProcessAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(150, cancellationToken);
        return Amount >= 0;
    }
}

// Models/CardPayment.cs - override
public sealed class CardPayment : Payment
{
    public override string Method => "Card";
    public override async Task<bool> ProcessAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(250, cancellationToken);
        return Amount > 0;
    }
}

// Models/CashPayment.cs - override
public sealed class CashPayment : Payment
{
    public override string Method => "Cash";
    public override async Task<bool> ProcessAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        return ReceivedAmount >= Amount;
    }
}

// Program.cs - polimorfizm w runtime'ie
Payment payment = order.TotalAmount > 1000
    ? new CardPayment(order.TotalAmount, "1234")
    : new CashPayment(order.TotalAmount, order.TotalAmount);
await paymentProcessor.ProcessAsync(payment);
```

### 7. **Interfejsy i Abstrakcja**

**Interfejsy:**
```csharp
// Interfaces/IRepository.cs - kontrakt generyczny
public interface IRepository<T> where T : class
{
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

// Interfaces/ILogger.cs
public interface ILogger
{
    Task LogAsync(string message, CancellationToken cancellationToken = default);
}

// Interfaces/IPaymentProcessor.cs
public interface IPaymentProcessor
{
    Task ProcessAsync(Payment payment, CancellationToken cancellationToken = default);
}

// Interfaces/IReportGenerator.cs
public interface IReportGenerator
{
    string Name { get; }
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
```

**Klasy abstrakcyjne:**
```csharp
// Models/User.cs
public abstract class User : BaseEntity { ... }

// Models/Payment.cs
public abstract class Payment : BaseEntity { ... }
```

**Implementacje interfejsów:**
```csharp
// Repositories/JsonRepository.cs - implementuje IRepository<T>
public class JsonRepository<T> : IRepository<T> where T : BaseEntity
{
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default) { ... }
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) { ... }
    // ...
}

// Services/FileLogger.cs - implementuje ILogger
public sealed class FileLogger : ILogger
{
    public async Task LogAsync(string message, CancellationToken cancellationToken = default) { ... }
}

// Plugins/SalesReportGenerator.cs - implementuje IReportGenerator
public sealed class SalesReportGenerator : IReportGenerator
{
    public string Name => "sales";
    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default) { ... }
}
```

### 8. **Generyki i Kolekcje**

```csharp
// Generyczne repozytorium - Models/Repositories/JsonRepository.cs
public class JsonRepository<T> : IRepository<T> where T : BaseEntity { ... }

// Program.cs - użycie generyk
IRepository<Product> productRepository = new JsonRepository<Product>(AppConfig.ProductsFilePath);
IRepository<Order> orderRepository = new JsonRepository<Order>(AppConfig.OrdersFilePath);
IRepository<InventoryItem> inventoryRepository = new JsonRepository<InventoryItem>(AppConfig.InventoryFilePath);

// Kolekcje w modelach - Models/Warehouse.cs
private readonly Dictionary<int, InventoryItem> _stock = new();
private readonly Queue<Order> _orderQueue = new();

// Models/Order.cs
public List<OrderItem> Items { get; set; } = new();
```

### 9. **Delegaty i Zdarzenia**

```csharp
// Events/EventDelegates.cs - definicje delegatów
public delegate void OrderCreatedHandler(object? sender, Order order);
public delegate void LowStockHandler(object? sender, Product product);
public delegate void PaymentCompletedHandler(object? sender, Payment payment);

// Events/EventHub.cs - centrum zdarzeń
public static class EventHub
{
    public static event OrderCreatedHandler? OnOrderCreated;
    public static event LowStockHandler? OnLowStock;
    public static event PaymentCompletedHandler? OnPaymentCompleted;

    public static void RaiseOrderCreated(Order order) => OnOrderCreated?.Invoke(null, order);
    public static void RaiseLowStock(Product product) => OnLowStock?.Invoke(null, product);
    public static void RaisePaymentCompleted(Payment payment) => OnPaymentCompleted?.Invoke(null, payment);
}

// Program.cs - subskrypcja zdarzeń
EventHub.OnOrderCreated += (_, order) => 
    Console.WriteLine($"Zdarzenie: utworzono zamówienie #{order.Id}");
EventHub.OnLowStock += (_, product) => 
    Console.WriteLine($"Zdarzenie: niski stan magazynowy dla {product.Name}");

// Services/OrderService.cs - wywoływanie zdarzeń
if (item.Quantity <= item.LowStockThreshold)
{
    EventHub.RaiseLowStock(product);
}
EventHub.RaiseOrderCreated(order);
```

### 10. **Przeciążanie Operatorów**

```csharp
// Models/Product.cs - porównanie cen
public static bool operator >(Product left, Product right) => left.Price > right.Price;
public static bool operator <(Product left, Product right) => left.Price < right.Price;
public static bool operator ==(Product? left, Product? right)
{
    if (ReferenceEquals(left, right)) return true;
    if (left is null || right is null) return false;
    return left.Name == right.Name && left.Price == right.Price;
}
public static bool operator !=(Product? left, Product? right) => !(left == right);

// Program.cs - użycie operatora
Console.WriteLine(phone > monitor
    ? $"{phone.Name} jest droższy niż {monitor.Name}."
    : $"{phone.Name} jest tańszy...");

// Models/Order.cs - łączenie zamówień
public static Order operator +(Order left, Order right)
{
    var merged = new Order(left.CustomerId);
    foreach (var item in left.Items.Concat(right.Items))
    {
        // Logika łączenia pozycji...
    }
    return merged;
}

// Models/InventoryItem.cs - operacje na ilości
public static InventoryItem operator +(InventoryItem item, int amount)
{
    item.Quantity += amount;
    return item;
}
public static InventoryItem operator -(InventoryItem item, int amount)
{
    item.Quantity = Math.Max(0, item.Quantity - amount);
    return item;
}
```

### 11. **Asynchroniczność**

```csharp
// Program.cs - main async
private static async Task Main()
{
    await SeedDataAsync(productRepository, customerRepository, inventoryRepository);
    await SyncWarehouseAsync(warehouse, inventoryRepository);
    
    while (running)
    {
        switch (Console.ReadLine())
        {
            case "1": await ListProductsAsync(productRepository); break;
            case "2": await CreateManualOrderAsync(...); break;
            case "4": await reportService.GenerateAllAsync(); break;
        }
    }
}

// Services/OrderService.cs - async operacje
public async Task<Order> CreateOrderAsync(int customerId, Dictionary<int, int> request, CancellationToken cancellationToken = default)
{
    var products = await _productRepository.GetAllAsync(cancellationToken);
    var inventory = (await _inventoryRepository.GetAllAsync(cancellationToken)).ToDictionary(i => i.ProductId);
    
    await _orderRepository.AddAsync(order, cancellationToken);
    await _logger.LogAsync($"Order {order.Id} created...", cancellationToken);
}

// Services/FileLogger.cs - async logowanie
public async Task LogAsync(string message, CancellationToken cancellationToken = default)
{
    await File.AppendAllTextAsync(AppConfig.LogFilePath, $"{DateTime.UtcNow:O} - {message}{Environment.NewLine}", cancellationToken);
}

// Models/Payment.cs - async przetwarzanie
public virtual async Task<bool> ProcessAsync(CancellationToken cancellationToken = default)
{
    await Task.Delay(150, cancellationToken);
    return Amount >= 0;
}
```

### 12. **Refleksja**

```csharp
// Utilities/ReflectionHelper.cs - odkrywanie generatorów raportów
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
            generators.Add(generator);
    }
    return generators;
}

// Utilities/ReflectionHelper.cs - inspekcja metadanych
public static IReadOnlyList<string> InspectModelMetadata<T>()
{
    return typeof(T)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(p => $"{p.Name}: {p.PropertyType.Name}")
        .ToList();
}

// Utilities/ReflectionHelper.cs - mapowanie kontraktów usług
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

// Program.cs - użycie refleksji
var serviceMap = ReflectionHelper.AutoRegisterServices();
Console.WriteLine($"Refleksja wykryła kontrakty usług: {serviceMap.Count}");

// Program.cs - opcja 5 w menu
foreach (var line in ReflectionHelper.InspectModelMetadata<Product>())
{
    Console.WriteLine(line);  // Wyświetla: Name: String, Price: Decimal, Id: Int32
}
```

## Funkcje dodatkowe

### 1. Zarządzanie produktami, klientami, zamówieniami i magazynem

Każda encja ma własne generyczne repozytorium JSON (`Repositories/JsonRepository.cs`):

```csharp
// Program.cs - każdy typ danych ma swoje repozytorium
IRepository<Product> productRepository = new JsonRepository<Product>(AppConfig.ProductsFilePath);
IRepository<Customer> customerRepository = new JsonRepository<Customer>(AppConfig.CustomersFilePath);
IRepository<Order> orderRepository = new JsonRepository<Order>(AppConfig.OrdersFilePath);
IRepository<InventoryItem> inventoryRepository = new JsonRepository<InventoryItem>(AppConfig.InventoryFilePath);
```

Operacje na zamówieniach orkiestruje `Services/OrderService.cs` - waliduje stan magazynu, tworzy zamówienie, aktualizuje `InventoryItem` i wywołuje zdarzenia.  
Magazyn (`Models/Warehouse.cs`) utrzymuje w pamięci słownik stanów i kolejkę zamówień do przetworzenia.

---

### 2. Przetwarzanie płatności

Zaimplementowane w `Services/PaymentProcessor.cs`:

```csharp
// Services/PaymentProcessor.cs
public async Task ProcessAsync(Payment payment, CancellationToken cancellationToken = default)
{
    var success = await payment.ProcessAsync(cancellationToken);  // Polimorfizm
    if (!success)
        throw new DomainException("Payment failed.");

    await _logger.LogAsync($"Payment processed: {payment.Method} {payment.Amount:C}", cancellationToken);
    EventHub.RaisePaymentCompleted(payment);  // Zdarzenie
}
```

Typ płatności wybierany polimorficznie w `Program.cs`:

```csharp
// Program.cs
Payment payment = order.TotalAmount > 1000
    ? new CardPayment(order.TotalAmount, "1234")   // Models/CardPayment.cs
    : new CashPayment(order.TotalAmount, order.TotalAmount);  // Models/CashPayment.cs

await paymentProcessor.ProcessAsync(payment);
```

---

### 3. Logowanie do pliku (`Data/logs.txt`)

Zaimplementowane w `Services/FileLogger.cs`. Używa `SemaphoreSlim` do thread-safe zapisu:

```csharp
// Services/FileLogger.cs
public sealed class FileLogger : ILogger
{
    private readonly SemaphoreSlim _gate = new(1, 1);  // Blokada przed równoległym zapisem

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
```

Logi są zapisywane przy: starcie/zatrzymaniu aplikacji, tworzeniu zamówień, przetwarzaniu płatności i generowaniu raportów.

---

### 4. Trwałość danych w JSON

Zaimplementowana przez `Utilities/JsonStorage.cs` (statyczny helper) i `Repositories/JsonRepository.cs` (generyczne repozytorium):

```csharp
// Utilities/JsonStorage.cs - odczyt z pliku, tworzenie jeśli nie istnieje
public static async Task<IReadOnlyList<T>> LoadAsync<T>(string filePath, CancellationToken cancellationToken = default)
{
    EnsureEnvironment();

    if (!File.Exists(filePath))
        await File.WriteAllTextAsync(filePath, "[]", cancellationToken);  // Tworzy pusty plik

    await using var stream = File.OpenRead(filePath);
    var items = await JsonSerializer.DeserializeAsync<List<T>>(stream, cancellationToken: cancellationToken);
    return items ?? new List<T>();
}

// Repositories/JsonRepository.cs - zapis do pliku
private async Task SaveAsync(List<T> entities, CancellationToken cancellationToken)
{
    var json = JsonSerializer.Serialize(entities, _serializerOptions);  // WriteIndented = true
    await File.WriteAllTextAsync(_filePath, json, cancellationToken);
}
```

Ścieżki do wszystkich plików centralnie w `Utilities/AppConfig.cs`.

---

### 5. Walidacja danych i własny wyjątek `DomainException`

Wyjątek zdefiniowany w `Models/DomainException.cs`:

```csharp
// Models/DomainException.cs
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

Rzucany w logice biznesowej:

```csharp
// Services/OrderService.cs - walidacja stanu magazynowego
if (!inventory.TryGetValue(pair.Key, out var item) || item.Quantity < pair.Value)
    throw new DomainException($"Insufficient stock for {product.Name}.");

// Repositories/JsonRepository.cs - walidacja przy aktualizacji
if (index < 0)
    throw new DomainException($"Entity with id {entity.Id} not found.");

// Services/PaymentProcessor.cs - walidacja płatności
if (!success)
    throw new DomainException("Payment failed.");
```

Przechwytywany w `Program.cs`:

```csharp
// Program.cs
catch (DomainException ex)
{
    Console.WriteLine($"Nie udało się utworzyć zamówienia: {ex.Message}");
}
```

---

### 6. Dane startowe (Seed)

Zaimplementowane w `Program.cs` - metoda `SeedDataAsync`:

```csharp
// Program.cs - seed produktów (tylko jeśli plik jest pusty)
if (!products.Any())
{
    await productRepository.AddAsync(new Product("Phone", 2200m));
    await productRepository.AddAsync(new Product("Monitor", 900m));
    await productRepository.AddAsync(new Product("Keyboard", 200m));
}

// Seed inventory - uzupełnia brakujące wpisy dla istniejących produktów
var inventoryProductIds = new HashSet<int>((await inventoryRepository.GetAllAsync()).Select(i => i.ProductId));
foreach (var product in products)
{
    if (!inventoryProductIds.Contains(product.Id))
        await inventoryRepository.AddAsync(new InventoryItem(product.Id, GetSeedQuantity(product.Name)));
}

// Seed klienta
if (!(await customerRepository.GetAllAsync()).Any())
    await customerRepository.AddAsync(new Customer("Anna", "Nowak", "anna@local.dev"));
```

Domyślne ilości startowe per produkt:

```csharp
// Program.cs
private static int GetSeedQuantity(string productName) => productName switch
{
    "Phone" => 8,
    "Monitor" => 6,
    "Keyboard" => 20,
    _ => 10  // Domyślna ilość dla wszystkich pozostałych produktów
};
```

