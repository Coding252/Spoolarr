using Application.DTOs;

namespace Application.Interfaces;

public interface IPrinterService
{
    Task<IEnumerable<PrinterResponse>> GetAllAsync();
    Task<PrinterResponse?> GetByIdAsync(Guid id);
    Task<PrinterResponse> RegisterAsync(RegisterPrinterRequest request);
    Task<PrinterResponse?> UpdateAsync(Guid id, UpdatePrinterRequest request);
    Task<bool> DeleteAsync(Guid id);
}
