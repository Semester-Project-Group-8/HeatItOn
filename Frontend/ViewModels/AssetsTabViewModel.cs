using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Frontend.Data;
using Frontend.Models;

namespace Frontend.ViewModels;

public class AssetsTabViewModel : ViewModelBase
{
    private readonly AssetClient _assetClient;
    private string _statusMessage = string.Empty;

    public ObservableCollection<AssetCardItem> AssetItems { get; } = new();
    public bool HasAssets => AssetItems.Count > 0;
    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (SetProperty(ref _statusMessage, value))
                OnPropertyChanged(nameof(HasStatusMessage));
        }
    }

    public AssetsTabViewModel(AssetClient assetClient)
    {
        _assetClient = assetClient;
        AssetItems.CollectionChanged += (_, __) => OnPropertyChanged(nameof(HasAssets));
        LoadFromBackend();
    }

    private async void LoadFromBackend()
    {
        try
        {
            var assets = await _assetClient.GetAll() ?? new List<Asset>();

            AssetItems.Clear();
            if (assets.Count == 0)
            {
                StatusMessage = "No assets available from backend yet.";
                return;
            }

            foreach (var asset in assets)
            {
                AssetItems.Add(MapAssetToCard(asset));
            }

            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            AssetItems.Clear();
            StatusMessage = "Backend unavailable.";
            Console.WriteLine($"Error loading assets: {ex.Message}");
        }
    }

    private static AssetCardItem MapAssetToCard(Asset asset)
    {
        var details = new List<string>
        {
            $"Max heat: {FormatDecimal(asset.MaxHeat)} MW",
            $"Production costs: {asset.ProductionCost} DKK/MWh(th)"
        };

        if (asset.CO2Emission != 0)
            details.Add($"CO2 emissions: {asset.CO2Emission} kg/MWh(th)");

        if (asset.GasConsumption != 0)
            details.Add($"Gas consumption: {FormatDecimal(asset.GasConsumption)} MWh(gas)/MWh(th)");

        if (asset.OilConsumption != 0)
            details.Add($"Oil consumption: {FormatDecimal(asset.OilConsumption)} MWh(oil)/MWh(th)");

        if (asset.MaxElectricity != 0)
            details.Add($"Max electricity: {FormatDecimal(asset.MaxElectricity)} MW");

        return new AssetCardItem(
            string.IsNullOrWhiteSpace(asset.Name) ? $"Asset {asset.Id}" : asset.Name,
            ResolveImagePath(asset.Name),
            details);
    }

    private static string ResolveImagePath(string? assetName)
    {
        var key = (assetName ?? string.Empty).Trim().ToLowerInvariant();

        if (key.StartsWith("gb", StringComparison.Ordinal)) return "/Assets/gb1.png";
        if (key.StartsWith("ob", StringComparison.Ordinal)) return "/Assets/ob1.png";
        if (key.StartsWith("gm", StringComparison.Ordinal)) return "/Assets/gm1.png";
        if (key.StartsWith("eb", StringComparison.Ordinal)) return "/Assets/eb1.png";

        return "/Assets/gb1.png";
    }

    private static string FormatDecimal(float value)
    {
        return value
            .ToString("0.##", CultureInfo.InvariantCulture)
            .Replace('.', ',');
    }
}

public record AssetCardItem(string Name, string ImagePath, IReadOnlyList<string> Details);