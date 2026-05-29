using Application.Interfaces;
using Domain.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PrintJobRepository(FilamentDbContext db) : IPrintJobRepository
{
    public async Task<IEnumerable<PrintJob>> GetBySpoolIdAsync(Guid spoolId)
    {
        return await db.PrintJobs
            .Where(j => j.SpoolId == spoolId)
            .OrderByDescending(j => j.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PrintJob>> GetByPrinterIdAsync(Guid printerId)
    {
        return await db.PrintJobs
            .Where(j => j.PrinterId == printerId)
            .OrderByDescending(j => j.StartedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PrintJob>> GetRunningAsync()
    {
        return await db.PrintJobs
            .Where(j => j.Status == "running")
            .ToListAsync();
    }

    public async Task<PrintJob?> GetByIdAsync(Guid id)
    {
        return await db.PrintJobs
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<PrintJob> CreateAsync(PrintJob printJob)
    {
        db.PrintJobs.Add(printJob);
        await db.SaveChangesAsync();
        return printJob;
    }

    public async Task<PrintJob> UpdateAsync(PrintJob printJob)
    {
        db.PrintJobs.Update(printJob);
        await db.SaveChangesAsync();
        return printJob;
    }
}
