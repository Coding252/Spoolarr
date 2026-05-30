namespace Application.DTOs;

public record PrinterResponse(
    Guid Id,
    string Name,
    string Brand,
    string Model,
    string? SerialNumber,
    string IpAddress,
    string Protocol,
    string? AccessCode,
    int? Port,
    string? CloudEmail,
    bool HasCloudPassword,
    bool HasAms,
    int AmsSlotCount,
    bool IsActive,
    DateTime? LastSeenAt,
    DateTime CreatedAt,
    string? Notes
);
