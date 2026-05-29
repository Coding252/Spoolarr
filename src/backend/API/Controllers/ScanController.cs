using API.Hubs;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace API.Controllers;

[ApiController]
[Route("api/spools")]
public class ScanController(
    INfcScanService nfcScanService,
    IHubContext<NfcScanHub> hubContext,
    ILogger<ScanController> logger) : ControllerBase
{
    [HttpPost("scan")]
    public async Task<IActionResult> Scan([FromBody] ScanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TagUid))
        {
            logger.LogWarning("Scan received with empty TagUid");
            return BadRequest("TagUid is required");
        }

        var result = await nfcScanService.ProcessScanAsync(request.TagUid);

        try
        {
            await hubContext.Clients.All.SendAsync("ScanResult", result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to push ScanResult via SignalR");
        }

        return Ok(result);
    }
}
