using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record UpdateWeightRequest(
    [Range(0, double.MaxValue, ErrorMessage = "NewWeightG must be 0 or greater")] float NewWeightG
);
