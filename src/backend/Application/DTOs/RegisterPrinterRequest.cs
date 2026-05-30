using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record RegisterPrinterRequest(
    [Required] string Name,
    [Required] string Brand,
    [Required] string Model,
    [Required] string Protocol,
    string? SerialNumber,
    string? IpAddress,
    string? AccessCode,
    int? Port,
    string? CloudEmail,
    string? CloudPassword,
    bool HasAms = false,
    int AmsSlotCount = 0,
    string? Notes = null
);
