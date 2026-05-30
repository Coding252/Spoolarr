using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Test;

public class PrinterControllerTests
{
    private readonly IPrinterStatusService _statusService = Substitute.For<IPrinterStatusService>();
    private readonly PrinterController _sut;

    public PrinterControllerTests() => _sut = new PrinterController(_statusService);

    [Fact]
    public void GetStatus_WhenNoStatus_ReturnsNoContent()
    {
        _statusService.GetStatus().Returns((PrinterStatus?)null);

        var result = _sut.GetStatus();

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void GetStatus_WhenStatusExists_ReturnsOk()
    {
        _statusService.GetStatus().Returns(BuildStatus("RUNNING"));

        var result = _sut.GetStatus();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<PrinterStatus>(ok.Value);
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
