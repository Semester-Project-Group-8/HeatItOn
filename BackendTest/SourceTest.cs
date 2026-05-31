using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
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

    Source test_source = new Source
    {
        Id = 1, 
        TimeFrom = DateTime.Now, 
        TimeTo = DateTime.Now, 
        HeatDemand = 10, 
        ElectricityPrice = 5
    };

    [Fact]
    public async Task AddSource_PositiveCase()
    {
        await _sourceService.Post(test_source);

        var result = await _sourceService.List();

        Assert.Single(result);
    }

    [Fact]
    public async Task AddSource_NegativeCase()
    {
       await _sourceService.Post(test_source);

        var result = await _sourceService.List();

        Assert.NotEqual(2, result.Count());
    }

     [Fact]
    public async Task AddSource_EdgeCase()
    {
        await _sourceService.Post(test_source);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sourceService.Post(new Source {Id = 1, TimeFrom = DateTime.Now,  TimeTo = DateTime.Now, HeatDemand = 100, ElectricityPrice = 25})
        );
    }

    [Fact]
    public async Task GetSource_PositiveCase()
    {
        await _sourceService.Post(test_source);

        var result = await _sourceService.Get(1);

        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetSource_NegativeCase()
    {
        await _sourceService.Post(test_source);

        var result = await _sourceService.Get(1);

        Assert.NotEqual(1000, result.HeatDemand);
    }

    [Fact]
    public async Task GetSource_EdgeCase()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>( async () =>
        await _sourceService.Get(1));
    }

    [Fact]
    public async Task AddListOfSources_PositiveCase()
    {
        var sources = new List<Source>
        {
            new Source {Id = 1, TimeFrom = DateTime.Now, TimeTo = DateTime.Now, HeatDemand = 10, ElectricityPrice = 3},
            new Source {Id = 2, TimeFrom = DateTime.Now, TimeTo = DateTime.Now, HeatDemand = 100, ElectricityPrice = 30}
        };
        await _sourceService.PostList(sources);
        var result = await _sourceService.List();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task AddListOfSources_NegativeCase()
    {
        var sources = new List<Source>
        {
            new Source {Id = 1, TimeFrom = DateTime.Now, TimeTo = DateTime.Now, HeatDemand = 10, ElectricityPrice = 3},
            new Source {Id = 2, TimeFrom = DateTime.Now, TimeTo = DateTime.Now, HeatDemand = 100, ElectricityPrice = 30}
        };
        await _sourceService.PostList(sources);
        var result = await _sourceService.List();

        Assert.NotEqual(5, result.Count());

    }

    [Fact]
    public async Task AddListOfSources_EdgeCase()
    {
        var sources = new List<Source>{};
        await _sourceService.PostList(sources);

        var result = await _sourceService.List();

        Assert.Empty(result);

    }

    [Fact]
    public async Task ListSources_PositiveCase()
    {
        await _sourceService.Post(test_source);
        var result = await _sourceService.List();

        Assert.Single(result);
    }

    [Fact]
    public async Task ListSources_NegativeCase()
    {
        var result = await _sourceService.List();

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateSource_PositiveCase()
    {
        await _sourceService.Post(test_source);
        await _sourceService.Put(1, new Source {Id = 1, TimeFrom = DateTime.Now, TimeTo = DateTime.Now, HeatDemand = 500, ElectricityPrice = 200});

        var result = await _sourceService.List();

        Assert.Equal(500, result.First().HeatDemand);
        Assert.Equal(200, result.First().ElectricityPrice);
    }

    [Fact]
    public async Task UpdateSource_NrgativeCase()
    {
        await _sourceService.Post(test_source);
        await _sourceService.Put(1, new Source {Id = 1, TimeFrom = DateTime.Now, TimeTo = DateTime.Now, HeatDemand = 500, ElectricityPrice = 200});

        var result = await _sourceService.List();

        Assert.NotEqual(10, result.First().HeatDemand);
        Assert.NotEqual(5, result.First().ElectricityPrice);
    }

    [Fact]
    public async Task UpdateSource_EdgeCase()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sourceService.Put(999, test_source)
        );
    }

    [Fact]
    public async Task DeleteSource_PositiveCase()
    {
        await _sourceService.Post(test_source);

        await _sourceService.Delete(1);

        var result = await _sourceService.List();

        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteSource_EdgeCase()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sourceService.Delete(999)
        );
    }

    [Fact]
    public async Task DeleteSource_EdgeCase2_DeletingTwice()
    {
        await _sourceService.Post(test_source);

        await _sourceService.Delete(1);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _sourceService.Delete(1)
        );
    }
}