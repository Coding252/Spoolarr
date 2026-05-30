using Application.DTOs;
using Application.Interfaces;
using Domain.Models;

namespace Application.Services;

public class SpoolService(ISpoolRepository spoolRepository) : ISpoolService
{
    public async Task<IEnumerable<SpoolResponse>> GetAllAsync()
    {
        var spools = await spoolRepository.GetAllAsync();
        return spools.Select(ToResponse);
    }

    public async Task<SpoolResponse?> GetByIdAsync(Guid id)
    {
        var spool = await spoolRepository.GetByIdAsync(id);
        return spool is null ? null : ToResponse(spool);
    }

    public async Task<SpoolResponse> RegisterAsync(RegisterSpoolRequest request)
    {
        var spool = new Spool
        {
            Id = Guid.NewGuid(),
            Brand = request.Brand,
            Material = request.Material,
            ColorName = request.ColorName,
            ColorHex = request.ColorHex,
            InitialWeightG = request.InitialWeightG,
            CurrentWeightG = request.InitialWeightG,
            SpoolWeightG = request.SpoolWeightG,
            DiameterMm = request.DiameterMm,
            LowStockThresholdG = request.LowStockThresholdG,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var created = await spoolRepository.CreateAsync(spool);
        return ToResponse(created);
    }

    public async Task<SpoolResponse?> ActivateAsync(Guid id)
    {
        var target = await spoolRepository.GetByIdAsync(id);
        if (target is null)
            return null;

        var current = await spoolRepository.GetActiveAsync();
        if (current is not null && current.Id != id)
        {
            current.IsActive = false;
            await spoolRepository.UpdateAsync(current);
        }

        target.IsActive = true;
        target.LastScannedAt = DateTime.UtcNow;
        var updated = await spoolRepository.UpdateAsync(target);
        return ToResponse(updated);
    }

    public async Task<SpoolResponse?> UpdateAsync(Guid id, UpdateSpoolRequest request)
    {
        var spool = await spoolRepository.GetByIdAsync(id);
        if (spool is null)
            return null;

        if (request.Brand is not null) spool.Brand = request.Brand;
        if (request.Material is not null) spool.Material = request.Material;
        if (request.ColorName is not null) spool.ColorName = request.ColorName;
        if (request.ColorHex is not null) spool.ColorHex = request.ColorHex;
        if (request.CurrentWeightG is not null) spool.CurrentWeightG = request.CurrentWeightG.Value;
        if (request.SpoolWeightG is not null) spool.SpoolWeightG = request.SpoolWeightG.Value;
        if (request.DiameterMm is not null) spool.DiameterMm = request.DiameterMm.Value;
        if (request.LowStockThresholdG is not null) spool.LowStockThresholdG = request.LowStockThresholdG.Value;
        if (request.Notes is not null) spool.Notes = request.Notes;

        var updated = await spoolRepository.UpdateAsync(spool);
        return ToResponse(updated);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var spool = await spoolRepository.GetByIdAsync(id);
        if (spool is null)
            return false;

        await spoolRepository.DeleteAsync(id);
        return true;
    }

    private static SpoolResponse ToResponse(Spool spool) => new(
        spool.Id,
        spool.Brand,
        spool.Material,
        spool.ColorName,
        spool.ColorHex,
        spool.InitialWeightG,
        spool.CurrentWeightG,
        spool.SpoolWeightG,
        spool.DiameterMm,
        spool.LowStockThresholdG,
        spool.IsActive,
        spool.IsArchived,
        spool.CreatedAt,
        spool.LastScannedAt,
        spool.Notes
    );
}
