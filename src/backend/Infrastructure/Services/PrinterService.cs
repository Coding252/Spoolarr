using Application.DTOs;
using Application.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class PrinterService(
    IPrinterRepository printerRepository,
    IDataProtectionProvider dataProtectionProvider,
    ILogger<PrinterService> logger) : IPrinterService
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("MqttCloudPassword");

    public async Task<IEnumerable<PrinterResponse>> GetAllAsync()
    {
        var printers = await printerRepository.GetAllAsync();
        return printers.Select(ToResponse);
    }

    public async Task<PrinterResponse?> GetByIdAsync(Guid id)
    {
        var printer = await printerRepository.GetByIdAsync(id);
        return printer is null ? null : ToResponse(printer);
    }

    public async Task<PrinterResponse> RegisterAsync(RegisterPrinterRequest request)
    {
        var printer = new Printer
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Brand = request.Brand,
            Model = request.Model,
            SerialNumber = request.SerialNumber,
            IpAddress = request.IpAddress ?? string.Empty,
            Protocol = request.Protocol,
            AccessCode = request.AccessCode,
            Port = request.Port,
            CloudEmail = request.CloudEmail,
            CloudPassword = EncryptIfProvided(request.CloudPassword),
            HasAms = request.HasAms,
            AmsSlotCount = request.AmsSlotCount,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Notes = request.Notes
        };

        var created = await printerRepository.CreateAsync(printer);
        logger.LogInformation("Registered printer {Name} ({Protocol})", created.Name, created.Protocol);
        return ToResponse(created);
    }

    public async Task<PrinterResponse?> UpdateAsync(Guid id, UpdatePrinterRequest request)
    {
        var printer = await printerRepository.GetByIdAsync(id);
        if (printer is null)
            return null;

        if (request.Name is not null) printer.Name = request.Name;
        if (request.Brand is not null) printer.Brand = request.Brand;
        if (request.Model is not null) printer.Model = request.Model;
        if (request.SerialNumber is not null) printer.SerialNumber = request.SerialNumber;
        if (request.IpAddress is not null) printer.IpAddress = request.IpAddress;
        if (request.Protocol is not null) printer.Protocol = request.Protocol;
        if (request.AccessCode is not null) printer.AccessCode = request.AccessCode;
        if (request.Port is not null) printer.Port = request.Port;
        if (request.CloudEmail is not null) printer.CloudEmail = request.CloudEmail;
        if (request.CloudPassword is not null) printer.CloudPassword = _protector.Protect(request.CloudPassword);
        if (request.HasAms is not null) printer.HasAms = request.HasAms.Value;
        if (request.AmsSlotCount is not null) printer.AmsSlotCount = request.AmsSlotCount.Value;
        if (request.Notes is not null) printer.Notes = request.Notes;

        var updated = await printerRepository.UpdateAsync(printer);
        return ToResponse(updated);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var printer = await printerRepository.GetByIdAsync(id);
        if (printer is null)
            return false;

        await printerRepository.DeleteAsync(id);
        return true;
    }

    private string? EncryptIfProvided(string? password) =>
        string.IsNullOrEmpty(password) ? null : _protector.Protect(password);

    private static PrinterResponse ToResponse(Printer printer) => new(
        printer.Id,
        printer.Name,
        printer.Brand,
        printer.Model,
        printer.SerialNumber,
        printer.IpAddress,
        printer.Protocol,
        printer.AccessCode,
        printer.Port,
        printer.CloudEmail,
        HasCloudPassword: !string.IsNullOrEmpty(printer.CloudPassword),
        printer.HasAms,
        printer.AmsSlotCount,
        printer.IsActive,
        printer.LastSeenAt,
        printer.CreatedAt,
        printer.Notes
    );
}
