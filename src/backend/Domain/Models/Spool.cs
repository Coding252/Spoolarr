namespace Domain.Models;

public class Spool
{
    public Guid Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Material { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string ColorHex { get; set; } = string.Empty;
    public float InitialWeightG { get; set; }
    public float CurrentWeightG { get; set; }
    public float SpoolWeightG { get; set; } = 200;
    public float DiameterMm { get; set; } = 1.75f;
    public float LowStockThresholdG { get; set; } = 100;
    public bool IsActive { get; set; } = false;
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastScannedAt { get; set; }
    public string? Notes { get; set; }
    public ICollection<NfcTag> NfcTags { get; set; } = new List<NfcTag>();
    public ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
}
