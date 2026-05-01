using Frontend.Data;
using Frontend.Data.CSV;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Frontend.Models;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

namespace Frontend.ViewModels;

public class AssetsTabViewModel : ViewModelBase
{
    private readonly AssetClient _assetClient;
    private readonly OptimizerClient _optimizerClient;
    private readonly List<AssetCardItem> _allAssetItems = new();
    private string _statusMessage = string.Empty;
    private bool _isScenario1Selected = true;
    private bool _isScenario2Selected;
    private bool _isCustomScenarioSelected;
    private AddAssetDialogViewModel? _currentDialog;
    private ManagerButtonViewModel? _currentManagerDialog;

    public ObservableCollection<AssetCardItem> AssetItems { get; } = new();
    public bool HasAssets => AssetItems.Count > 0;
    public ICommand OpenAddAssetDialogCommand { get; }
    public ICommand OpenManagerButtonViewCommand { get; }
    public ICommand StartOptimizationCommand { get; }
    
    public AddAssetDialogViewModel? CurrentDialog
    {
        get => _currentDialog;
        private set
        {
            if (SetProperty(ref _currentDialog, value))
            {
                OnPropertyChanged(nameof(HasOpenDialog));
                OnPropertyChanged(nameof(IsUiEnabled));
            }
        }
    }

    public ManagerButtonViewModel? CurrentManagerDialog
    {
        get => _currentManagerDialog;
        private set
        {
            if (SetProperty(ref _currentManagerDialog, value))
            {
                OnPropertyChanged(nameof(HasOpenDialog));
                OnPropertyChanged(nameof(IsUiEnabled));
            }
        }
    }

    public bool HasOpenDialog => CurrentDialog != null || CurrentManagerDialog != null;
    public bool IsUiEnabled => !HasOpenDialog;

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

    public AssetsTabViewModel(AssetClient assetClient, OptimizerClient optimizerClient)
    {
        _assetClient = assetClient;
        _optimizerClient = optimizerClient;
        AssetItems.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasAssets));
        OpenAddAssetDialogCommand = new RelayCommand(OpenAddAssetDialog);
        OpenManagerButtonViewCommand = new RelayCommand(OpenManagerButtonView);
        StartOptimizationCommand = new RelayCommand(StartOptimization);
        _ = LoadFromBackendAsync();
    }

    private void OpenManagerButtonView()
    {
        var managerVm = new ManagerButtonViewModel();
        managerVm.AddRequested += () =>
        {
            CurrentManagerDialog = null;
            OpenAddAssetDialog();
        };
        managerVm.ImportRequested += () =>
        {
            CurrentManagerDialog = null;
            _ = ImportAssets();
        };
        managerVm.ExportRequested += () =>
        {
            CurrentManagerDialog = null;
            ExportAssets();
        };
        managerVm.CancelRequested += () =>
        {
            CurrentManagerDialog = null;
        };

        CurrentManagerDialog = managerVm;
    }

    public async void StartOptimization()
    {
        try
        {
            List<Asset> scenarioAssets= new List<Asset>();
            foreach (AssetCardItem item in _allAssetItems)
            {
                if(item is { IsSelected: true, OriginalAsset: not null })
                {
                    scenarioAssets.Add(item.OriginalAsset);
                }
            }
            await _optimizerClient.Optimize(scenarioAssets);
        }
        catch (Exception e)
        {
           Console.WriteLine(e); 
        }
    }
    public async Task ImportAssets()
    {
        CsvHandler.ImportAsset(Path.Combine(AppContext.BaseDirectory,"assets.csv"),_assetClient);
    }

    public void ExportAssets()
    {
        CsvHandler.ExportAsset(Path.Combine(AppContext.BaseDirectory, "assets_export.csv"), _assetClient);
    }

    private void OpenAddAssetDialog()
    {
        var dialogVm = new AddAssetDialogViewModel();
        dialogVm.OnAssetAdded += async void (asset) =>
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
        
        dialogVm.OnAssetAdded += async void (editedAsset) =>
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
        
        dialogVm.OnAssetDeleted += async void () =>
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

    public async Task LoadFromBackendAsync()
    {
        try
        {
            var assets = await _assetClient.GetAll() ?? new List<Asset>();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _allAssetItems.Clear();
                AssetItems.Clear();

                if (assets.Count == 0)
                {
                    StatusMessage = "No assets available from backend yet.";
                    return;
                }

                foreach (var asset in assets)
                {
                    var cardItem = MapAssetToCard(asset);
                    cardItem.EditCommand = new RelayCommand(() => OpenEditAssetDialog(asset));
                    _allAssetItems.Add(cardItem);
                }

                foreach (var item in _allAssetItems)
                    AssetItems.Add(item);

                ApplyScenarioSelection();
                StatusMessage = string.Empty;
            });
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AssetItems.Clear();
                StatusMessage = "Backend unavailable.";
            });
            Console.WriteLine($"Error loading assets: {ex.Message}");
        }
    }

    private void ApplyScenarioSelection()
    {
        var selectedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (IsScenario1Selected)
        {
            selectedNames = new HashSet<string>(["Gas Boiler 1", "Gas Boiler 2", "Gas Boiler 3", "Oil Boiler 1"], StringComparer.OrdinalIgnoreCase);
        }
        else if (IsScenario2Selected)
        {
            selectedNames = new HashSet<string>(["Gas Motor 1", "Electric Boiler 1", "Gas Boiler 1", "Gas Boiler 3"], StringComparer.OrdinalIgnoreCase);
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

        var assetType = DeriveAssetType(asset.Name);
        var electricityDisplay = asset.MaxElectricity == 0 ? "—"
            : $"{(asset.MaxElectricity > 0 ? "+" : "")}{FormatDecimal(asset.MaxElectricity)} MW";

        return new AssetCardItem(
            string.IsNullOrWhiteSpace(asset.Name) ? $"Asset {asset.Id}" : asset.Name,
            LoadFromResource(string.IsNullOrWhiteSpace(asset.ImageName) ? "placeholder.png" : asset.ImageName),
            details,
            assetType,
            $"{FormatDecimal(asset.MaxHeat)} MW",
            $"{asset.ProductionCost} DKK",
            $"{asset.CO2Emission} kg",
            electricityDisplay,
            asset);
    }

    private static string DeriveAssetType(string name)
    {
        if (name.Contains("Motor", StringComparison.OrdinalIgnoreCase)) return "MOTOR";
        if (name.Contains("Electric", StringComparison.OrdinalIgnoreCase)) return "ELECTRIC";
        if (name.Contains("Oil", StringComparison.OrdinalIgnoreCase)) return "OIL";
        return "GAS";
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

    public void Refresh()
    {
        _ = LoadFromBackendAsync();
    }
}

public class AssetCardItem : ViewModelBase
{
    private bool _isSelected;

    private static readonly IBrush SelectedBorderBrush = new SolidColorBrush(Color.Parse("#E06020"));
    private static readonly IBrush UnselectedBorderBrush = new SolidColorBrush(Color.Parse("#E0E0E0"));

    public string Name { get; }
    public Bitmap ImagePath { get; }
    public IReadOnlyList<string> Details { get; }
    public Asset? OriginalAsset { get; }
    public ICommand? EditCommand { get; set; }

    public string AssetType { get; }
    public IBrush TypeAccentBrush { get; }
    public IBrush BannerBrush { get; }
    public string MaxHeatDisplay { get; }
    public string CostDisplay { get; }
    public string CO2Display { get; }
    public string ElectricityDisplay { get; }

    public IBrush CardBorderBrush => _isSelected ? SelectedBorderBrush : UnselectedBorderBrush;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty(ref _isSelected, value))
                OnPropertyChanged(nameof(CardBorderBrush));
        }
    }

    public AssetCardItem(string name, Bitmap imagePath, IReadOnlyList<string> details,
        string assetType, string maxHeatDisplay, string costDisplay,
        string co2Display, string electricityDisplay, Asset? originalAsset = null)
    {
        Name = name;
        ImagePath = imagePath;
        Details = details;
        AssetType = assetType;
        (TypeAccentBrush, BannerBrush) = GetTypeBrushes(assetType);
        MaxHeatDisplay = maxHeatDisplay;
        CostDisplay = costDisplay;
        CO2Display = co2Display;
        ElectricityDisplay = electricityDisplay;
        OriginalAsset = originalAsset;
    }

    private static (IBrush accent, IBrush banner) GetTypeBrushes(string type) => type switch
    {
        "OIL" => (
            new SolidColorBrush(Color.Parse("#8B6040")),
            new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops { new GradientStop(Color.Parse("#D0C8BC"), 0), new GradientStop(Color.Parse("#EDE8E2"), 1) }
            }),
        "MOTOR" => (
            new SolidColorBrush(Color.Parse("#6040A0")),
            new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops { new GradientStop(Color.Parse("#C8C0E0"), 0), new GradientStop(Color.Parse("#E8E4F5"), 1) }
            }),
        "ELECTRIC" => (
            new SolidColorBrush(Color.Parse("#208040")),
            new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops { new GradientStop(Color.Parse("#B8D4C0"), 0), new GradientStop(Color.Parse("#DCF0E3"), 1) }
            }),
        _ => (
            new SolidColorBrush(Color.Parse("#E06020")),
            new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops { new GradientStop(Color.Parse("#BFD9EE"), 0), new GradientStop(Color.Parse("#E5F0F8"), 1) }
            })
    };
}