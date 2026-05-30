using System.Text.Json;
using Application.DTOs;
using Application.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class MqttMessageProcessor(
    ISpoolRepository spoolRepository,
    IPrintJobRepository printJobRepository,
    IPrinterStatusService statusService,
    IPrinterStatusPusher statusPusher,
    ILogger<MqttMessageProcessor> logger) : IMqttMessageProcessor
{
    public async Task ProcessAsync(string payload, Guid printerId)
    {
        if (string.IsNullOrEmpty(payload)) return;

        using var doc = JsonDocument.Parse(payload);
        if (!doc.RootElement.TryGetProperty("print", out var print)) return;
        if (!print.TryGetProperty("gcode_state", out var stateEl)) return;

        var state = stateEl.GetString() ?? string.Empty;
        var fileName = print.TryGetProperty("subtask_name", out var nameEl) ? nameEl.GetString() : null;

        var status = new PrinterStatus(
            GcodeState: state,
            ProgressPercent: print.TryGetProperty("mc_percent", out var pct) ? pct.GetInt32() : 0,
            RemainingMinutes: print.TryGetProperty("mc_remaining_time", out var rem) ? rem.GetInt32() : 0,
            SubtaskName: fileName,
            LayerNum: print.TryGetProperty("layer_num", out var layer) ? layer.GetInt32() : 0,
            TotalLayerNum: print.TryGetProperty("total_layer_num", out var totalLayer) ? totalLayer.GetInt32() : 0,
            NozzleTempC: print.TryGetProperty("nozzle_temper", out var nozzle) ? nozzle.GetSingle() : 0,
            BedTempC: print.TryGetProperty("bed_temper", out var bed) ? bed.GetSingle() : 0,
            UpdatedAt: DateTime.UtcNow);

        statusService.UpdateStatus(status);
        await statusPusher.PushAsync(status);

        if (state != "FINISH") return;

        if (!print.TryGetProperty("filament_weight", out var weightEl)) return;
        var grams = weightEl.GetSingle();
        if (grams <= 0) return;

        logger.LogInformation("Print finished — {Grams}g used, file: {File}", grams, fileName ?? "unknown");
        await DeductFromActiveSpoolAsync(grams, fileName, printerId);
    }

    private async Task DeductFromActiveSpoolAsync(float grams, string? fileName, Guid printerId)
    {
        var spool = await spoolRepository.GetActiveAsync();
        if (spool is null)
        {
            logger.LogWarning("Print finished but no active spool is set — skipping deduction");
            return;
        }

        spool.CurrentWeightG = Math.Max(0, spool.CurrentWeightG - grams);
        await spoolRepository.UpdateAsync(spool);
        logger.LogInformation("Deducted {Grams}g from spool {SpoolId}. Remaining: {Weight}g",
            grams, spool.Id, spool.CurrentWeightG);

        var now = DateTime.UtcNow;
        await printJobRepository.CreateAsync(new PrintJob
        {
            Id = Guid.NewGuid(),
            SpoolId = spool.Id,
            PrinterId = printerId,
            GramsUsed = grams,
            PrintFileName = fileName,
            Status = "finished",
            Source = "mqtt",
            StartedAt = now,
            FinishedAt = now,
            LastUpdatedAt = now
        });
    }
}
