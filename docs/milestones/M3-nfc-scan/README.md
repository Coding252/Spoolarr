# Milestone 3 — NFC Scan Flow

> Handle the NFC tag scan event — look up the tag UID, route to register or activate, and push the result to the browser in real time via SignalR.

---

## Goal

By the end of this milestone, tapping a phone to a spool triggers a scan event that the API processes and instantly pushes back to the browser — no page refresh needed. Unknown tags prompt registration. Known tags activate the spool.

---

## Depends On

- Milestone 0 — Project Bootstrap
- Milestone 1 — Data Model
- Milestone 2 — Spool API

---

## Tasks

- [ ] `POST /api/spools/scan` — receives tag UID, routes to register or activate
- [ ] `NfcScanService` — lookup by UID, return spool or "unknown tag"
- [ ] `NfcScanHub` SignalR — push scan result to browser in real time

---

## What to Create

### Install SignalR NuGet package

```bash
cd src/Spoolarr.Api
dotnet add package Microsoft.AspNetCore.SignalR
```

### `NfcScanResult.cs` — scan outcome model

```csharp
// src/Spoolarr.Api/Models/NfcScanResult.cs
public record NfcScanResult(
    string Status,        // "activated" | "unknown" | "error"
    string TagUid,
    SpoolResponse? Spool, // null if unknown tag
    string? Message
);
```

### `NfcScanService.cs`

```csharp
// src/Spoolarr.Api/Services/NfcScanService.cs
public interface INfcScanService
{
    Task<NfcScanResult> ProcessScanAsync(string tagUid);
}

public class NfcScanService : INfcScanService
{
    private readonly ISpoolRepository _repo;
    private readonly ISpoolService _spoolService;

    public NfcScanService(ISpoolRepository repo, ISpoolService spoolService)
    {
        _repo = repo;
        _spoolService = spoolService;
    }

    public async Task<NfcScanResult> ProcessScanAsync(string tagUid)
    {
        var spool = await _repo.GetByNfcTagUidAsync(tagUid);

        if (spool is null)
        {
            // Unknown tag — tell the UI to show the register form
            return new NfcScanResult(
                Status: "unknown",
                TagUid: tagUid,
                Spool: null,
                Message: "Tag not registered. Please fill in spool details."
            );
        }

        // Known tag — activate the spool
        var activated = await _spoolService.ActivateAsync(spool.Id);

        return new NfcScanResult(
            Status: "activated",
            TagUid: tagUid,
            Spool: activated,
            Message: $"{spool.Brand} {spool.Material} ({spool.ColorName}) is now active."
        );
    }
}
```

### `NfcScanHub.cs` — SignalR hub

```csharp
// src/Spoolarr.Api/Hubs/NfcScanHub.cs
public class NfcScanHub : Hub
{
    // Client connects and waits for scan events pushed from server
    // No client-to-server methods needed here
}
```

### `ScanController.cs`

```csharp
// src/Spoolarr.Api/Controllers/ScanController.cs
[ApiController]
[Route("api/spools")]
public class ScanController : ControllerBase
{
    private readonly INfcScanService _scanService;
    private readonly IHubContext<NfcScanHub> _hub;

    public ScanController(INfcScanService scanService, IHubContext<NfcScanHub> hub)
    {
        _scanService = scanService;
        _hub = hub;
    }

    [HttpPost("scan")]
    public async Task<IActionResult> Scan([FromBody] ScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TagUid))
            return BadRequest("TagUid is required.");

        var result = await _scanService.ProcessScanAsync(request.TagUid);

        // Push result to all connected browser clients via SignalR
        await _hub.Clients.All.SendAsync("ScanResult", result);

        return Ok(result);
    }
}

public record ScanRequest(string TagUid);
```

### Register SignalR + services in `Program.cs`

```csharp
builder.Services.AddSignalR();
builder.Services.AddScoped<INfcScanService, NfcScanService>();

// ...

app.MapHub<NfcScanHub>("/hubs/nfc");
```

### Browser-side SignalR listener (JavaScript snippet for Milestone 5)

```javascript
// Connect to SignalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/nfc")
    .build();

// Listen for scan results pushed from server
connection.on("ScanResult", (result) => {
    if (result.status === "activated") {
        showActivatedBanner(result.spool);
    } else if (result.status === "unknown") {
        showRegisterForm(result.tagUid);
    }
});

connection.start();
```

---

## Scan Flow Summary

```
Phone taps spool NFC tag
        │
        ▼
Browser reads tag UID via Web NFC API
        │
        ▼
POST /api/spools/scan  { tagUid: "04:AA:BB:CC" }
        │
        ▼
NfcScanService.ProcessScanAsync(tagUid)
        │
   ┌────┴────┐
Known?      Unknown?
   │             │
Activate     Return "unknown"
spool            │
   │         Show register form
   └────┬────┘
        ▼
SignalR push → ScanResult → Browser
```

---

## How to Test

```bash
# Simulate a scan of a known tag (use a seed spool UID)
curl -X POST http://localhost:5000/api/spools/scan \
  -H "Content-Type: application/json" \
  -d '{ "tagUid": "04:A1:B2:C3" }'

# Expected response for known tag:
# { "status": "activated", "tagUid": "04:A1:B2:C3", "spool": { ... } }

# Simulate a scan of an unknown tag
curl -X POST http://localhost:5000/api/spools/scan \
  -H "Content-Type: application/json" \
  -d '{ "tagUid": "04:FF:FF:FF" }'

# Expected response for unknown tag:
# { "status": "unknown", "tagUid": "04:FF:FF:FF", "spool": null }
```

---

## Definition of Done

- [ ] `POST /api/spools/scan` with known UID activates spool and returns `"activated"`
- [ ] `POST /api/spools/scan` with unknown UID returns `"unknown"` and no spool
- [ ] SignalR hub is registered and accessible at `/hubs/nfc`
- [ ] SignalR `ScanResult` event fires on every scan
- [ ] Scan endpoint tested with curl for both known and unknown UIDs
