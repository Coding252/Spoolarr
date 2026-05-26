namespace Domain.Models;

public class Print
{
    public Guid Id { get; set; }
    public Guid SpoolId { get; set; }
    public string? BambuTaskId { get; set; }
    public string? BambuSerialNumber { get; set; }
    public string? Name { get; set; }
    public string? GcodeFile { get; set; }
    public double GramsUsed { get; set; }
    public DateTime PrintedAt { get; set; }
    public PrintSource Source { get; set; } = PrintSource.Mqtt;
    public Spool Spool { get; set; } = null!;
}
