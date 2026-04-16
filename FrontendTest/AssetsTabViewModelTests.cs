using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Frontend.Data;
using Frontend.Models;
using Frontend.ViewModels;

namespace FrontendTest;

public class AssetsTabViewModelTests
{
    [Fact]
    public async Task LoadFromBackend_Loads_Items_Into_AssetItems()
    {
        var vm = CreateViewModelWithAssets(new List<Asset>
        {
            new() { Id = 1, Name = "GB1", MaxHeat = 11f, ProductionCost = 150 },
            new() { Id = 2, Name = "GM1", MaxHeat = 9f, ProductionCost = 200 }
        });

        await WaitForAsync(() => vm.AssetItems.Count == 2);

        Assert.Equal(2, vm.AssetItems.Count);
        Assert.True(vm.HasAssets);
    }

    [Fact]
    public async Task OpenEditAssetDialog_Prefills_And_Saves_Updated_Asset()
    {
        var vm = CreateViewModelWithAssets(new List<Asset>
        {
            new()
            {
                Id = 1,
                Name = "GB1",
                MaxHeat = 11f,
                ProductionCost = 150,
                CO2Emission = 3,
                GasConsumption = 1.1f,
                OilConsumption = 0,
                MaxElectricity = 2f
            }
        });

        await WaitForAsync(() => vm.AssetItems.Count == 1);

        var original = vm.AssetItems[0].OriginalAsset;
        Assert.NotNull(original);

        vm.OpenEditAssetDialog(original!);

        Assert.NotNull(vm.CurrentDialog);
        Assert.True(vm.CurrentDialog!.IsEditMode);
        Assert.Equal("GB1", vm.CurrentDialog.Name);
    }

    [Fact]
    public async Task ApplyScenarioSelection_Selects_Correct_Assets()
    {
        var vm = CreateViewModelWithAssets(new List<Asset>
        {
            new() { Id = 1, Name = "GB1", MaxHeat = 11f, ProductionCost = 150 },
            new() { Id = 2, Name = "GM1", MaxHeat = 12f, ProductionCost = 170 },
            new() { Id = 3, Name = "XX1", MaxHeat = 8f, ProductionCost = 110 }
        });

        await WaitForAsync(() => vm.AssetItems.Count == 3);

        var applyMethod = typeof(AssetsTabViewModel).GetMethod("ApplyScenarioSelection", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(applyMethod);

        vm.IsScenario1Selected = false;
        vm.IsScenario2Selected = true;
        applyMethod!.Invoke(vm, null);

        var gb1 = vm.AssetItems.First(x => x.Name == "GB1");
        var gm1 = vm.AssetItems.First(x => x.Name == "GM1");
        var xx1 = vm.AssetItems.First(x => x.Name == "XX1");

        Assert.True(gb1.IsSelected);
        Assert.True(gm1.IsSelected);
        Assert.False(xx1.IsSelected);
    }

    [Fact]
    public void MapAssetToCard_Maps_Asset_To_CardItem_Details()
    {
        var mapMethod = typeof(AssetsTabViewModel).GetMethod("MapAssetToCard", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(mapMethod);

        var asset = new Asset
        {
            Id = 10,
            Name = "GB1",
            MaxHeat = 13.5f,
            ProductionCost = 450,
            CO2Emission = 12,
            GasConsumption = 1.1f,
            OilConsumption = 0.3f,
            MaxElectricity = 2.4f
        };

        var card = (AssetCardItem)mapMethod!.Invoke(null, new object[] { asset })!;
        Assert.NotNull(card);
        Assert.Equal("GB1", card.Name);
        Assert.Equal("/Assets/gb1.png", card.ImagePath);
        Assert.Equal(asset, card.OriginalAsset);
        Assert.Contains(card.Details, d => d.Contains("Max heat"));
        Assert.Contains(card.Details, d => d.Contains("Production costs"));
    }

    [Fact]
    public void FormatDecimal_Formats_To_Comma_Decimal_String()
    {
        var formatMethod = typeof(AssetsTabViewModel).GetMethod("FormatDecimal", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(formatMethod);

        var formatted = (string)formatMethod!.Invoke(null, new object[] { 12.5f })!;

        Assert.Equal("12,5", formatted);
    }

    [Fact]
    public async Task OpenEditAssetDialog_Delete_Removes_Item_From_UI()
    {
        var vm = CreateViewModelWithAssets(new List<Asset>
        {
            new() { Id = 1, Name = "GB1", MaxHeat = 11f, ProductionCost = 150 }
        });

        await WaitForAsync(() => vm.AssetItems.Count == 1);

        var original = vm.AssetItems[0].OriginalAsset;
        Assert.NotNull(original);

        vm.OpenEditAssetDialog(original!);
        vm.CurrentDialog!.DeleteCommand.Execute(null);

        await WaitForAsync(() => vm.AssetItems.Count == 0);

        Assert.Null(vm.CurrentDialog);
        Assert.Empty(vm.AssetItems);
        Assert.False(vm.HasAssets);
    }

    private static AssetsTabViewModel CreateViewModelWithAssets(List<Asset> assets)
    {
        var json = JsonSerializer.Serialize(assets);
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            if (request.Method == HttpMethod.Get && request.RequestUri is not null && request.RequestUri.AbsolutePath.EndsWith("/Asset", StringComparison.OrdinalIgnoreCase))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }

            if (request.Method == HttpMethod.Delete && request.RequestUri is not null && request.RequestUri.AbsolutePath.StartsWith("/Asset/", StringComparison.OrdinalIgnoreCase))
            {
                assets.RemoveAll(a => request.RequestUri.AbsolutePath.EndsWith($"/{a.Id}", StringComparison.OrdinalIgnoreCase));
                json = JsonSerializer.Serialize(assets);
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        return new AssetsTabViewModel(new AssetClient(httpClient));
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        var timeout = TimeSpan.FromSeconds(5);
        var start = DateTime.UtcNow;
        while (!condition())
        {
            if (DateTime.UtcNow - start > timeout)
                throw new TimeoutException("Condition was not reached in time.");

            await Task.Delay(25);
        }
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request, cancellationToken));
        }
    }
}
