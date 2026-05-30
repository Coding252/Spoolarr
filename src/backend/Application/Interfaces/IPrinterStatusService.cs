using Application.DTOs;

namespace Application.Interfaces;

public interface IPrinterStatusService
{
    PrinterStatus? GetStatus();
    void UpdateStatus(PrinterStatus status);
}
