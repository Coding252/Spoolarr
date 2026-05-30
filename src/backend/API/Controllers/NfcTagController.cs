using API.Hubs;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace API.Controllers;

[ApiController]
[Route("api/nfc-tags")]
public class NfcTagController(
    INfcTagService nfcTagService,
    INfcScanService nfcScanService,
    IHubContext<NfcScanHub> hubContext,
    ILogger<NfcTagController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllTags()
    {
        var tags = await nfcTagService.GetAllAsync();
        return Ok(tags);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTagById(Guid id)
    {
        var tag = await nfcTagService.GetByIdAsync(id);
        return tag is null ? NotFound() : Ok(tag);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterTag([FromBody] RegisterNfcTagRequest request)
    {
        var tag = await nfcTagService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, tag);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        var deleted = await nfcTagService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("scan")]
    public async Task<IActionResult> ScanTag([FromBody] ScanRequest request)
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
