namespace Application.DTOs;

public record SpoolResponse(
    Guid Id,
    string Brand,
    string Material,
    string ColorName,
    string ColorHex,
    float InitialWeightG,
    float CurrentWeightG,
    float SpoolWeightG,
    float DiameterMm,
    float LowStockThresholdG,
    bool IsActive,
    bool IsArchived,
    DateTime CreatedAt,
    DateTime? LastScannedAt,
    string? Notes
);
