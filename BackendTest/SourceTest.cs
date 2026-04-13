using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Services;

namespace BackendTest;

public class SourceServiceTests : IDisposable
{
    private readonly BackendDbContext _context;
    private readonly SourceService _sourceService;

    public SourceServiceTests()
    {
        var options = new DbContextOptionsBuilder<BackendDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BackendDbContext(options);
        _sourceService = new SourceService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task AddSource_ShouldAddItem()
    {
        await _sourceService.AddSource(1, DateTime.Now, DateTime.Now, 10, 5);

        var result = await _sourceService.ListSources();

        Assert.Single(result);
    }

    [Fact]
    public async Task AddSource_ShouldThrow_WhenDuplicateId()
    {
        await _sourceService.AddSource(1, DateTime.Now, DateTime.Now, 10, 5);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sourceService.AddSource(1, DateTime.Now, DateTime.Now, 20, 10)
        );
    }

    [Fact]
    public async Task AddSource_ShouldThrow_WhenHeatNegative()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sourceService.AddSource(1, DateTime.Now, DateTime.Now, -10, 5)
        );
    }

    [Fact]
    public async Task AddSource_ShouldThrow_WhenFromAfterTo()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sourceService.AddSource(1, DateTime.Now, DateTime.Now.AddDays(-1), 10, 5)
        );
    }


    [Fact]
    public async Task ListSources_ShouldReturnEmpty_WhenNoItems()
    {
        var result = await _sourceService.ListSources();

        Assert.Empty(result);
    }

    [Fact]
    public async Task ListByMonth_ShouldReturnCorrectItems()
    {
        var date = new DateTime(2025, 5, 1);

        await _sourceService.AddSource(1, date, date, 10, 5);
        await _sourceService.AddSource(2, new DateTime(2025, 6, 1), date, 20, 10);

        var result = await _sourceService.ListByMonth(5);

        Assert.Single(result);
    }

    [Fact]
    public async Task ListByMonth_ShouldThrow_WhenInvalidMonth()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sourceService.ListByMonth(13)
        );
    }

    [Fact]
    public async Task AddSources_ShouldAddMultipleItems()
    {
        var sources = new List<Source>
        {
            new Source { Id = 1, TimeFrom = DateTime.Now, TimeTo = DateTime.Now, HeatDemand = 10, ElectricityPrice = 5 },
            new Source { Id = 2, TimeFrom = DateTime.Now, TimeTo = DateTime.Now, HeatDemand = 20, ElectricityPrice = 10 }
        };

        await _sourceService.AddSources(sources);

        var result = await _sourceService.ListSources();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task AddSources_ShouldThrow_WhenListIsEmpty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sourceService.AddSources(new List<Source>())
        );
    }

    [Fact]
    public async Task AddSources_ShouldThrow_WhenListIsNull()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _sourceService.AddSources(null)
        );
    }


    [Fact]
    public async Task UpdateSource_ShouldUpdateCorrectly()
    {
        await _sourceService.AddSource(1, DateTime.Now, DateTime.Now, 10, 5);

        await _sourceService.UpdateSource(1, DateTime.Now, DateTime.Now, 50, 20);

        var result = await _sourceService.ListSources();

        Assert.Equal(50, result.First().HeatDemand);
        Assert.Equal(20, result.First().ElectricityPrice);
    }

    [Fact]
    public async Task UpdateSource_ShouldThrow_WhenNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sourceService.UpdateSource(999, DateTime.Now, DateTime.Now, 10, 5)
        );
    }

    [Fact]
    public async Task DeleteSource_ShouldRemoveItem()
    {
        await _sourceService.AddSource(1, DateTime.Now, DateTime.Now, 10, 5);

        await _sourceService.DeleteSource(1);

        var result = await _sourceService.ListSources();

        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteSource_ShouldThrow_WhenNotFound()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sourceService.DeleteSource(999)
        );
    }

    [Fact]
    public async Task DeleteSource_ShouldThrow_WhenDeletedTwice()
    {
        await _sourceService.AddSource(1, DateTime.Now, DateTime.Now, 10, 5);

        await _sourceService.DeleteSource(1);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sourceService.DeleteSource(1)
        );
    }
}