# Milestone 1 — Data Model

> Define the database entities, run EF Core migrations, set up repositories, and seed test data.

---

## Goal

By the end of this milestone the database has the correct tables for spools and print jobs, repositories are in place to read and write data, and seed data exists so you can test without manually inserting rows.

---

## Depends On

- Milestone 0 — Project Bootstrap

---

## Tasks

- [ ] `Spool` entity + migration
- [ ] `PrintJob` entity + migration
- [ ] `SpoolRepository` + `PrintJobRepository`
- [ ] Seed data for testing

---

## What to Create

### `Spool.cs`

```csharp
// src/Spoolarr.Api/Models/Spool.cs
public class Spool
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NfcTagUid { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public float InitialWeightG { get; set; }
    public float CurrentWeightG { get; set; }
    public float LowStockThresholdG { get; set; } = 100f;
    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastScannedAt { get; set; }
    public string? Notes { get; set; }

    public ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
}
```

### `PrintJob.cs`

```csharp
// src/Spoolarr.Api/Models/PrintJob.cs
public class PrintJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SpoolId { get; set; }
    public float GramsUsed { get; set; }
    public string? PrintName { get; set; }
    public DateTime PrintedAt { get; set; } = DateTime.UtcNow;
    public string Source { get; set; } = "mqtt"; // "mqtt" or "manual"

    public Spool Spool { get; set; } = null!;
}
```

### Update `FilamentDbContext.cs`

```csharp
// src/Spoolarr.Api/Data/FilamentDbContext.cs
public class FilamentDbContext : DbContext
{
    public FilamentDbContext(DbContextOptions<FilamentDbContext> options)
        : base(options) { }

    public DbSet<Spool> Spools => Set<Spool>();
    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Spool>()
            .HasIndex(s => s.NfcTagUid)
            .IsUnique();

        modelBuilder.Entity<Spool>()
            .HasMany(s => s.PrintJobs)
            .WithOne(p => p.Spool)
            .HasForeignKey(p => p.SpoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### `SpoolRepository.cs`

```csharp
// src/Spoolarr.Api/Repositories/SpoolRepository.cs
public interface ISpoolRepository
{
    Task<List<Spool>> GetAllAsync();
    Task<Spool?> GetByIdAsync(Guid id);
    Task<Spool?> GetByNfcTagUidAsync(string uid);
    Task<Spool?> GetActiveAsync();
    Task<Spool> CreateAsync(Spool spool);
    Task<Spool> UpdateAsync(Spool spool);
    Task DeleteAsync(Guid id);
}

public class SpoolRepository : ISpoolRepository
{
    private readonly FilamentDbContext _db;
    public SpoolRepository(FilamentDbContext db) => _db = db;

    public Task<List<Spool>> GetAllAsync() =>
        _db.Spools.OrderByDescending(s => s.LastScannedAt).ToListAsync();

    public Task<Spool?> GetByIdAsync(Guid id) =>
        _db.Spools.Include(s => s.PrintJobs).FirstOrDefaultAsync(s => s.Id == id);

    public Task<Spool?> GetByNfcTagUidAsync(string uid) =>
        _db.Spools.FirstOrDefaultAsync(s => s.NfcTagUid == uid);

    public Task<Spool?> GetActiveAsync() =>
        _db.Spools.FirstOrDefaultAsync(s => s.IsActive);

    public async Task<Spool> CreateAsync(Spool spool)
    {
        _db.Spools.Add(spool);
        await _db.SaveChangesAsync();
        return spool;
    }

    public async Task<Spool> UpdateAsync(Spool spool)
    {
        _db.Spools.Update(spool);
        await _db.SaveChangesAsync();
        return spool;
    }

    public async Task DeleteAsync(Guid id)
    {
        var spool = await _db.Spools.FindAsync(id);
        if (spool != null)
        {
            _db.Spools.Remove(spool);
            await _db.SaveChangesAsync();
        }
    }
}
```

### `PrintJobRepository.cs`

```csharp
// src/Spoolarr.Api/Repositories/PrintJobRepository.cs
public interface IPrintJobRepository
{
    Task<List<PrintJob>> GetBySpoolIdAsync(Guid spoolId);
    Task<PrintJob> CreateAsync(PrintJob job);
}

public class PrintJobRepository : IPrintJobRepository
{
    private readonly FilamentDbContext _db;
    public PrintJobRepository(FilamentDbContext db) => _db = db;

    public Task<List<PrintJob>> GetBySpoolIdAsync(Guid spoolId) =>
        _db.PrintJobs
            .Where(p => p.SpoolId == spoolId)
            .OrderByDescending(p => p.PrintedAt)
            .ToListAsync();

    public async Task<PrintJob> CreateAsync(PrintJob job)
    {
        _db.PrintJobs.Add(job);
        await _db.SaveChangesAsync();
        return job;
    }
}
```

### Register repositories in `Program.cs`

```csharp
builder.Services.AddScoped<ISpoolRepository, SpoolRepository>();
builder.Services.AddScoped<IPrintJobRepository, PrintJobRepository>();
```

### EF Core migration commands

```bash
cd src/Spoolarr.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Seed data (for testing only)

```csharp
// src/Spoolarr.Api/Data/SeedData.cs
public static class SeedData
{
    public static async Task InitialiseAsync(FilamentDbContext db)
    {
        if (db.Spools.Any()) return;

        var spools = new List<Spool>
        {
            new Spool
            {
                NfcTagUid = "04:A1:B2:C3",
                Brand = "Bambu Lab",
                Material = "PLA Basic",
                ColorName = "Bambu Green",
                ColorHex = "#00AE42",
                InitialWeightG = 1000f,
                CurrentWeightG = 740f,
            },
            new Spool
            {
                NfcTagUid = "04:D4:E5:F6",
                Brand = "Polymaker",
                Material = "PETG",
                ColorName = "Galaxy Black",
                ColorHex = "#1A1A2E",
                InitialWeightG = 1000f,
                CurrentWeightG = 320f,
            }
        };

        db.Spools.AddRange(spools);
        await db.SaveChangesAsync();
    }
}
```

```csharp
// Call in Program.cs after app.Build()
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FilamentDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.InitialiseAsync(db);
}
```

---

## How to Test

1. Run migrations: `dotnet ef database update`
2. Start the API: `dotnet run`
3. Open the SQLite file with any SQLite viewer (e.g. DB Browser for SQLite)
4. Confirm `Spools` and `PrintJobs` tables exist
5. Confirm seed data rows are present in `Spools`

---

## Definition of Done

- [ ] `Spools` and `PrintJobs` tables created by migration
- [ ] `NfcTagUid` has a unique index
- [ ] Both repositories registered in DI
- [ ] Seed data inserts 2 test spools on first run
- [ ] `dotnet ef migrations list` shows `InitialCreate` as applied
