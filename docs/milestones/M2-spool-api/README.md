# Milestone 2 — Spool API

> Build the REST API endpoints for managing spools — list, get, register, activate, and update weight.

---

## Goal

By the end of this milestone you have a fully working REST API for spool management that can be tested with Postman or curl. No UI yet — just the backend endpoints that the frontend will call later.

---

## Depends On

- Milestone 0 — Project Bootstrap
- Milestone 1 — Data Model

---

## Tasks

- [ ] `GET /api/spools` — list all spools
- [ ] `GET /api/spools/{id}` — get single spool
- [ ] `POST /api/spools` — register new spool
- [ ] `PATCH /api/spools/{id}/activate` — set active spool
- [ ] `PATCH /api/spools/{id}/weight` — update weight manually

---

## What to Create

### `SpoolDto.cs` — request / response shapes

```csharp
// src/Spoolarr.Api/DTOs/SpoolDto.cs

public record SpoolResponse(
    Guid Id,
    string NfcTagUid,
    string Brand,
    string Material,
    string ColorName,
    string ColorHex,
    float InitialWeightG,
    float CurrentWeightG,
    float LowStockThresholdG,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastScannedAt,
    string? Notes
);

public record RegisterSpoolRequest(
    string NfcTagUid,
    string Brand,
    string Material,
    string ColorName,
    string ColorHex,
    float InitialWeightG,
    float? LowStockThresholdG,
    string? Notes
);

public record UpdateWeightRequest(float NewWeightG);
```

### `SpoolService.cs`

```csharp
// src/Spoolarr.Api/Services/SpoolService.cs
public interface ISpoolService
{
    Task<List<SpoolResponse>> GetAllAsync();
    Task<SpoolResponse?> GetByIdAsync(Guid id);
    Task<SpoolResponse> RegisterAsync(RegisterSpoolRequest request);
    Task<SpoolResponse?> ActivateAsync(Guid id);
    Task<SpoolResponse?> UpdateWeightAsync(Guid id, float newWeightG);
}

public class SpoolService : ISpoolService
{
    private readonly ISpoolRepository _repo;
    public SpoolService(ISpoolRepository repo) => _repo = repo;

    public async Task<List<SpoolResponse>> GetAllAsync()
    {
        var spools = await _repo.GetAllAsync();
        return spools.Select(ToResponse).ToList();
    }

    public async Task<SpoolResponse?> GetByIdAsync(Guid id)
    {
        var spool = await _repo.GetByIdAsync(id);
        return spool is null ? null : ToResponse(spool);
    }

    public async Task<SpoolResponse> RegisterAsync(RegisterSpoolRequest req)
    {
        var spool = new Spool
        {
            NfcTagUid = req.NfcTagUid,
            Brand = req.Brand,
            Material = req.Material,
            ColorName = req.ColorName,
            ColorHex = req.ColorHex,
            InitialWeightG = req.InitialWeightG,
            CurrentWeightG = req.InitialWeightG,
            LowStockThresholdG = req.LowStockThresholdG ?? 100f,
            Notes = req.Notes
        };
        var created = await _repo.CreateAsync(spool);
        return ToResponse(created);
    }

    public async Task<SpoolResponse?> ActivateAsync(Guid id)
    {
        // Deactivate current active spool
        var current = await _repo.GetActiveAsync();
        if (current != null)
        {
            current.IsActive = false;
            await _repo.UpdateAsync(current);
        }

        var spool = await _repo.GetByIdAsync(id);
        if (spool is null) return null;

        spool.IsActive = true;
        spool.LastScannedAt = DateTime.UtcNow;
        var updated = await _repo.UpdateAsync(spool);
        return ToResponse(updated);
    }

    public async Task<SpoolResponse?> UpdateWeightAsync(Guid id, float newWeightG)
    {
        var spool = await _repo.GetByIdAsync(id);
        if (spool is null) return null;

        spool.CurrentWeightG = newWeightG;
        var updated = await _repo.UpdateAsync(spool);
        return ToResponse(updated);
    }

    private static SpoolResponse ToResponse(Spool s) => new(
        s.Id, s.NfcTagUid, s.Brand, s.Material,
        s.ColorName, s.ColorHex, s.InitialWeightG,
        s.CurrentWeightG, s.LowStockThresholdG,
        s.IsActive, s.CreatedAt, s.LastScannedAt, s.Notes
    );
}
```

### `SpoolController.cs`

```csharp
// src/Spoolarr.Api/Controllers/SpoolController.cs
[ApiController]
[Route("api/spools")]
public class SpoolController : ControllerBase
{
    private readonly ISpoolService _service;
    public SpoolController(ISpoolService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var spools = await _service.GetAllAsync();
        return Ok(spools);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var spool = await _service.GetByIdAsync(id);
        return spool is null ? NotFound() : Ok(spool);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterSpoolRequest request)
    {
        var spool = await _service.RegisterAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = spool.Id }, spool);
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var spool = await _service.ActivateAsync(id);
        return spool is null ? NotFound() : Ok(spool);
    }

    [HttpPatch("{id:guid}/weight")]
    public async Task<IActionResult> UpdateWeight(Guid id, [FromBody] UpdateWeightRequest request)
    {
        var spool = await _service.UpdateWeightAsync(id, request.NewWeightG);
        return spool is null ? NotFound() : Ok(spool);
    }
}
```

### Register service in `Program.cs`

```csharp
builder.Services.AddScoped<ISpoolService, SpoolService>();
```

---

## API Reference

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/spools` | List all spools |
| `GET` | `/api/spools/{id}` | Get spool by ID |
| `POST` | `/api/spools` | Register new spool |
| `PATCH` | `/api/spools/{id}/activate` | Set spool as active |
| `PATCH` | `/api/spools/{id}/weight` | Manually update weight |

---

## How to Test

Use curl or Postman:

```bash
# List all spools
curl http://localhost:5000/api/spools

# Register a new spool
curl -X POST http://localhost:5000/api/spools \
  -H "Content-Type: application/json" \
  -d '{
    "nfcTagUid": "04:AA:BB:CC",
    "brand": "Polymaker",
    "material": "PLA",
    "colorName": "Matte White",
    "colorHex": "#F5F5F5",
    "initialWeightG": 1000
  }'

# Activate a spool (replace with real ID)
curl -X PATCH http://localhost:5000/api/spools/{id}/activate

# Update weight manually
curl -X PATCH http://localhost:5000/api/spools/{id}/weight \
  -H "Content-Type: application/json" \
  -d '{ "newWeightG": 850 }'
```

---

## Definition of Done

- [ ] All 5 endpoints return correct HTTP status codes
- [ ] `POST /api/spools` returns `201 Created` with location header
- [ ] Activating a spool deactivates the previously active one
- [ ] `GET /api/spools` returns seed spools from Milestone 1
- [ ] All endpoints tested with Postman or curl
