
using BenchmarkDotNet.Attributes;
using EFCoreTrackingLab.Data;
using EFCoreTrackingLab.Models;
using Microsoft.EntityFrameworkCore;

namespace EFCoreTrackingLab.Benchmarks;

[MemoryDiagnoser]
public class OrderQueryBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        using var db = new AppDbContext();

        if (db.Customers.Count() != 10 ||
            db.Orders.Count() != 1000)
        {
            throw new InvalidOperationException(
                "Benchmark data is not ready.");
        }
    }

    [Benchmark(Baseline = true)]
    public async Task<List<Order>> AsNoTrackingQuery()
    {
        await using var db = new AppDbContext();

        return await db.Orders
            .AsNoTracking()
            .Include(o => o.Customer)
            .OrderBy(o => o.Id)
            .ToListAsync();
    }

    [Benchmark]
    public async Task<List<Order>> AsTrackingQuery()
    {
        await using var db = new AppDbContext();

        return await db.Orders
            .AsTracking()
            .Include(o => o.Customer)
            .OrderBy(o => o.Id)
            .ToListAsync();
    }

    [Benchmark]
    public async Task<List<Order>> AsNoTrackingWithIdentityResolutionQuery()
    {
        await using var db = new AppDbContext();

        return await db.Orders
            .AsNoTrackingWithIdentityResolution()
            .Include(o => o.Customer)
            .OrderBy(o => o.Id)
            .ToListAsync();
    }
}
