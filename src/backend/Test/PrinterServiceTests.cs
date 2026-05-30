using Application.DTOs;
using Application.Interfaces;
using Domain.Models;
using Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Test;

public class PrinterServiceTests
{
    private readonly IPrinterRepository _repo = Substitute.For<IPrinterRepository>();
    private readonly IDataProtector _protector = Substitute.For<IDataProtector>();
    private readonly PrinterService _sut;

    public PrinterServiceTests()
    {
        var provider = Substitute.For<IDataProtectionProvider>();
        provider.CreateProtector(Arg.Any<string>()).Returns(_protector);
        _sut = new PrinterService(_repo, provider, NullLogger<PrinterService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPrinters()
    {
        _repo.GetAllAsync().Returns([BuildPrinter(), BuildPrinter()]);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsPrinterResponse()
    {
        var printer = BuildPrinter();
        _repo.GetByIdAsync(printer.Id).Returns(printer);

        var result = await _sut.GetByIdAsync(printer.Id);

        Assert.NotNull(result);
        Assert.Equal(printer.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((Printer?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_WithCloudPassword_DoesNotStorePlainText()
    {
        _repo.CreateAsync(Arg.Any<Printer>()).Returns(x => x.Arg<Printer>());

        var request = new RegisterPrinterRequest(
            "My Printer", "Bambu", "X1C", "bambu_cloud",
            SerialNumber: "ABC123", IpAddress: null, AccessCode: null,
            Port: 8883, CloudEmail: "test@test.com", CloudPassword: "secret");

        await _sut.RegisterAsync(request);

        await _repo.Received(1).CreateAsync(Arg.Is<Printer>(p =>
            p.CloudPassword != null && p.CloudPassword != "secret"));
    }

    [Fact]
    public async Task RegisterAsync_WhenNoCloudPassword_StoresNull()
    {
        _repo.CreateAsync(Arg.Any<Printer>()).Returns(x => x.Arg<Printer>());

        var request = new RegisterPrinterRequest(
            "LAN Printer", "Bambu", "P1S", "bambu_lan",
            SerialNumber: "XYZ", IpAddress: "192.168.1.100", AccessCode: "12345678",
            Port: 8883, CloudEmail: null, CloudPassword: null);

        await _sut.RegisterAsync(request);

        await _repo.Received(1).CreateAsync(Arg.Is<Printer>(p => p.CloudPassword == null));
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_AppliesChangedFields()
    {
        var printer = BuildPrinter();
        _repo.GetByIdAsync(printer.Id).Returns(printer);
        _repo.UpdateAsync(Arg.Any<Printer>()).Returns(x => x.Arg<Printer>());

        var result = await _sut.UpdateAsync(printer.Id, new UpdatePrinterRequest(
            Name: "Updated", Brand: null, Model: null, SerialNumber: null,
            IpAddress: "10.0.0.5", Protocol: null, AccessCode: null,
            Port: null, CloudEmail: null, CloudPassword: null,
            HasAms: null, AmsSlotCount: null, Notes: null));

        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
        Assert.Equal("10.0.0.5", result.IpAddress);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((Printer?)null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), new UpdatePrinterRequest(
            null, null, null, null, null, null, null, null, null, null, null, null, null));

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_ReturnsTrue()
    {
        var printer = BuildPrinter();
        _repo.GetByIdAsync(printer.Id).Returns(printer);

        var result = await _sut.DeleteAsync(printer.Id);

        Assert.True(result);
        await _repo.Received(1).DeleteAsync(printer.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((Printer?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
        await _repo.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task GetByIdAsync_HasCloudPassword_TrueWhenEncryptedPasswordExists()
    {
        var printer = BuildPrinter(cloudPassword: "encrypted-value");
        _repo.GetByIdAsync(printer.Id).Returns(printer);

        var result = await _sut.GetByIdAsync(printer.Id);

        Assert.True(result!.HasCloudPassword);
    }

    [Fact]
    public async Task GetByIdAsync_HasCloudPassword_FalseWhenNoPassword()
    {
        var printer = BuildPrinter(cloudPassword: null);
        _repo.GetByIdAsync(printer.Id).Returns(printer);

        var result = await _sut.GetByIdAsync(printer.Id);

        Assert.False(result!.HasCloudPassword);
    }

    private static Printer BuildPrinter(string? cloudPassword = null) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test Printer",
        Brand = "Bambu",
        Model = "X1C",
        SerialNumber = "ABC123",
        IpAddress = "192.168.1.100",
        Protocol = "bambu_lan",
        AccessCode = "12345678",
        Port = 8883,
        CloudPassword = cloudPassword,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };
}
