using Application.DTOs;

namespace Application.Interfaces;

public interface ISpoolService
{
    Task<IEnumerable<SpoolResponse>> GetAllAsync();
    Task<SpoolResponse?> GetByIdAsync(Guid id);
    Task<SpoolResponse> RegisterAsync(RegisterSpoolRequest request);
    Task<SpoolResponse?> ActivateAsync(Guid id);
    Task<SpoolResponse?> UpdateWeightAsync(Guid id, UpdateWeightRequest request);
}
