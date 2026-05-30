using Application.Interfaces;
using Application.Services;
using Domain.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Test;

public class MqttMessageProcessorTests
{
    private readonly ISpoolRepository _spoolRepo = Substitute.For<ISpoolRepository>();
    private readonly IPrintJobRepository _printJobRepo = Substitute.For<IPrintJobRepository>();
    private readonly IPrinterStatusService _statusService = Substitute.For<IPrinterStatusService>();
    private readonly IPrinterStatusPusher _statusPusher = Substitute.For<IPrinterStatusPusher>();
    private readonly MqttMessageProcessor _sut;

    public MqttMessageProcessorTests() =>
        _sut = new MqttMessageProcessor(
            _spoolRepo, _printJobRepo, _statusService, _statusPusher,
            NullLogger<MqttMessageProcessor>.Instance);

    [Fact]
    public async Task ProcessAsync_WhenPayloadEmpty_DoesNotUpdateStatus()
    {
        await _sut.ProcessAsync(string.Empty, Guid.NewGuid());

        _statusService.DidNotReceive().UpdateStatus(Arg.Any<Application.DTOs.PrinterStatus>());
    }

    [Fact]
    public async Task ProcessAsync_WhenNoPrintProperty_DoesNotUpdateStatus()
    {
        await _sut.ProcessAsync("{\"other\":{}}", Guid.NewGuid());

        _statusService.DidNotReceive().UpdateStatus(Arg.Any<Application.DTOs.PrinterStatus>());
    }

    [Fact]
    public async Task ProcessAsync_WhenNoGcodeState_DoesNotUpdateStatus()
    {
        await _sut.ProcessAsync("{\"print\":{\"mc_percent\":50}}", Guid.NewGuid());

        _statusService.DidNotReceive().UpdateStatus(Arg.Any<Application.DTOs.PrinterStatus>());
    }

    [Fact]
    public async Task ProcessAsync_UpdatesStatusForEveryMessage()
    {
        await _sut.ProcessAsync(RunningPayload(), Guid.NewGuid());

        _statusService.Received(1).UpdateStatus(Arg.Is<Application.DTOs.PrinterStatus>(s => s.GcodeState == "RUNNING"));
    }

    [Fact]
    public async Task ProcessAsync_PushesStatusForEveryMessage()
    {
        await _sut.ProcessAsync(RunningPayload(), Guid.NewGuid());

        await _statusPusher.Received(1).PushAsync(Arg.Any<Application.DTOs.PrinterStatus>());
    }

    [Fact]
    public async Task ProcessAsync_WhenStateIsRunning_DoesNotDeductFromSpool()
    {
        await _sut.ProcessAsync(RunningPayload(), Guid.NewGuid());

        await _spoolRepo.DidNotReceive().GetActiveAsync();
    }

    [Fact]
    public async Task ProcessAsync_WhenFinish_DeductsCorrectGramsFromSpool()
    {
        var spool = BuildSpool(weight: 500);
        _spoolRepo.GetActiveAsync().Returns(spool);
        _spoolRepo.UpdateAsync(Arg.Any<Spool>()).Returns(x => x.Arg<Spool>());

        await _sut.ProcessAsync(FinishPayload(grams: 120), Guid.NewGuid());

        Assert.Equal(380, spool.CurrentWeightG);
    }

    [Fact]
    public async Task ProcessAsync_WhenFinish_FloorsWeightAtZero()
    {
        var spool = BuildSpool(weight: 50);
        _spoolRepo.GetActiveAsync().Returns(spool);
        _spoolRepo.UpdateAsync(Arg.Any<Spool>()).Returns(x => x.Arg<Spool>());

        await _sut.ProcessAsync(FinishPayload(grams: 200), Guid.NewGuid());

        Assert.Equal(0, spool.CurrentWeightG);
    }

    [Fact]
    public async Task ProcessAsync_WhenFinish_LogsPrintJob()
    {
        var printerId = Guid.NewGuid();
        var spool = BuildSpool(weight: 500);
        _spoolRepo.GetActiveAsync().Returns(spool);
        _spoolRepo.UpdateAsync(Arg.Any<Spool>()).Returns(x => x.Arg<Spool>());

        await _sut.ProcessAsync(FinishPayload(grams: 100), printerId);

        await _printJobRepo.Received(1).CreateAsync(Arg.Is<PrintJob>(j =>
            j.PrinterId == printerId &&
            j.SpoolId == spool.Id &&
            j.GramsUsed == 100 &&
            j.Status == "finished"));
    }

    [Fact]
    public async Task ProcessAsync_WhenFinish_NoActiveSpool_SkipsDeduction()
    {
        _spoolRepo.GetActiveAsync().Returns((Spool?)null);

        await _sut.ProcessAsync(FinishPayload(grams: 100), Guid.NewGuid());

        await _spoolRepo.DidNotReceive().UpdateAsync(Arg.Any<Spool>());
        await _printJobRepo.DidNotReceive().CreateAsync(Arg.Any<PrintJob>());
    }

    [Fact]
    public async Task ProcessAsync_WhenFinish_NoFilamentWeight_SkipsDeduction()
    {
        var payload = "{\"print\":{\"gcode_state\":\"FINISH\"}}";

        await _sut.ProcessAsync(payload, Guid.NewGuid());

        await _spoolRepo.DidNotReceive().GetActiveAsync();
    }

    private static string RunningPayload() =>
        "{\"print\":{\"gcode_state\":\"RUNNING\",\"mc_percent\":42,\"mc_remaining_time\":60}}";

    private static string FinishPayload(float grams) =>
        $"{{\"print\":{{\"gcode_state\":\"FINISH\",\"filament_weight\":{grams},\"subtask_name\":\"test.gcode\"}}}}";

    private static Spool BuildSpool(float weight = 500) => new()
    {
        Id = Guid.NewGuid(),
        Brand = "Test",
        Material = "PLA",
        ColorName = "White",
        ColorHex = "#FFFFFF",
        InitialWeightG = weight,
        CurrentWeightG = weight,
        SpoolWeightG = 200,
        DiameterMm = 1.75f,
        LowStockThresholdG = 100,
        CreatedAt = DateTime.UtcNow
    };
}
