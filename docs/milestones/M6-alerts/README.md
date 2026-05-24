# Milestone 6 — Low Stock Alerts

> Trigger a push notification when a spool's remaining weight drops below its threshold after a print.

---

## Goal

By the end of this milestone, whenever a print finishes and a spool drops below its `lowStockThresholdG`, a notification is sent via ntfy or webhook so you know to restock before the next print.

---

## Depends On

- Milestone 4 — Bambu MQTT (gram deduction happens here)

---

## Tasks

- [ ] Threshold check after every gram deduction
- [ ] Webhook / ntfy push notification

---

## What to Create

### `AlertSettings.cs`

```csharp
// src/Spoolarr.Api/Settings/AlertSettings.cs
public class AlertSettings
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "ntfy"; // "ntfy" or "webhook"
    public string NtfyUrl { get; set; } = "https://ntfy.sh/spoolarr-alerts";
    public string WebhookUrl { get; set; } = string.Empty;
}
```

### `appsettings.json` — add alert settings

```json
{
  "Alerts": {
    "Enabled": true,
    "Provider": "ntfy",
    "NtfyUrl": "https://ntfy.sh/your-topic-here"
  }
}
```

### `AlertService.cs`

```csharp
// src/Spoolarr.Api/Services/AlertService.cs
public interface IAlertService
{
    Task CheckAndAlertAsync(Spool spool);
}

public class AlertService : IAlertService
{
    private readonly AlertSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AlertService> _logger;

    public AlertService(
        IOptions<AlertSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<AlertService> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task CheckAndAlertAsync(Spool spool)
    {
        if (!_settings.Enabled) return;
        if (spool.CurrentWeightG > spool.LowStockThresholdG) return;

        var message = $"⚠️ Low filament on {spool.Brand} {spool.Material} " +
                      $"({spool.ColorName}) — {spool.CurrentWeightG}g remaining.";

        _logger.LogWarning("Low stock alert: {Message}", message);

        await (  _settings.Provider == "ntfy"
            ? SendNtfyAsync(message, spool)
            : SendWebhookAsync(message));
    }

    private async Task SendNtfyAsync(string message, Spool spool)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, _settings.NtfyUrl)
        {
            Content = new StringContent(message)
        };
        request.Headers.Add("Title", "Spoolarr — Low Filament");
        request.Headers.Add("Priority", "high");
        request.Headers.Add("Tags", "spool,filament,warning");

        await client.SendAsync(request);
    }

    private async Task SendWebhookAsync(string message)
    {
        var client = _httpClientFactory.CreateClient();
        var payload = JsonSerializer.Serialize(new { text = message });
        await client.PostAsync(
            _settings.WebhookUrl,
            new StringContent(payload, Encoding.UTF8, "application/json"));
    }
}
```

### Update `MqttListenerService.cs` — add alert check after deduction

```csharp
// Inside DeductFromActiveSpoolAsync, after saving:
await spoolRepo.UpdateAsync(activeSpool);

// Check and fire alert if below threshold
var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
await alertService.CheckAndAlertAsync(activeSpool);
```

### Register in `Program.cs`

```csharp
builder.Services.Configure<AlertSettings>(
    builder.Configuration.GetSection("Alerts"));

builder.Services.AddHttpClient();
builder.Services.AddScoped<IAlertService, AlertService>();
```

---

## ntfy Setup (self-hosted or cloud)

ntfy is the easiest way to get push notifications on your phone for free.

**Option A — Use ntfy.sh (cloud, free):**
1. Pick a unique topic name e.g. `spoolarr-yourname-abc123`
2. Set `NtfyUrl` to `https://ntfy.sh/spoolarr-yourname-abc123`
3. Install the ntfy app on your phone and subscribe to that topic

**Option B — Self-host ntfy in Docker:**
```yaml
# Add to docker-compose.yml
  ntfy:
    image: binwiederhier/ntfy
    command: serve
    ports:
      - "8080:80"
    volumes:
      - ntfy-data:/var/lib/ntfy
```
Then set `NtfyUrl` to `http://ntfy:80/spoolarr-alerts`

---

## How to Test

1. Set a spool's `LowStockThresholdG` to `500`
2. Set its `CurrentWeightG` to `510`
3. Simulate a print that uses 20g (via MQTT or direct DB update)
4. Confirm alert notification arrives on your phone
5. Confirm log shows `"Low stock alert: ..."`

**Direct test without MQTT:**
```bash
# Manually update weight below threshold to trigger next check
curl -X PATCH http://localhost:5000/api/spools/{id}/weight \
  -H "Content-Type: application/json" \
  -d '{ "newWeightG": 80 }'
```

---

## Definition of Done

- [ ] Alert fires when `currentWeightG` drops below `lowStockThresholdG`
- [ ] No alert fires when weight is above threshold
- [ ] ntfy notification received on phone
- [ ] Webhook fires correctly when provider set to `"webhook"`
- [ ] Alerts can be disabled via `"Enabled": false` in config
