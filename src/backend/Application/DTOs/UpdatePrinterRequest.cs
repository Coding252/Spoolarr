namespace Application.DTOs;

public record UpdatePrinterRequest(
    string? Name,
    string? Brand,
    string? Model,
    string? SerialNumber,
    string? IpAddress,
    string? Protocol,
    string? AccessCode,
    int? Port,
    string? CloudEmail,
    string? CloudPassword,
    bool? HasAms,
    int? AmsSlotCount,
    string? Notes
);
