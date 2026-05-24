# Milestone 4 — Bambu Lab MQTT Integration

> Connect to the Bambu Lab printer on the local network via MQTT, listen for print-finish events, extract grams used, and automatically deduct from the active spool.

---

## Goal

By the end of this milestone, finishing a print on your Bambu printer automatically deducts the correct grams from the active spool and logs the print job to the database — no manual input required.

---

## Depends On

- Milestone 0 — Project Bootstrap
- Milestone 1 — Data Model
- Milestone 2 — Spool API

---

## Tasks

- [ ] `MqttListenerService` as `IHostedService`
- [ ] Connect to printer on LAN using MQTTnet
- [ ] Parse print-finish event, extract grams used
- [ ] Deduct grams from active spool via `SpoolService`
- [ ] Log `PrintJob` to DB

---

## What to Create

### Install MQTTnet NuGet package

```bash
cd src/Spoolarr.Api
dotnet add package MQTTnet
dotnet add package MQTTnet.Extensions.ManagedClient
```

### Bambu Lab MQTT connection details

| Setting | Value |
|---|---|
| Host | Your printer's LAN IP |
| Port | `8883` (TLS) |
| Username | `bblp` |
| Password | LAN access code (shown on printer screen) |
| Topic | `device/{serial}/report` |
| TLS | Required — skip cert validation on LAN |

### `BambuMqttSettings.cs`

```csharp
// src/Spoolarr.Api/Settings/BambuMqttSettings.cs
public class BambuMqttSettings
{
    public string PrinterIp { get; set; } = string.Empty;
    public int Port { get; set; } = 8883;
    public string Serial { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
}
```

### `appsettings.json` — add Bambu settings

```json
{
  "BambuMqtt": {
    "PrinterIp": "192.168.1.50",
    "Port": 8883,
    "Serial": "01S00C123456789",
    "AccessCode": "12345678"
  }
}
```

### `MqttListenerService.cs`

```csharp
// src/Spoolarr.Api/Services/MqttListenerService.cs
public class MqttListenerService : IHostedService
{
    private readonly BambuMqttSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MqttListenerService> _logger;
    private IMqttClient? _client;

    public MqttListenerService(
        IOptions<BambuMqttSettings> settings,
        IServiceScopeFactory scopeFactory,
        ILogger<MqttListenerService> logger)
    {
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(_settings.PrinterIp, _settings.Port)
            .WithCredentials("bblp", _settings.AccessCode)
            .WithTlsOptions(o => o.WithCertificateValidationHandler(_ => true))
            .WithClientId($"spoolarr-{Guid.NewGuid()}")
            .Build();

        _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;

        await _client.ConnectAsync(options, cancellationToken);

        var topic = $"device/{_settings.Serial}/report";
        await _client.SubscribeAsync(topic, cancellationToken: cancellationToken);

        _logger.LogInformation("Connected to Bambu printer at {Ip}", _settings.PrinterIp);
    }

    private async Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs e)
    {
        try
        {
            var payload = e.ApplicationMessage.ConvertPayloadToString();
            var doc = JsonDocument.Parse(payload);

            // Bambu sends print progress under "print" key
            if (!doc.RootElement.TryGetProperty("print", out var print)) return;

            // Only process when print is finished
            if (!print.TryGetProperty("gcode_state", out var state)) return;
            if (state.GetString() != "FINISH") return;

            // Extract grams used
            if (!print.TryGetProperty("mc_remaining_time", out _)) return;
            if (!print.TryGetProperty("filament_weight", out var weightEl)) return;

            var gramsUsed = weightEl.GetSingle();
            if (gramsUsed <= 0) return;

            await DeductFromActiveSpoolAsync(gramsUsed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MQTT message");
        }
    }

    private async Task DeductFromActiveSpoolAsync(float gramsUsed)
    {
        using var scope = _scopeFactory.CreateScope();
        var spoolRepo = scope.ServiceProvider.GetRequiredService<ISpoolRepository>();
        var printJobRepo = scope.ServiceProvider.GetRequiredService<IPrintJobRepository>();

        var activeSpool = await spoolRepo.GetActiveAsync();
        if (activeSpool is null)
        {
            _logger.LogWarning("Print finished but no active spool set. Skipping deduction.");
            return;
        }

        activeSpool.CurrentWeightG = Math.Max(0, activeSpool.CurrentWeightG - gramsUsed);
        await spoolRepo.UpdateAsync(activeSpool);

        var printJob = new PrintJob
        {
            SpoolId = activeSpool.Id,
            GramsUsed = gramsUsed,
            Source = "mqtt"
        };
        await printJobRepo.CreateAsync(printJob);

        _logger.LogInformation(
            "Deducted {Grams}g from spool {Id}. Remaining: {Remaining}g",
            gramsUsed, activeSpool.Id, activeSpool.CurrentWeightG);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client?.IsConnected == true)
            await _client.DisconnectAsync(cancellationToken: cancellationToken);
    }
}
```

### Register in `Program.cs`

```csharp
builder.Services.Configure<BambuMqttSettings>(
    builder.Configuration.GetSection("BambuMqtt"));

builder.Services.AddHostedService<MqttListenerService>();
```

---

## Bambu MQTT Message Reference

The printer sends JSON payloads on `device/{serial}/report`. A print-finish event looks like:

```json
{
  "print": {
    "gcode_state": "FINISH",
    "filament_weight": 12.4,
    "mc_remaining_time": 0,
    "subtask_name": "my_model.3mf"
  }
}
```

Key fields:

| Field | Description |
|---|---|
| `gcode_state` | `"FINISH"` when print is complete |
| `filament_weight` | Grams of filament used in this print |
| `subtask_name` | Name of the print file |

---

## How to Test

1. Start the API with valid `BambuMqtt` settings in `appsettings.json`
2. Check logs for `"Connected to Bambu printer at {Ip}"`
3. Start and finish a print on the Bambu printer
4. Check logs for `"Deducted {Grams}g from spool"`
5. Call `GET /api/spools/{id}` and confirm `currentWeightG` has decreased

**Without a physical printer** — use MQTT Explorer or mosquitto to publish a fake message:

```bash
mosquitto_pub \
  -h 192.168.1.50 -p 8883 \
  -u bblp -P 12345678 \
  -t "device/01S00C123456789/report" \
  -m '{"print":{"gcode_state":"FINISH","filament_weight":15.2}}'
```

---

## Definition of Done

- [ ] Service starts and connects to printer on API boot
- [ ] Print-finish event is detected from MQTT
- [ ] Grams are correctly deducted from active spool
- [ ] `PrintJob` row is created in DB after each print
- [ ] Warning is logged if no active spool is set
- [ ] `CurrentWeightG` never goes below 0
