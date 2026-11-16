using EFCoreTrackingLab.Data;
using EFCoreTrackingLab.Models;
using Microsoft.EntityFrameworkCore;

await using var db = new AppDbContext();

await db.Database.MigrateAsync();

if (!await db.Customers.AnyAsync())
{
    var customers = Enumerable.Range(1, 10)
        .Select(i => new Customer
        {
            Name = $"Customer {i}"
        })
        .ToList();

    db.Customers.AddRange(customers);

    await db.SaveChangesAsync();

    var orders = customers.SelectMany(customer =>
        Enumerable.Range(1, 100)
            .Select(i => new Order
            {
                CustomerId = customer.Id,
                Total = 100 + i,
                CreatedAt = new DateTime(2026, 1, 1)
                    .AddDays(i - 1)
            }))
        .ToList();

    db.Orders.AddRange(orders);

    await db.SaveChangesAsync();
}

Console.WriteLine(
    $"Customers: {await db.Customers.CountAsync()}");

Console.WriteLine(
    $"Orders: {await db.Orders.CountAsync()}");



Console.WriteLine("\n--- Tracking Test ---");

await using var trackingDb = new AppDbContext();

var ordersTracking = await trackingDb.Orders
    .AsNoTracking()
    .Include(o => o.Customer)
    .OrderBy(o => o.Id)
    .ToListAsync();

// هل أول Order وثاني Order يشتركان في نفس Customer Object؟
bool sameReference = ReferenceEquals(
    ordersTracking[0].Customer,
    ordersTracking[1].Customer
);

Console.WriteLine($"Same Customer Reference: {sameReference}");

Console.WriteLine(
    $"Tracked Orders: " +
    $"{trackingDb.ChangeTracker.Entries<Order>().Count()}");

Console.WriteLine(
    $"Tracked Customers: " +
    $"{trackingDb.ChangeTracker.Entries<Customer>().Count()}");

Console.WriteLine(
    $"Total Tracked Entities: " +
    $"{trackingDb.ChangeTracker.Entries().Count()}");


// ---
Console.WriteLine("\n--- NoTracking Test ---");

await using var noTrackingDb = new AppDbContext();

var noTrackingOrders = await noTrackingDb.Orders
    .AsNoTracking()
    .Include(o => o.Customer)
    .OrderBy(o => o.Id)
    .ToListAsync();

// 1. هل أول Orderين يشتركان في نفس Customer Object؟
bool sameReference1 = ReferenceEquals(
    noTrackingOrders[0].Customer,
    noTrackingOrders[1].Customer
);

// 2. كم Entity يتتبعها DbContext؟
int trackedOrders = noTrackingDb.ChangeTracker
    .Entries<Order>()
    .Count();

int trackedCustomers = noTrackingDb.ChangeTracker
    .Entries<Customer>()
    .Count();

// 3. كم Customer مختلفًا بحسب Primary Key؟
int distinctCustomerIds = noTrackingOrders
    .Select(o => o.Customer.Id)
    .Distinct()
    .Count();

// 4. كم Customer Object مختلفًا في الذاكرة؟
int distinctCustomerReferences = noTrackingOrders
    .Select(o => o.Customer)
    .Distinct(ReferenceEqualityComparer.Instance)
    .Count();

Console.WriteLine($"Same Customer Reference: {sameReference1}");
Console.WriteLine($"Tracked Orders: {trackedOrders}");
Console.WriteLine($"Tracked Customers: {trackedCustomers}");
Console.WriteLine($"Distinct Customer IDs: {distinctCustomerIds}");
Console.WriteLine($"Distinct Customer References: {distinctCustomerReferences}");



Console.WriteLine(
    "\n--- NoTracking With Identity Resolution Test ---");

await using var identityDb = new AppDbContext();

var identityOrders = await identityDb.Orders
    .AsNoTrackingWithIdentityResolution()
    .Include(o => o.Customer)
    .OrderBy(o => o.Id)
    .ToListAsync();

// 1. هل أول Orderين يشتركان في نفس Customer Object؟
bool sameReference2 = ReferenceEquals(
    identityOrders[0].Customer,
    identityOrders[1].Customer
);

// 2. عدد Entities المتتبعة في DbContext
int trackedOrders2 = identityDb.ChangeTracker
    .Entries<Order>()
    .Count();

int trackedCustomers2 = identityDb.ChangeTracker
    .Entries<Customer>()
    .Count();

// 3. عدد العملاء حسب Primary Key
int distinctCustomerIds2 = identityOrders
    .Select(o => o.Customer.Id)
    .Distinct()
    .Count();

// 4. عدد Customer Objects الفعلية في الذاكرة
int distinctCustomerReferences2 = identityOrders
    .Select(o => o.Customer)
    .Distinct(ReferenceEqualityComparer.Instance)
    .Count();

Console.WriteLine($"Same Customer Reference: {sameReference2}");
Console.WriteLine($"Tracked Orders: {trackedOrders2}");
Console.WriteLine($"Tracked Customers: {trackedCustomers2}");
Console.WriteLine($"Distinct Customer IDs: {distinctCustomerIds2}");
Console.WriteLine($"Distinct Customer References: {distinctCustomerReferences2}");
