using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Frontend.Data;
using Frontend.Models;

namespace Frontend.ViewModels;

public class AssetsTabViewModel : ViewModelBase
{
    private readonly AssetClient _assetClient;
    private readonly List<AssetCardItem> _allAssetItems = new();
    private string _statusMessage = string.Empty;
    private bool _isScenario1Selected = true;
    private bool _isScenario2Selected;
    private bool _isCustomScenarioSelected;
    private AddAssetDialogViewModel? _currentDialog;

    public ObservableCollection<AssetCardItem> AssetItems { get; } = new();
    public bool HasAssets => AssetItems.Count > 0;
    public ICommand OpenAddAssetDialogCommand { get; }
    
    public AddAssetDialogViewModel? CurrentDialog
    {
        get => _currentDialog;
        private set => SetProperty(ref _currentDialog, value);
    }

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

    public bool IsScenario1Selected
    {
        get => _isScenario1Selected;
        set
        {
            if (SetProperty(ref _isScenario1Selected, value) && value)
                ApplyScenarioSelection();
        }
    }

    public bool IsScenario2Selected
    {
        get => _isScenario2Selected;
        set
        {
            if (SetProperty(ref _isScenario2Selected, value) && value)
                ApplyScenarioSelection();
        }
    }

    public bool IsCustomScenarioSelected
    {
        get => _isCustomScenarioSelected;
        set
        {
            if (SetProperty(ref _isCustomScenarioSelected, value) && value)
                ApplyScenarioSelection();
        }
    }

    public AssetsTabViewModel(AssetClient assetClient)
    {
        _assetClient = assetClient;
        AssetItems.CollectionChanged += (_, __) => OnPropertyChanged(nameof(HasAssets));
        OpenAddAssetDialogCommand = new RelayCommand(OpenAddAssetDialog);
        LoadFromBackend();
    }

    private void OpenAddAssetDialog()
    {
        var dialogVm = new AddAssetDialogViewModel();
        dialogVm.OnAssetAdded += async (asset) =>
        {
            try
            {
                await _assetClient.Post(asset);
                await LoadFromBackendAsync();
                CurrentDialog = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating asset: {ex.Message}");
            }
        };
        dialogVm.OnCanceled += () =>
        {
            CurrentDialog = null;
        };
        CurrentDialog = dialogVm;
    }

    public void OpenEditAssetDialog(Asset asset)
    {
        var dialogVm = new AddAssetDialogViewModel();
        dialogVm.InitializeForEdit(asset);
        
        dialogVm.OnAssetAdded += async (editedAsset) =>
        {
            try
            {
                await _assetClient.Put(editedAsset);
                await LoadFromBackendAsync();
                CurrentDialog = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating asset: {ex.Message}");
            }
        };
        
        dialogVm.OnCanceled += () =>
        {
            CurrentDialog = null;
        };
        
        dialogVm.OnAssetDeleted += async () =>
        {
            try
            {
                await _assetClient.Delete(asset.Id);
                await LoadFromBackendAsync();
                CurrentDialog = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting asset: {ex.Message}");
            }
        };
        
        CurrentDialog = dialogVm;
    }

    private async void LoadFromBackend()
    {
        await LoadFromBackendAsync();
    }

    private async Task LoadFromBackendAsync()
    {
        try
        {
            var assets = await _assetClient.GetAll() ?? new List<Asset>();

            _allAssetItems.Clear();
            if (assets.Count == 0)
            {
                AssetItems.Clear();
                StatusMessage = "No assets available from backend yet.";
                return;
            }

            foreach (var asset in assets)
            {
                var cardItem = MapAssetToCard(asset);
                cardItem.EditCommand = new RelayCommand(() => OpenEditAssetDialog(asset));
                _allAssetItems.Add(cardItem);
            }

            AssetItems.Clear();
            foreach (var item in _allAssetItems)
                AssetItems.Add(item);

            ApplyScenarioSelection();
            StatusMessage = string.Empty;
        }
        catch (Exception ex)
        {
            AssetItems.Clear();
            StatusMessage = "Backend unavailable.";
            Console.WriteLine($"Error loading assets: {ex.Message}");
        }
    }

    private void ApplyScenarioSelection()
    {
        var selectedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (IsScenario1Selected)
        {
            selectedNames = new HashSet<string>(new[] { "Gas Boiler 1", "Gas Boiler 2", "Gas Boiler 3", "Oil Boiler 1" }, StringComparer.OrdinalIgnoreCase);
        }
        else if (IsScenario2Selected)
        {
            selectedNames = new HashSet<string>(new[] { "Gas Motor 1", "Electric Boiler 1", "Gas Boiler 1", "Gas Boiler 3" }, StringComparer.OrdinalIgnoreCase);
        }

        foreach (var item in _allAssetItems)
            item.IsSelected = selectedNames.Contains(item.Name);
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

        var cardItem = new AssetCardItem(
            string.IsNullOrWhiteSpace(asset.Name) ? $"Asset {asset.Id}" : asset.Name,
                LoadFromResource(string.IsNullOrWhiteSpace(asset.ImageName) ? "placeholder.png" : asset.ImageName),
                details,
                asset);
        return cardItem;
    }

    private static string FormatDecimal(float value)
    {
        return value
            .ToString("0.##", CultureInfo.InvariantCulture)
            .Replace('.', ',');
    }
    private static Bitmap LoadFromResource(string resourceName)
    {
        return new Bitmap(AssetLoader.Open(new Uri($"avares://Frontend/Assets/{resourceName}")));
    }
}

public class AssetCardItem : ViewModelBase
{
    private bool _isSelected;

    public string Name { get; }
    public Bitmap ImagePath { get; }
    public IReadOnlyList<string> Details { get; }
    public Asset? OriginalAsset { get; }
    public ICommand? EditCommand { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public AssetCardItem(string name, Bitmap imagePath, IReadOnlyList<string> details, Asset? originalAsset = null)
    {
        Name = name;
        ImagePath = imagePath;
        Details = details;
        OriginalAsset = originalAsset;
    }
}