using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Test;

public class SpoolControllerTests
{
    private readonly ISpoolService _service = Substitute.For<ISpoolService>();
    private readonly SpoolController _sut;

    public SpoolControllerTests() => _sut = new SpoolController(_service);

    [Fact]
    public async Task GetAllSpools_ReturnsOkWithSpools()
    {
        _service.GetAllAsync().Returns([BuildResponse(), BuildResponse()]);

        var result = await _sut.GetAllSpools();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<SpoolResponse>>(ok.Value);
    }

    [Fact]
    public async Task GetSpoolById_WhenFound_ReturnsOk()
    {
        var response = BuildResponse();
        _service.GetByIdAsync(response.Id).Returns(response);

        var result = await _sut.GetSpoolById(response.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task GetSpoolById_WhenNotFound_ReturnsNotFound()
    {
        _service.GetByIdAsync(Arg.Any<Guid>()).Returns((SpoolResponse?)null);

        var result = await _sut.GetSpoolById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RegisterSpool_ReturnsCreated()
    {
        var request = new RegisterSpoolRequest("Bambu", "PLA", "White", "#FFFFFF", 1000);
        var response = BuildResponse();
        _service.RegisterAsync(request).Returns(response);

        var result = await _sut.RegisterSpool(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(response, created.Value);
    }

    [Fact]
    public async Task ActivateSpool_WhenFound_ReturnsOk()
    {
        var response = BuildResponse(isActive: true);
        _service.ActivateAsync(response.Id).Returns(response);

        var result = await _sut.ActivateSpool(response.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task ActivateSpool_WhenNotFound_ReturnsNotFound()
    {
        _service.ActivateAsync(Arg.Any<Guid>()).Returns((SpoolResponse?)null);

        var result = await _sut.ActivateSpool(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateSpool_WhenFound_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var request = new UpdateSpoolRequest("Prusa", null, "Red", "#FF0000", null, null, null, null, null);
        var response = BuildResponse();
        _service.UpdateAsync(id, request).Returns(response);

        var result = await _sut.UpdateSpool(id, request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateSpool_WhenNotFound_ReturnsNotFound()
    {
        _service.UpdateAsync(Arg.Any<Guid>(), Arg.Any<UpdateSpoolRequest>()).Returns((SpoolResponse?)null);

        var result = await _sut.UpdateSpool(Guid.NewGuid(), new UpdateSpoolRequest(null, null, null, null, null, null, null, null, null));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteSpool_WhenFound_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _service.DeleteAsync(id).Returns(true);

        var result = await _sut.DeleteSpool(id);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteSpool_WhenNotFound_ReturnsNotFound()
    {
        _service.DeleteAsync(Arg.Any<Guid>()).Returns(false);

        var result = await _sut.DeleteSpool(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    private static SpoolResponse BuildResponse(bool isActive = false) => new(
        Guid.NewGuid(), "Bambu", "PLA", "White", "#FFFFFF",
        1000, 800, 200, 1.75f, 100, isActive, false,
        DateTime.UtcNow, null, null);
}
