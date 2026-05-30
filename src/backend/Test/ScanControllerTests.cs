using API.Controllers;
using API.Hubs;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Test;

public class ScanControllerTests
{
    private readonly INfcScanService _nfcScanService = Substitute.For<INfcScanService>();
    private readonly IHubContext<NfcScanHub> _hubContext = Substitute.For<IHubContext<NfcScanHub>>();
    private readonly IClientProxy _clientProxy = Substitute.For<IClientProxy>();
    private readonly ScanController _sut;

    public ScanControllerTests()
    {
        var clients = Substitute.For<IHubClients>();
        clients.All.Returns(_clientProxy);
        _hubContext.Clients.Returns(clients);

        _sut = new ScanController(_nfcScanService, _hubContext, NullLogger<ScanController>.Instance);
    }

    [Fact]
    public async Task Scan_WhenTagUidIsEmpty_ReturnsBadRequest()
    {
        var result = await _sut.Scan(new ScanRequest(string.Empty));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Scan_WhenTagUidIsWhitespace_ReturnsBadRequest()
    {
        var result = await _sut.Scan(new ScanRequest("   "));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Scan_WhenTagUidIsValid_ReturnsOk()
    {
        var scanResult = new NfcScanResult("unknown", "04:AA:BB:CC", null, "Tag not registered");
        _nfcScanService.ProcessScanAsync("04:AA:BB:CC").Returns(scanResult);

        var result = await _sut.Scan(new ScanRequest("04:AA:BB:CC"));

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(scanResult, ok.Value);
    }

    [Fact]
    public async Task Scan_WhenTagUidIsValid_PushesResultToHub()
    {
        var scanResult = new NfcScanResult("unknown", "04:AA:BB:CC", null, "Tag not registered");
        _nfcScanService.ProcessScanAsync("04:AA:BB:CC").Returns(scanResult);

        await _sut.Scan(new ScanRequest("04:AA:BB:CC"));

        await _clientProxy.ReceivedWithAnyArgs(1).SendCoreAsync(default!, default!, default);
    }
}
