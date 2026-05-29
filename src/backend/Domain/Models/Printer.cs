namespace Domain.Models;

public class Printer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string? AccessCode { get; set; }
    public int? Port { get; set; }
    public bool HasAms { get; set; } = false;
    public int AmsSlotCount { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public ICollection<PrintJob> PrintJobs { get; set; } = new List<PrintJob>();
}
