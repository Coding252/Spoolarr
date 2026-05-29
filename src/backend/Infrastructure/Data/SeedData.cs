using Domain.Models;

namespace Infrastructure.Data;

public static class SeedData
{
    public static async Task InitialiseAsync(FilamentDbContext db)
    {
        if (db.Spools.Any())
            return;

        var spoolPla = new Spool
        {
            Id = Guid.NewGuid(),
            Brand = "Bambu Lab",
            Material = "PLA",
            ColorName = "Jade White",
            ColorHex = "#FFFFFF",
            InitialWeightG = 1000,
            CurrentWeightG = 1000,
            SpoolWeightG = 200,
            DiameterMm = 1.75f,
            LowStockThresholdG = 100,
            IsActive = true,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var spoolPetg = new Spool
        {
            Id = Guid.NewGuid(),
            Brand = "Polymaker",
            Material = "PETG",
            ColorName = "Galaxy Black",
            ColorHex = "#1A1A2E",
            InitialWeightG = 1000,
            CurrentWeightG = 750,
            SpoolWeightG = 200,
            DiameterMm = 1.75f,
            LowStockThresholdG = 100,
            IsActive = false,
            IsArchived = false,
            CreatedAt = DateTime.UtcNow
        };

        var nfcTag = new NfcTag
        {
            Id = Guid.NewGuid(),
            TagUid = "04:A1:B2:C3:D4:E5:F6",
            Type = "ntag215",
            SpoolId = spoolPla.Id,
            CreatedAt = DateTime.UtcNow
        };

        var printer = new Printer
        {
            Id = Guid.NewGuid(),
            Name = "Garage X1C",
            Brand = "Bambu Lab",
            Model = "X1 Carbon",
            IpAddress = "192.168.1.100",
            Protocol = "bambu-mqtt",
            HasAms = true,
            AmsSlotCount = 4,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Spools.AddRange(spoolPla, spoolPetg);
        db.NfcTags.Add(nfcTag);
        db.Printers.Add(printer);
        await db.SaveChangesAsync();
    }
}
