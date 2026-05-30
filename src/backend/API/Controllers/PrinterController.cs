using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/printers")]
public class PrinterController(IPrinterStatusService printerStatusService) : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var status = printerStatusService.GetStatus();
        return status is null ? NoContent() : Ok(status);
    }
}
