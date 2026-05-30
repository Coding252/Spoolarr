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
    public async Task GetAll_ReturnsOkWithSpools()
    {
        _service.GetAllAsync().Returns([BuildResponse(), BuildResponse()]);

        var result = await _sut.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<SpoolResponse>>(ok.Value);
    }

    [Fact]
    public async Task GetById_WhenFound_ReturnsOk()
    {
        var response = BuildResponse();
        _service.GetByIdAsync(response.Id).Returns(response);

        var result = await _sut.GetById(response.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ReturnsNotFound()
    {
        _service.GetByIdAsync(Arg.Any<Guid>()).Returns((SpoolResponse?)null);

        var result = await _sut.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Register_ReturnsCreated()
    {
        var request = new RegisterSpoolRequest("Bambu", "PLA", "White", "#FFFFFF", 1000);
        var response = BuildResponse();
        _service.RegisterAsync(request).Returns(response);

        var result = await _sut.Register(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(response, created.Value);
    }

    [Fact]
    public async Task Activate_WhenFound_ReturnsOk()
    {
        var response = BuildResponse(isActive: true);
        _service.ActivateAsync(response.Id).Returns(response);

        var result = await _sut.Activate(response.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, ok.Value);
    }

    [Fact]
    public async Task Activate_WhenNotFound_ReturnsNotFound()
    {
        _service.ActivateAsync(Arg.Any<Guid>()).Returns((SpoolResponse?)null);

        var result = await _sut.Activate(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateWeight_WhenFound_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var request = new UpdateWeightRequest(300);
        var response = BuildResponse();
        _service.UpdateWeightAsync(id, request).Returns(response);

        var result = await _sut.UpdateWeight(id, request);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateWeight_WhenNotFound_ReturnsNotFound()
    {
        _service.UpdateWeightAsync(Arg.Any<Guid>(), Arg.Any<UpdateWeightRequest>()).Returns((SpoolResponse?)null);

        var result = await _sut.UpdateWeight(Guid.NewGuid(), new UpdateWeightRequest(100));

        Assert.IsType<NotFoundResult>(result);
    }

    private static SpoolResponse BuildResponse(bool isActive = false) => new(
        Guid.NewGuid(), "Bambu", "PLA", "White", "#FFFFFF",
        1000, 800, 200, 1.75f, 100, isActive, false,
        DateTime.UtcNow, null, null);
}
