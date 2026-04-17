using System;
using System.Windows.Input;
using Frontend.Models;

namespace Frontend.ViewModels;

public class AddAssetDialogViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private float _maxHeat;
    private int _productionCost;
    private int _co2Emission;
    private float _gasConsumption;
    private float _oilConsumption;
    private float _maxElectricity;
    private bool _isEditMode;
    private Asset? _originalAsset;

    public Action<Asset>? OnAssetAdded;
    public Action? OnCanceled;
    public Action? OnAssetDeleted;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public float MaxHeat
    {
        get => _maxHeat;
        set => SetProperty(ref _maxHeat, value);
    }

    public int ProductionCost
    {
        get => _productionCost;
        set => SetProperty(ref _productionCost, value);
    }

    public int CO2Emission
    {
        get => _co2Emission;
        set => SetProperty(ref _co2Emission, value);
    }

    public float GasConsumption
    {
        get => _gasConsumption;
        set => SetProperty(ref _gasConsumption, value);
    }

    public float OilConsumption
    {
        get => _oilConsumption;
        set => SetProperty(ref _oilConsumption, value);
    }

    public float MaxElectricity
    {
        get => _maxElectricity;
        set => SetProperty(ref _maxElectricity, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        private set => SetProperty(ref _isEditMode, value);
    }

    public string DialogTitle => IsEditMode ? "Edit Asset" : "Add New Asset";
    public string SubmitButtonText => IsEditMode ? "Save Changes" : "Add Asset";

    public ICommand SubmitCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand DeleteCommand { get; }

    public AddAssetDialogViewModel()
    {
        SubmitCommand = new RelayCommand(Submit);
        CancelCommand = new RelayCommand(() => OnCanceled?.Invoke());
        DeleteCommand = new RelayCommand(Delete);
    }

    public void InitializeForEdit(Asset asset)
    {
        _originalAsset = asset;
        IsEditMode = true;
        Name = asset.Name;
        MaxHeat = asset.MaxHeat;
        ProductionCost = asset.ProductionCost;
        CO2Emission = asset.CO2Emission;
        GasConsumption = asset.GasConsumption;
        OilConsumption = asset.OilConsumption;
        MaxElectricity = asset.MaxElectricity;
    }

    private void Submit()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return;

        var asset = new Asset
        {
            Id = _originalAsset?.Id ?? 0,
            Name = Name,
            MaxHeat = MaxHeat,
            ProductionCost = ProductionCost,
            CO2Emission = CO2Emission,
            GasConsumption = GasConsumption,
            OilConsumption = OilConsumption,
            MaxElectricity = MaxElectricity
        };

        OnAssetAdded?.Invoke(asset);
    }

    private void Delete()
    {
        OnAssetDeleted?.Invoke();
    }
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute ?? (() => true);
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute();

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
