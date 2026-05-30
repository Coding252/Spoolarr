using Application.DTOs;
using Application.Interfaces;
using Application.Services;
using Domain.Models;
using NSubstitute;

namespace Test;

public class SpoolServiceTests
{
    private readonly ISpoolRepository _repo = Substitute.For<ISpoolRepository>();
    private readonly SpoolService _sut;

    public SpoolServiceTests() => _sut = new SpoolService(_repo);

    [Fact]
    public async Task GetAllAsync_ReturnsAllSpools()
    {
        _repo.GetAllAsync().Returns([BuildSpool(), BuildSpool()]);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsResponse()
    {
        var spool = BuildSpool();
        _repo.GetByIdAsync(spool.Id).Returns(spool);

        var result = await _sut.GetByIdAsync(spool.Id);

        Assert.NotNull(result);
        Assert.Equal(spool.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((Spool?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_CreatesSpoolWithCorrectWeight()
    {
        var request = new RegisterSpoolRequest("Bambu", "PLA", "White", "#FFFFFF", 1000);
        _repo.CreateAsync(Arg.Any<Spool>()).Returns(x => x.Arg<Spool>());

        var result = await _sut.RegisterAsync(request);

        Assert.Equal(1000, result.CurrentWeightG);
        Assert.Equal(1000, result.InitialWeightG);
    }

    [Fact]
    public async Task ActivateAsync_WhenFound_SetsIsActiveTrue()
    {
        var spool = BuildSpool();
        _repo.GetByIdAsync(spool.Id).Returns(spool);
        _repo.GetActiveAsync().Returns((Spool?)null);
        _repo.UpdateAsync(Arg.Any<Spool>()).Returns(x => x.Arg<Spool>());

        var result = await _sut.ActivateAsync(spool.Id);

        Assert.NotNull(result);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task ActivateAsync_DeactivatesPreviousActiveSpool()
    {
        var previous = BuildSpool(isActive: true);
        var target = BuildSpool();
        _repo.GetByIdAsync(target.Id).Returns(target);
        _repo.GetActiveAsync().Returns(previous);
        _repo.UpdateAsync(Arg.Any<Spool>()).Returns(x => x.Arg<Spool>());

        await _sut.ActivateAsync(target.Id);

        Assert.False(previous.IsActive);
    }

    [Fact]
    public async Task ActivateAsync_WhenNotFound_ReturnsNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((Spool?)null);

        var result = await _sut.ActivateAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WhenFound_AppliesChangedFields()
    {
        var spool = BuildSpool();
        _repo.GetByIdAsync(spool.Id).Returns(spool);
        _repo.UpdateAsync(Arg.Any<Spool>()).Returns(x => x.Arg<Spool>());

        var result = await _sut.UpdateAsync(spool.Id, new UpdateSpoolRequest(
            Brand: "Prusa", Material: null, ColorName: "Red", ColorHex: "#FF0000",
            CurrentWeightG: null, SpoolWeightG: null, DiameterMm: null,
            LowStockThresholdG: null, Notes: "updated"));

        Assert.NotNull(result);
        Assert.Equal("Prusa", result.Brand);
        Assert.Equal("PLA", result.Material);
        Assert.Equal("Red", result.ColorName);
        Assert.Equal("updated", result.Notes);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((Spool?)null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), new UpdateSpoolRequest(
            null, null, null, null, null, null, null, null, null));

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenFound_ReturnsTrue()
    {
        var spool = BuildSpool();
        _repo.GetByIdAsync(spool.Id).Returns(spool);

        var result = await _sut.DeleteAsync(spool.Id);

        Assert.True(result);
        await _repo.Received(1).DeleteAsync(spool.Id);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>()).Returns((Spool?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
        await _repo.DidNotReceive().DeleteAsync(Arg.Any<Guid>());
    }

    private static Spool BuildSpool(bool isActive = false, float weight = 1000) => new()
    {
        Id = Guid.NewGuid(),
        Brand = "Bambu",
        Material = "PLA",
        ColorName = "White",
        ColorHex = "#FFFFFF",
        InitialWeightG = weight,
        CurrentWeightG = weight,
        SpoolWeightG = 200,
        DiameterMm = 1.75f,
        LowStockThresholdG = 100,
        IsActive = isActive,
        CreatedAt = DateTime.UtcNow
    };
}
