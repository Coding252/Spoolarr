using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record RegisterSpoolRequest(
    [Required] string Brand,
    [Required] string Material,
    [Required] string ColorName,
    [Required] string ColorHex,
    [Range(0.001, double.MaxValue, ErrorMessage = "InitialWeightG must be greater than 0")] float InitialWeightG,
    float SpoolWeightG = 200,
    float DiameterMm = 1.75f,
    float LowStockThresholdG = 100,
    string? Notes = null
);
