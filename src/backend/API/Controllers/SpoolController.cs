using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/spools")]
public class SpoolController(ISpoolService spoolService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllSpools()
    {
        var spools = await spoolService.GetAllAsync();
        return Ok(spools);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSpoolById(Guid id)
    {
        var spool = await spoolService.GetByIdAsync(id);
        return spool is null ? NotFound() : Ok(spool);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterSpool([FromBody] RegisterSpoolRequest request)
    {
        var spool = await spoolService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetSpoolById), new { id = spool.Id }, spool);
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ActivateSpool(Guid id)
    {
        var spool = await spoolService.ActivateAsync(id);
        return spool is null ? NotFound() : Ok(spool);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSpool(Guid id, [FromBody] UpdateSpoolRequest request)
    {
        var spool = await spoolService.UpdateAsync(id, request);
        return spool is null ? NotFound() : Ok(spool);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSpool(Guid id)
    {
        var deleted = await spoolService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
