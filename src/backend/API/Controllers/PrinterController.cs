using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/printers")]
public class PrinterController(
    IPrinterService printerService,
    IPrinterStatusService printerStatusService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllPrinters()
    {
        var printers = await printerService.GetAllAsync();
        return Ok(printers);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPrinterById(Guid id)
    {
        var printer = await printerService.GetByIdAsync(id);
        return printer is null ? NotFound() : Ok(printer);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterPrinter([FromBody] RegisterPrinterRequest request)
    {
        var printer = await printerService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetPrinterById), new { id = printer.Id }, printer);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePrinter(Guid id, [FromBody] UpdatePrinterRequest request)
    {
        var printer = await printerService.UpdateAsync(id, request);
        return printer is null ? NotFound() : Ok(printer);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePrinter(Guid id)
    {
        var deleted = await printerService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var status = printerStatusService.GetStatus();
        return status is null ? NoContent() : Ok(status);
    }
}
