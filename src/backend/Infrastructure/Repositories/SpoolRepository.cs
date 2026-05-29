using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SpoolRepository(FilamentDbContext db) : ISpoolRepository
{
    public async Task<IEnumerable<Spool>> GetAllAsync()
    {
        return await db.Spools
            .Where(s => !s.IsArchived)
            .OrderByDescending(s => s.LastScannedAt)
            .ToListAsync();
    }

    public async Task<Spool?> GetByIdAsync(Guid id)
    {
        return await db.Spools
            .Include(s => s.NfcTags)
            .Include(s => s.PrintJobs)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Spool?> GetActiveAsync()
    {
        return await db.Spools
            .FirstOrDefaultAsync(s => s.IsActive);
    }

    public async Task<Spool> CreateAsync(Spool spool)
    {
        db.Spools.Add(spool);
        await db.SaveChangesAsync();
        return spool;
    }

    public async Task<Spool> UpdateAsync(Spool spool)
    {
        db.Spools.Update(spool);
        await db.SaveChangesAsync();
        return spool;
    }

    public async Task ArchiveAsync(Guid id)
    {
        await db.Spools
            .Where(s => s.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsArchived, true));
    }

    public async Task DeleteAsync(Guid id)
    {
        await db.Spools
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync();
    }
}
