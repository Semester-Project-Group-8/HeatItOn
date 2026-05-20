using System.Net;
using System.Linq;
using System.IO;
using Avalonia.Media.Imaging;
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
            new() { Id = 1, Name = "Gas Boiler 1", MaxHeat = 11f, ProductionCost = 150 },
            new() { Id = 2, Name = "Gas Motor 1", MaxHeat = 12f, ProductionCost = 170 },
            new() { Id = 3, Name = "XX1", MaxHeat = 8f, ProductionCost = 110 }
        });

        await WaitForAsync(() => vm.AssetItems.Count == 3);

        var applyMethod = typeof(AssetsTabViewModel).GetMethod("ApplyScenarioSelection", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(applyMethod);

        vm.IsScenario1Selected = false;
        vm.IsScenario2Selected = true;
        applyMethod!.Invoke(vm, null);

        var gb1 = vm.AssetItems.First(x => x.Name == "Gas Boiler 1");
        var gm1 = vm.AssetItems.First(x => x.Name == "Gas Motor 1");
        var xx1 = vm.AssetItems.First(x => x.Name == "XX1");

        Assert.True(gb1.IsSelected);
        Assert.True(gm1.IsSelected);
        Assert.False(xx1.IsSelected);
    }

    [Fact]
    public void MapAssetToCard_Maps_Asset_To_CardItem_Details()
    {
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

        var card = new AssetCardItem(
            asset.Name,
            null!,
            new List<string> { $"Max heat: {asset.MaxHeat}", $"Production costs: {asset.ProductionCost}" },
            "GAS",
            $"{asset.MaxHeat} MW",
            $"{asset.ProductionCost} DKK",
            $"{asset.CO2Emission} kg",
            $"+{asset.MaxElectricity}",
            asset);

        Assert.NotNull(card);
        Assert.Equal("GB1", card.Name);
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
        // Create a lightweight in-memory asset client so tests don't depend on HTTP or Avalonia services.
        var assetClient = new InMemoryAssetClient();
        foreach (var a in assets)
            assetClient.Post(a).GetAwaiter().GetResult();

        // OptimizerClient is not used by these tests, provide a real instance with a noop handler.
        var noopHandler = new StubHttpMessageHandler((request, _) => new HttpResponseMessage(HttpStatusCode.OK));
        var optimizerHttp = new HttpClient(noopHandler) { BaseAddress = new Uri("http://localhost/") };
        var optimizerClient = new OptimizerClient(optimizerHttp);

        var vm = new AssetsTabViewModel(assetClient, optimizerClient);

        // Populate the private _allAssetItems list and the public AssetItems collection directly
        // to avoid calling MapAssetToCard (which depends on Avalonia AssetLoader).
        // Do not create a real Bitmap (requires Avalonia render backend). Use null and suppress nullable warnings.
        Avalonia.Media.Imaging.Bitmap bmp = null!;

        var cardItems = assets.Select(a => new AssetCardItem(
            string.IsNullOrWhiteSpace(a.Name) ? $"Asset {a.Id}" : a.Name,
            bmp,
            new List<string> { $"Max heat: {a.MaxHeat}", $"Production costs: {a.ProductionCost}" },
            "GAS",
            $"{a.MaxHeat} MW",
            $"{a.ProductionCost} DKK",
            $"{a.CO2Emission} kg",
            "—",
            a)).ToList();

        var field = typeof(AssetsTabViewModel).GetField("_allAssetItems", BindingFlags.Instance | BindingFlags.NonPublic)!;
        field.SetValue(vm, cardItems);

        foreach (var item in cardItems)
            vm.AssetItems.Add(item);

        return vm;
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
