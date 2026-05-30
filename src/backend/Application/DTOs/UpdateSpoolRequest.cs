namespace Application.DTOs;

public record UpdateSpoolRequest(
    string? Brand,
    string? Material,
    string? ColorName,
    string? ColorHex,
    float? CurrentWeightG,
    float? SpoolWeightG,
    float? DiameterMm,
    float? LowStockThresholdG,
    string? Notes
);
