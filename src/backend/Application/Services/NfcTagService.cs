using Application.DTOs;
using Application.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class NfcTagService(
    INfcTagRepository nfcTagRepository,
    ILogger<NfcTagService> logger) : INfcTagService
{
    public async Task<IEnumerable<NfcTagResponse>> GetAllAsync()
    {
        var tags = await nfcTagRepository.GetAllAsync();
        return tags.Select(ToResponse);
    }

    public async Task<NfcTagResponse?> GetByIdAsync(Guid id)
    {
        var tag = await nfcTagRepository.GetByIdAsync(id);
        return tag is null ? null : ToResponse(tag);
    }

    public async Task<NfcTagResponse> RegisterAsync(RegisterNfcTagRequest request)
    {
        var tag = new NfcTag
        {
            Id = Guid.NewGuid(),
            TagUid = request.TagUid,
            Type = request.Type,
            SpoolId = request.SpoolId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await nfcTagRepository.CreateAsync(tag);
        logger.LogInformation("Registered NFC tag {TagUid} for spool {SpoolId}", created.TagUid, created.SpoolId);
        return ToResponse(created);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var tag = await nfcTagRepository.GetByIdAsync(id);
        if (tag is null)
            return false;

        await nfcTagRepository.DeleteAsync(id);
        return true;
    }

    private static NfcTagResponse ToResponse(NfcTag tag) => new(
        tag.Id,
        tag.TagUid,
        tag.Type,
        tag.SpoolId,
        tag.CreatedAt);
}
