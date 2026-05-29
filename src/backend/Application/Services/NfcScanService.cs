using Application.DTOs;
using Application.Interfaces;

namespace Application.Services;

public class NfcScanService(INfcTagRepository nfcTagRepository, ISpoolService spoolService) : INfcScanService
{
    public async Task<NfcScanResult> ProcessScanAsync(string tagUid)
    {
        var tag = await nfcTagRepository.GetByTagUidAsync(tagUid);

        if (tag is null)
            return new NfcScanResult("unknown", tagUid, null, "Tag not registered");

        var spool = await spoolService.ActivateAsync(tag.SpoolId);
        return new NfcScanResult("activated", tagUid, spool, null);
    }
}
