using WarehouseManagementSystem.Events;
using WarehouseManagementSystem.Interfaces;
using WarehouseManagementSystem.Models;
using WarehouseManagementSystem.Repositories;
using WarehouseManagementSystem.Services;
using WarehouseManagementSystem.Utilities;

namespace WarehouseManagementSystem;

internal static class Program
{
    private static async Task Main()
    {
        JsonStorage.EnsureEnvironment();

        // Constructor injection is used as dependency injection.
        ILogger logger = new FileLogger();
        IRepository<Product> productRepository = new JsonRepository<Product>(AppConfig.ProductsFilePath);
        IRepository<Customer> customerRepository = new JsonRepository<Customer>(AppConfig.CustomersFilePath);
        IRepository<Order> orderRepository = new JsonRepository<Order>(AppConfig.OrdersFilePath);
        IRepository<InventoryItem> inventoryRepository = new JsonRepository<InventoryItem>(AppConfig.InventoryFilePath);

        var orderService = new OrderService(orderRepository, productRepository, inventoryRepository, logger);
        IPaymentProcessor paymentProcessor = new PaymentProcessor(logger);
        var reportService = new ReportService(logger);
        var warehouse = new Warehouse();

        SubscribeToEvents(logger);
        var existingOrders = await orderRepository.GetAllAsync();
        Order.InitializeCounter(existingOrders.Any() ? existingOrders.Max(o => o.Id) : 0);
        await SeedDataAsync(productRepository, customerRepository, inventoryRepository);
        await SyncWarehouseAsync(warehouse, inventoryRepository);

        Console.WriteLine("System Zarządzania Magazynem (.NET 8)");
        await logger.LogAsync("Aplikacja uruchomiona.");

        var serviceMap = ReflectionHelper.AutoRegisterServices();
        Console.WriteLine($"Refleksja wykryła kontrakty usług: {serviceMap.Count}");

        bool running = true;
        while (running)
        {
            Console.WriteLine();
            Console.WriteLine("1. Lista produktów");
            Console.WriteLine("2. Utwórz zamówienie (ręcznie)");
            Console.WriteLine("3. Przetwórz płatność");
            Console.WriteLine("4. Generuj raporty");
            Console.WriteLine("5. Pokaż metadane produktu (refleksja)");
            Console.WriteLine("0. Wyjście");
            Console.Write("Wybierz opcję: ");

            switch (Console.ReadLine())
            {
                case "1":
                    await ListProductsAsync(productRepository);
                    break;
                case "2":
                    await CreateManualOrderAsync(orderService, customerRepository, productRepository, warehouse);
                    break;
                case "3":
                    await ProcessSamplePaymentAsync(orderRepository, paymentProcessor);
                    break;
                case "4":
                    await reportService.GenerateAllAsync();
                    break;
                case "5":
                    foreach (var line in ReflectionHelper.InspectModelMetadata<Product>())
                    {
                        Console.WriteLine(line);
                    }
                    break;
                case "0":
                    running = false;
                    break;
            }
        }

        await logger.LogAsync("Aplikacja zatrzymana.");
    }

    private static void SubscribeToEvents(ILogger logger)
    {
        EventHub.OnOrderCreated += (_, order) => Console.WriteLine($"Zdarzenie: utworzono zamówienie #{order.Id}");
        EventHub.OnLowStock += (_, product) => Console.WriteLine($"Zdarzenie: niski stan magazynowy dla {product.Name}");
        EventHub.OnPaymentCompleted += async (_, payment) => await logger.LogAsync($"Zdarzenie: płatność zakończona ({payment.Method})");
    }

    private static async Task SeedDataAsync(
        IRepository<Product> productRepository,
        IRepository<Customer> customerRepository,
        IRepository<InventoryItem> inventoryRepository)
    {
        var products = (await productRepository.GetAllAsync()).ToList();

        if (!products.Any())
        {
            var phone = new Product("Phone", 2200m);
            var monitor = new Product("Monitor", 900m);
            var keyboard = new Product("Keyboard", 200m);

            await productRepository.AddAsync(phone);
            await productRepository.AddAsync(monitor);
            await productRepository.AddAsync(keyboard);

            products = new List<Product> { phone, monitor, keyboard };

            // Operator overloading for product comparison.
            Console.WriteLine(phone > monitor
                ? $"{phone.Name} jest droższy niż {monitor.Name}."
                : $"{phone.Name} jest tańszy lub w tej samej cenie co {monitor.Name}.");
        }

        var inventoryProductIds = new HashSet<int>((await inventoryRepository.GetAllAsync()).Select(i => i.ProductId));
        foreach (var product in products)
        {
            if (inventoryProductIds.Contains(product.Id))
            {
                continue;
            }

            await inventoryRepository.AddAsync(new InventoryItem(product.Id, GetSeedQuantity(product.Name)));
        }

        if (!(await customerRepository.GetAllAsync()).Any())
        {
            await customerRepository.AddAsync(new Customer("Anna", "Nowak", "anna@local.dev"));
        }
    }

    private static int GetSeedQuantity(string productName) => productName switch
    {
        "Phone" => 8,
        "Monitor" => 6,
        "Keyboard" => 20,
        _ => 10
    };

    private static async Task SyncWarehouseAsync(Warehouse warehouse, IRepository<InventoryItem> inventoryRepository)
    {
        warehouse.SyncInventory(await inventoryRepository.GetAllAsync());
    }

    private static async Task ListProductsAsync(IRepository<Product> productRepository)
    {
        var products = await productRepository.GetAllAsync();
        foreach (var product in products)
        {
            Console.WriteLine($"#{product.Id} {product.Name} - {product.Price:C}");
        }
    }

    private static async Task CreateManualOrderAsync(
        OrderService orderService,
        IRepository<Customer> customerRepository,
        IRepository<Product> productRepository,
        Warehouse warehouse)
    {
        var customer = (await customerRepository.GetAllAsync()).First();

        var products = await productRepository.GetAllAsync();
        if (!products.Any())
        {
            Console.WriteLine("Brak produktów do zamówienia.");
            return;
        }

        Console.WriteLine("Podaj pozycje zamówienia w formacie: produktId ilość (np. 2 3)");
        Console.WriteLine("Wpisz pustą linię, aby zakończyć.");

        var request = new Dictionary<int, int>();
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                break;
            }

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2
                || !int.TryParse(parts[0], out var productId)
                || !int.TryParse(parts[1], out var quantity)
                || quantity <= 0)
            {
                Console.WriteLine("Niepoprawny format. Użyj: produktId ilość, gdzie ilość > 0.");
                continue;
            }

            if (!products.Any(p => p.Id == productId))
            {
                Console.WriteLine($"Produkt o ID {productId} nie istnieje.");
                continue;
            }

            request[productId] = request.TryGetValue(productId, out var existing)
                ? existing + quantity
                : quantity;
        }

        if (request.Count == 0)
        {
            Console.WriteLine("Nie dodano żadnej pozycji zamówienia.");
            return;
        }

        try
        {
            var order = await orderService.CreateOrderAsync(customer.Id, request);
            warehouse.EnqueueOrder(order);
            var queued = warehouse.DequeueOrder();

            Console.WriteLine($"Utworzono zamówienie: {queued}");
        }
        catch (DomainException ex)
        {
            Console.WriteLine($"Nie udało się utworzyć zamówienia: {ex.Message}");
        }
    }

    private static async Task ProcessSamplePaymentAsync(IRepository<Order> orderRepository, IPaymentProcessor paymentProcessor)
    {
        var order = (await orderRepository.GetAllAsync()).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
        if (order is null)
        {
            Console.WriteLine("Brak zamówienia do opłacenia.");
            return;
        }

        // Runtime polymorphism: base type points to derived payment types.
        Payment payment = order.TotalAmount > 1000
            ? new CardPayment(order.TotalAmount, "1234")
            : new CashPayment(order.TotalAmount, order.TotalAmount);

        await paymentProcessor.ProcessAsync(payment);
        order.IsPaid = true;
        await orderRepository.UpdateAsync(order);
        Console.WriteLine("Płatność została przetworzona.");
    }
}
