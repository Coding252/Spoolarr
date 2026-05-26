namespace Domain.Models;

public class Spool
{
    public Guid Id { get; set; }
    public string NfcTagUid { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public double InitialWeightG { get; set; }
    public double CurrentWeightG { get; set; }
    public double LowStockThresholdG { get; set; } = 100;
    public bool IsActive { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? LastScannedAt { get; set; }
    public string? Notes { get; set; }
    public ICollection<Print> Prints { get; set; } = new List<Print>();
}
