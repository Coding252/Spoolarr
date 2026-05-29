using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class FilamentDbContext(DbContextOptions<FilamentDbContext> options) : DbContext(options)
{
    public DbSet<Spool> Spools => Set<Spool>();
    public DbSet<Printer> Printers => Set<Printer>();
    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();
    public DbSet<NfcTag> NfcTags => Set<NfcTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NfcTag>()
            .HasIndex(t => t.TagUid)
            .IsUnique();

        modelBuilder.Entity<NfcTag>()
            .HasOne(t => t.Spool)
            .WithMany(s => s.NfcTags)
            .HasForeignKey(t => t.SpoolId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PrintJob>()
            .HasOne(p => p.Spool)
            .WithMany(s => s.PrintJobs)
            .HasForeignKey(p => p.SpoolId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PrintJob>()
            .HasOne(p => p.Printer)
            .WithMany(pr => pr.PrintJobs)
            .HasForeignKey(p => p.PrinterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
