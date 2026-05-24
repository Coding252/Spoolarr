# Milestone 7 — AMS Multi-Slot Support

> Extend Spoolarr to support the Bambu Lab AMS (Automatic Material System) — track up to 4 filament slots simultaneously and map each MQTT print event to the correct spool.

---

## Goal

By the end of this milestone, each AMS slot is mapped to a registered spool. When a print finishes, the gram deduction goes to the correct spool based on which slot was used — not just the single "active" spool.

---

## Depends On

- Milestone 1 — Data Model
- Milestone 2 — Spool API
- Milestone 4 — Bambu MQTT

---

## Tasks

- [ ] Slot-to-spool mapping model
- [ ] Multi-slot MQTT parsing
- [ ] UI slot management

---

## What to Create

### `AmsSlot.cs` — new entity

```csharp
// src/Spoolarr.Api/Models/AmsSlot.cs
public class AmsSlot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int SlotIndex { get; set; }   // 0, 1, 2, 3
    public int AmsUnit { get; set; } = 0; // for multi-AMS setups
    public Guid? SpoolId { get; set; }
    public Spool? Spool { get; set; }
    public DateTime? LoadedAt { get; set; }
}
```

### Add to `FilamentDbContext.cs`

```csharp
public DbSet<AmsSlot> AmsSlots => Set<AmsSlot>();
```

### EF Core migration

```bash
dotnet ef migrations add AddAmsSlots
dotnet ef database update
```

### `AmsSlotRepository.cs`

```csharp
// src/Spoolarr.Api/Repositories/AmsSlotRepository.cs
public interface IAmsSlotRepository
{
    Task<List<AmsSlot>> GetAllAsync();
    Task<AmsSlot?> GetBySlotAsync(int unit, int slot);
    Task<AmsSlot> UpsertAsync(int unit, int slot, Guid? spoolId);
}

public class AmsSlotRepository : IAmsSlotRepository
{
    private readonly FilamentDbContext _db;
    public AmsSlotRepository(FilamentDbContext db) => _db = db;

    public Task<List<AmsSlot>> GetAllAsync() =>
        _db.AmsSlots.Include(s => s.Spool).ToListAsync();

    public Task<AmsSlot?> GetBySlotAsync(int unit, int slot) =>
        _db.AmsSlots
            .Include(s => s.Spool)
            .FirstOrDefaultAsync(s => s.AmsUnit == unit && s.SlotIndex == slot);

    public async Task<AmsSlot> UpsertAsync(int unit, int slot, Guid? spoolId)
    {
        var existing = await GetBySlotAsync(unit, slot);
        if (existing is null)
        {
            existing = new AmsSlot { AmsUnit = unit, SlotIndex = slot };
            _db.AmsSlots.Add(existing);
        }
        existing.SpoolId = spoolId;
        existing.LoadedAt = spoolId.HasValue ? DateTime.UtcNow : null;
        await _db.SaveChangesAsync();
        return existing;
    }
}
```

### `AmsController.cs` — manage slot assignments

```csharp
// src/Spoolarr.Api/Controllers/AmsController.cs
[ApiController]
[Route("api/ams")]
public class AmsController : ControllerBase
{
    private readonly IAmsSlotRepository _repo;
    public AmsController(IAmsSlotRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetSlots()
    {
        var slots = await _repo.GetAllAsync();
        return Ok(slots);
    }

    [HttpPut("{unit}/{slot}")]
    public async Task<IActionResult> AssignSpool(int unit, int slot, [FromBody] AssignSlotRequest request)
    {
        var result = await _repo.UpsertAsync(unit, slot, request.SpoolId);
        return Ok(result);
    }
}

public record AssignSlotRequest(Guid? SpoolId);
```

### Update `MqttListenerService.cs` — parse AMS slot from print event

```csharp
// Replace the single active-spool logic with slot-aware logic
private async Task DeductFromSlotAsync(int amsUnit, int slotIndex, float gramsUsed, string? printName)
{
    using var scope = _scopeFactory.CreateScope();
    var slotRepo = scope.ServiceProvider.GetRequiredService<IAmsSlotRepository>();
    var spoolRepo = scope.ServiceProvider.GetRequiredService<ISpoolRepository>();
    var printJobRepo = scope.ServiceProvider.GetRequiredService<IPrintJobRepository>();

    var slot = await slotRepo.GetBySlotAsync(amsUnit, slotIndex);
    if (slot?.SpoolId is null)
    {
        _logger.LogWarning("AMS unit {Unit} slot {Slot} has no spool assigned. Skipping.", amsUnit, slotIndex);
        return;
    }

    var spool = await spoolRepo.GetByIdAsync(slot.SpoolId.Value);
    if (spool is null) return;

    spool.CurrentWeightG = Math.Max(0, spool.CurrentWeightG - gramsUsed);
    await spoolRepo.UpdateAsync(spool);

    await printJobRepo.CreateAsync(new PrintJob
    {
        SpoolId = spool.Id,
        GramsUsed = gramsUsed,
        PrintName = printName,
        Source = "mqtt"
    });

    _logger.LogInformation(
        "AMS unit {Unit} slot {Slot}: deducted {Grams}g. Remaining: {Remaining}g",
        amsUnit, slotIndex, gramsUsed, spool.CurrentWeightG);
}
```

### Bambu AMS MQTT payload reference

When using AMS, the print event includes slot information:

```json
{
  "print": {
    "gcode_state": "FINISH",
    "ams": {
      "ams": [
        {
          "id": "0",
          "tray": [
            { "id": "0", "remain": 87 },
            { "id": "1", "remain": 45 },
            { "id": "2", "remain": 100 },
            { "id": "3", "remain": 12 }
          ]
        }
      ]
    },
    "ams_rfid_status": 3,
    "filament_weight": 8.4
  }
}
```

Key fields:

| Field | Description |
|---|---|
| `ams[n].tray[n].id` | Slot index (0–3) |
| `ams[n].tray[n].remain` | Remaining % (Bambu's own estimate) |
| `filament_weight` | Grams used in this print |

> Note: Bambu reports `remain` as a percentage, not grams. Spoolarr ignores this and uses its own gram tracking from `filament_weight` deductions for accuracy.

---

## New API endpoints

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/ams` | List all AMS slots and assigned spools |
| `PUT` | `/api/ams/{unit}/{slot}` | Assign a spool to a slot |

---

## UI changes (Milestone 5 extension)

- Add an **AMS panel** to the dashboard showing 4 slot cards
- Each slot shows the assigned spool color, material, and remaining grams
- Drag or select to assign a spool to a slot
- Unassigned slots show "Empty" in grey

---

## How to Test

```bash
# Assign spool to AMS unit 0, slot 1
curl -X PUT http://localhost:5000/api/ams/0/1 \
  -H "Content-Type: application/json" \
  -d '{ "spoolId": "your-spool-guid-here" }'

# Get all slot assignments
curl http://localhost:5000/api/ams
```

Then finish a multi-colour AMS print and verify the correct spool's weight was deducted.

---

## Definition of Done

- [ ] `AmsSlots` table created by migration
- [ ] Spools can be assigned to slots via `PUT /api/ams/{unit}/{slot}`
- [ ] Print-finish event deducts from the correct slot's spool
- [ ] Warning logged when slot has no spool assigned
- [ ] Dashboard shows AMS slot panel with live spool data
