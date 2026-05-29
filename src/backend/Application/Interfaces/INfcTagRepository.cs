using Domain.Models;

namespace Application.Interfaces;

public interface INfcTagRepository
{
    Task<NfcTag?> GetByTagUidAsync(string tagUid);
    Task<IEnumerable<NfcTag>> GetBySpoolIdAsync(Guid spoolId);
    Task<NfcTag> CreateAsync(NfcTag nfcTag);
    Task DeleteAsync(Guid id);
}
