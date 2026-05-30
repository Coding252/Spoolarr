using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Test;

public class PrinterControllerTests
{
    private readonly IPrinterService _service = Substitute.For<IPrinterService>();
    private readonly IPrinterStatusService _statusService = Substitute.For<IPrinterStatusService>();
    private readonly PrinterController _sut;

    public PrinterControllerTests() => _sut = new PrinterController(_service, _statusService);

    [Fact]
    public async Task GetAllPrinters_ReturnsOkWithPrinters()
    {
        _service.GetAllAsync().Returns([BuildResponse(), BuildResponse()]);

        var result = await _sut.GetAllPrinters();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<PrinterResponse>>(ok.Value);
    }

    [Fact]
    public async Task GetPrinterById_WhenFound_ReturnsOk()
    {
        var response = BuildResponse();
        _service.GetByIdAsync(response.Id).Returns(response);

        var result = await _sut.GetPrinterById(response.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task GetPrinterById_WhenNotFound_ReturnsNotFound()
    {
        _service.GetByIdAsync(Arg.Any<Guid>()).Returns((PrinterResponse?)null);

        var result = await _sut.GetPrinterById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RegisterPrinter_ReturnsCreated()
    {
        var request = new RegisterPrinterRequest("My Printer", "Bambu", "X1C", "bambu_lan",
            SerialNumber: null, IpAddress: "192.168.1.100", AccessCode: "12345678", Port: 8883,
            CloudEmail: null, CloudPassword: null);
        var response = BuildResponse();
        _service.RegisterAsync(request).Returns(response);

        var result = await _sut.RegisterPrinter(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(response, created.Value);
    }

    [Fact]
    public async Task UpdatePrinter_WhenFound_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var request = new UpdatePrinterRequest("New Name", null, null, null, null, null, null, null, null, null, null, null, null);
        var response = BuildResponse();
        _service.UpdateAsync(id, request).Returns(response);

        var result = await _sut.UpdatePrinter(id, request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePrinter_WhenNotFound_ReturnsNotFound()
    {
        _service.UpdateAsync(Arg.Any<Guid>(), Arg.Any<UpdatePrinterRequest>()).Returns((PrinterResponse?)null);

        var result = await _sut.UpdatePrinter(Guid.NewGuid(), new UpdatePrinterRequest(null, null, null, null, null, null, null, null, null, null, null, null, null));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeletePrinter_WhenFound_ReturnsNoContent()
    {
        _service.DeleteAsync(Arg.Any<Guid>()).Returns(true);

        var result = await _sut.DeletePrinter(Guid.NewGuid());

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeletePrinter_WhenNotFound_ReturnsNotFound()
    {
        _service.DeleteAsync(Arg.Any<Guid>()).Returns(false);

        var result = await _sut.DeletePrinter(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

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
        _statusService.GetStatus().Returns(new PrinterStatus(
            "RUNNING", 50, 10, "test.gcode", 100, 200, 220, 65, DateTime.UtcNow));

        var result = _sut.GetStatus();

        Assert.IsType<OkObjectResult>(result);
    }

    private static PrinterResponse BuildResponse() => new(
        Guid.NewGuid(), "My Printer", "Bambu", "X1C", "ABC123",
        "192.168.1.100", "bambu_lan", "12345678", 8883,
        null, false, false, 0, true, null, DateTime.UtcNow, null);
}
