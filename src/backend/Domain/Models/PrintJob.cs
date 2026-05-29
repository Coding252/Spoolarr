namespace Domain.Models;

public class PrintJob
{
    public Guid Id { get; set; }
    public Guid PrinterId { get; set; }
    public Guid SpoolId { get; set; }
    public string? PrintFileName { get; set; }
    public string Status { get; set; } = string.Empty;
    public float GramsUsed { get; set; } = 0;
    public float LastReportedGramsUsed { get; set; } = 0;
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string Source { get; set; } = "mqtt";
    public string? Notes { get; set; }
    public Printer Printer { get; set; } = null!;
    public Spool Spool { get; set; } = null!;
}
