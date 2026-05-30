using Application.DTOs;
using Application.Interfaces;

namespace Application.Services;

public class PrinterStatusService : IPrinterStatusService
{
    private PrinterStatus? _status;

    public PrinterStatus? GetStatus() => _status;

    public void UpdateStatus(PrinterStatus status) => _status = status;
}
