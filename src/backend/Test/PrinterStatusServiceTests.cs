using Application.DTOs;
using Application.Services;

namespace Test;

public class PrinterStatusServiceTests
{
    private readonly PrinterStatusService _sut = new();

    [Fact]
    public void GetStatus_Initially_ReturnsNull()
    {
        Assert.Null(_sut.GetStatus());
    }

    [Fact]
    public void UpdateStatus_SetsStatus()
    {
        var status = BuildStatus("RUNNING");

        _sut.UpdateStatus(status);

        Assert.NotNull(_sut.GetStatus());
    }

    [Fact]
    public void GetStatus_AfterUpdate_ReturnsCorrectState()
    {
        _sut.UpdateStatus(BuildStatus("RUNNING"));

        Assert.Equal("RUNNING", _sut.GetStatus()!.GcodeState);
    }

    [Fact]
    public void UpdateStatus_Twice_ReturnsLatest()
    {
        _sut.UpdateStatus(BuildStatus("RUNNING"));
        _sut.UpdateStatus(BuildStatus("FINISH"));

        Assert.Equal("FINISH", _sut.GetStatus()!.GcodeState);
    }

    private static PrinterStatus BuildStatus(string state) => new(
        GcodeState: state,
        ProgressPercent: 50,
        RemainingMinutes: 10,
        SubtaskName: "test.gcode",
        LayerNum: 100,
        TotalLayerNum: 200,
        NozzleTempC: 220,
        BedTempC: 65,
        UpdatedAt: DateTime.UtcNow);
}
