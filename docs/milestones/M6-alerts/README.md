# Milestone 6 — Low Stock Alerts

> Trigger a push notification when a spool's remaining weight drops below its threshold after a print.

---

## Goal

By the end of this milestone whenever a print finishes and a spool drops below its `LowStockThresholdG`, a notification is sent to your phone via ntfy or a webhook so you know to restock before the next print.

---

## Depends On

- Milestone 4 — Bambu MQTT

---

## Tasks

### NuGet packages
- [ ] Confirm `HttpClient` is available via `IHttpClientFactory` — register if not already done

### Settings
- [ ] Create `AlertSettings` class inside `Infrastructure/Settings/`
- [ ] Add `Enabled` field — bool, default `true`
- [ ] Add `Provider` field — string, values: `"ntfy"` or `"webhook"`
- [ ] Add `NtfyUrl` field — string
- [ ] Add `WebhookUrl` field — string
- [ ] Add `Alerts` section to `appsettings.json` in `API` with placeholder values
- [ ] Register `AlertSettings` in `Program.cs` via `Configure<AlertSettings>`

### AlertService
- [ ] Create `IAlertService` interface inside `Application/Interfaces/`
- [ ] Create `AlertService` class inside `Application/Services/` implementing `IAlertService`
- [ ] Add `CheckAndAlertAsync` — takes a `Spool`, checks threshold, sends alert if needed
- [ ] Skip if `Enabled` is false in settings
- [ ] Skip if `CurrentWeightG` is above `LowStockThresholdG`
- [ ] Build alert message with brand, material, color name, and remaining grams
- [ ] Route to ntfy sender if `Provider` is `"ntfy"`
- [ ] Route to webhook sender if `Provider` is `"webhook"`
- [ ] Add ntfy sender — POST message to `NtfyUrl` with title and priority headers
- [ ] Add webhook sender — POST JSON payload to `WebhookUrl`
- [ ] Log warning when alert is fired
- [ ] Register `IAlertService` → `AlertService` in `Program.cs`
- [ ] Register `IHttpClientFactory` in `Program.cs` via `builder.Services.AddHttpClient()`

### Wire up to MQTT service
- [ ] Call `AlertService.CheckAndAlertAsync` inside `MqttListenerService` after every gram deduction
- [ ] Resolve `IAlertService` from the service scope inside the hosted service

### Manual test endpoint
- [ ] Add `POST /api/spools/{id}/test-alert` endpoint for triggering an alert manually
- [ ] Only available in `Development` environment
- [ ] Useful for testing without waiting for a real print to finish

### ntfy setup (optional, self-hosted)
- [ ] Add `ntfy` service to `docker-compose.yml`
- [ ] Configure ntfy to run on the local network
- [ ] Update `NtfyUrl` in settings to point to self-hosted instance

---

## Alert message format

The notification should include:
- Spool brand and material
- Color name
- Remaining grams
- Example: `⚠️ Low filament on Bambu Lab PLA (Bambu Green) — 85g remaining`

---

## Definition of Done

- [ ] Alert fires when `CurrentWeightG` drops below `LowStockThresholdG` after a print
- [ ] No alert fires when weight is still above threshold
- [ ] ntfy notification received on phone when provider is `"ntfy"`
- [ ] Webhook fires correctly when provider is `"webhook"`
- [ ] Alerts do not fire when `Enabled` is set to `false`
- [ ] Warning is logged every time an alert is sent
- [ ] Manual test endpoint triggers alert without a real print
- [ ] Alert does not fire twice for the same print event
