using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/spools")]
public class SpoolController(ISpoolService spoolService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var spools = await spoolService.GetAllAsync();
        return Ok(spools);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var spool = await spoolService.GetByIdAsync(id);
        return spool is null ? NotFound() : Ok(spool);
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] RegisterSpoolRequest request)
    {
        var spool = await spoolService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = spool.Id }, spool);
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var spool = await spoolService.ActivateAsync(id);
        return spool is null ? NotFound() : Ok(spool);
    }

    [HttpPatch("{id:guid}/weight")]
    public async Task<IActionResult> UpdateWeight(Guid id, [FromBody] UpdateWeightRequest request)
    {
        var spool = await spoolService.UpdateWeightAsync(id, request);
        return spool is null ? NotFound() : Ok(spool);
    }
}
