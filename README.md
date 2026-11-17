# EF Core Tracking Performance Lab

A practical .NET 10 lab comparing **Entity Framework Core query tracking strategies** through functional experiments and performance benchmarks.

The project explores how change tracking and identity resolution affect object references, execution time, memory allocations, and garbage collection.

## 🎯 What Does This Project Demonstrate?

EF Core provides three query tracking strategies:

| Strategy | Change Tracking | Identity Resolution |
|---|---|---|
| `AsTracking()` | Yes | Yes |
| `AsNoTracking()` | No | No |
| `AsNoTrackingWithIdentityResolution()` | No (DbContext) | Yes (temporary) |

The goal is to understand their behavior and determine which strategy performs best for a specific read-heavy workload.

## 🛠 Tech Stack

- .NET 10
- Entity Framework Core 10
- C#
- SQL Server LocalDB
- BenchmarkDotNet 0.15.8
- Code First Migrations

## 📁 Project Structure

```text
EFCoreTrackingLab/
├── Benchmarks/
│   └── OrderQueryBenchmarks.cs
├── Data/
│   └── AppDbContext.cs
├── Models/
│   ├── Customer.cs
│   └── Order.cs
├── Migrations/
├── Program.cs
└── EFCoreTrackingLab.csproj
```

## 🗄 Test Dataset

The database contains:

| Entity | Count |
|---|---:|
| Customers | 10 |
| Orders | 1,000 |
| Orders per Customer | 100 |

The dataset deliberately contains repeated customer relationships to demonstrate identity resolution.

## 🧪 Functional Experiments

All three strategies were tested using identical queries and fresh `DbContext` instances.

### Verified Results

| Metric | Tracking | NoTracking | Identity Resolution |
|---|---:|---:|---:|
| Tracked Orders | 1,000 | 0 | 0 |
| Tracked Customers | 10 | 0 | 0 |
| Distinct Customer IDs | 10 | 10 | 10 |
| Distinct Customer References | 10 | 1,000 | 10 |
| Same Customer Reference | True | False | True |

### Key Observation

`AsNoTrackingWithIdentityResolution()` reuses customer instances within a query result without registering those entities in the main DbContext's ChangeTracker.

It uses a temporary standalone change tracker for identity resolution.

## ⚡ Performance Benchmark

The three query strategies were measured using BenchmarkDotNet in Release mode.

### Actual Results

| Method | Mean | Ratio | Gen0 | Gen1 | Allocated |
|---|---:|---:|---:|---:|---:|
| AsNoTracking | 1.345 ms | 1.00 | 54.6875 | 17.5781 | 1.01 MB |
| AsTracking | 2.079 ms | 1.55 | 78.1250 | 35.1563 | 1.44 MB |
| AsNoTrackingWithIdentityResolution | 2.039 ms | 1.52 | 97.6563 | 31.2500 | 1.79 MB |

**Benchmark notes:**

- Lower execution time and allocations are better for this workload.
- Ratio is relative to the AsNoTracking baseline.
- Gen0 and Gen1 represent normalized GC collections per 1,000 benchmark operations.
- Results were measured locally and may vary across environments.

## 📊 Key Findings

### 1. AsNoTracking achieved the best performance

For this workload, `AsNoTracking()` delivered the lowest execution time and managed memory allocations.

Compared with AsTracking, it reduced execution time by approximately **35%**.

### 2. Identity resolution does not guarantee better performance

Although `AsNoTrackingWithIdentityResolution()` reduced customer object duplication, it introduced overhead associated with temporary identity tracking.

In this particular benchmark, it allocated more memory than both alternatives.

### 3. Fewer objects do not necessarily mean fewer allocations

The total allocation cost includes not only entity instances but also EF Core's internal tracking structures.

### 4. Tracking remains useful for updates

`AsTracking()` is appropriate when entities need to be modified and their changes persisted through `SaveChanges()`.

### 5. Benchmark results are workload-dependent

Entity size, relationship duplication, database performance, and query complexity can substantially change the outcome.

## ▶️ Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server LocalDB
- EF Core CLI tools

### Clone the Repository

```bash
git clone <YOUR-REPOSITORY-URL>
cd EFCoreTrackingLab
```

### Restore Dependencies

```bash
dotnet restore
```

### Apply Migrations

```bash
dotnet ef database update
```

### Run the Functional Experiments

```bash
dotnet run
```

Expected dataset:

```text
Customers: 10
Orders: 1000
```

### Run Performance Benchmarks

```bash
dotnet run -c Release -- --benchmark
```

BenchmarkDotNet generates detailed reports in its artifacts directory.

## ⚠️ Benchmark Limitations

The measurements include:

- DbContext creation and disposal
- SQL Server interaction
- Query execution
- Entity materialization
- EF Core tracking-related overhead

The benchmark does not isolate EF Core's internal tracking costs from database and connection overhead.

Results are based on a single workload and should not be generalized to every production environment.

Performance testing under representative workloads is recommended before making application-wide optimization decisions.

## 🧠 Final Takeaway

> **Measure performance instead of assuming that fewer objects or less tracking always produces faster queries.**

For the tested workload, `AsNoTracking()` was the most efficient strategy.

However, `AsNoTrackingWithIdentityResolution()` remains valuable when repeated related entities make reference reuse important.

## 📚 References

- [Microsoft — Tracking vs. No-Tracking Queries](https://learn.microsoft.com/en-us/ef/core/querying/tracking)
- [Microsoft — Efficient Querying](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying)
- [Milan Jovanović — EF Core AsNoTracking and Identity Resolution](https://milanjovanovic.tech/blog/ef-core-asnotracking-identity-resolution)
- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)