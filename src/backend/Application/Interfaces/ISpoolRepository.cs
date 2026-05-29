using Domain.Models;

namespace Application.Interfaces;

public interface ISpoolRepository
{
    Task<IEnumerable<Spool>> GetAllAsync();
    Task<Spool?> GetByIdAsync(Guid id);
    Task<Spool?> GetActiveAsync();
    Task<Spool> CreateAsync(Spool spool);
    Task<Spool> UpdateAsync(Spool spool);
    Task ArchiveAsync(Guid id);
    Task DeleteAsync(Guid id);
}
