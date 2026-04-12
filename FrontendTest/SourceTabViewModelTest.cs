using Frontend.ViewModels;
using Frontend.Data;

namespace FrontendTest;

public class SourceTabViewModelIntegrationTests
{
    private readonly SourceClient _sourceClient;

    public SourceTabViewModelIntegrationTests()
    {
        var httpClient =
            new HttpClient
                {
                    BaseAddress = new Uri("http://localhost:8080")
                };

        _sourceClient = new SourceClient(httpClient);
    }

    [Fact]
    public async Task Loads_Files_From_Backend()
    {
        var vm = new SourceTabViewModel(_sourceClient);

        await Task.Delay(500);

        Assert.NotEmpty(vm.Files);
    }

    [Fact]
    public async Task Selecting_File_Filters_Sources()
    {
        var vm = new SourceTabViewModel(_sourceClient);

        await Task.Delay(500);

        var file = vm.Files.First();

        vm.SelectedFile = file;

        Assert.NotEmpty(vm.Sources);
        Assert.All(vm.Sources, s => Assert.Equal(file, s.FileName));
    }

    [Fact]
    public async Task HasSources_Becomes_True_After_File_Selection()
    {
        var vm = new SourceTabViewModel(_sourceClient);

        await Task.Delay(500);

        Assert.False(vm.HasSources);

        vm.SelectedFile = vm.Files.First();

        Assert.True(vm.HasSources);
    }


    [Fact]
    public async Task Builds_Winter_And_Summer_Series_Without_Exception()
    {
        var vm = new SourceTabViewModel(_sourceClient);

        await Task.Delay(500);

        Assert.NotEmpty(vm.Files); 

        vm.SelectedFile = vm.Files.First();

        Assert.NotNull(vm.WinterSeries);
        Assert.NotNull(vm.SummerSeries);
    }
}
