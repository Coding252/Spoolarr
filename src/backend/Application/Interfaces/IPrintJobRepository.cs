using Domain.Models;

namespace Application.Interfaces;

public interface IPrintJobRepository
{
    Task<IEnumerable<PrintJob>> GetBySpoolIdAsync(Guid spoolId);
    Task<IEnumerable<PrintJob>> GetByPrinterIdAsync(Guid printerId);
    Task<IEnumerable<PrintJob>> GetRunningAsync();
    Task<PrintJob?> GetByIdAsync(Guid id);
    Task<PrintJob> CreateAsync(PrintJob printJob);
    Task<PrintJob> UpdateAsync(PrintJob printJob);
}
