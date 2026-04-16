using Frontend.Models;
using Frontend.ViewModels;

namespace FrontendTest;

public class AddAssetDialogViewModelTests
{
    [Fact]
    public void InitializeForEdit_Sets_Edit_Mode_And_Copies_Fields()
    {
        var vm = new AddAssetDialogViewModel();
        var asset = new Asset
        {
            Id = 7,
            Name = "GB1",
            MaxHeat = 12.5f,
            ProductionCost = 300,
            CO2Emission = 10,
            GasConsumption = 1.1f,
            OilConsumption = 0.5f,
            MaxElectricity = 6.2f
        };

        vm.InitializeForEdit(asset);

        Assert.True(vm.IsEditMode);
        Assert.Equal("Edit Asset", vm.DialogTitle);
        Assert.Equal("Save Changes", vm.SubmitButtonText);
        Assert.Equal(asset.Name, vm.Name);
        Assert.Equal(asset.MaxHeat, vm.MaxHeat);
        Assert.Equal(asset.ProductionCost, vm.ProductionCost);
        Assert.Equal(asset.CO2Emission, vm.CO2Emission);
        Assert.Equal(asset.GasConsumption, vm.GasConsumption);
        Assert.Equal(asset.OilConsumption, vm.OilConsumption);
        Assert.Equal(asset.MaxElectricity, vm.MaxElectricity);
    }

    [Fact]
    public void SubmitCommand_Invokes_OnAssetAdded_With_Entered_Asset_In_Add_Mode()
    {
        var vm = new AddAssetDialogViewModel();
        Asset? submitted = null;
        vm.OnAssetAdded = a => submitted = a;

        vm.Name = "NewAsset";
        vm.MaxHeat = 22.5f;
        vm.ProductionCost = 480;
        vm.CO2Emission = 50;
        vm.GasConsumption = 1.7f;
        vm.OilConsumption = 0.2f;
        vm.MaxElectricity = 4.4f;

        vm.SubmitCommand.Execute(null);

        Assert.NotNull(submitted);
        Assert.Equal(0, submitted!.Id);
        Assert.Equal("NewAsset", submitted.Name);
        Assert.Equal(22.5f, submitted.MaxHeat);
        Assert.Equal(480, submitted.ProductionCost);
        Assert.Equal(50, submitted.CO2Emission);
        Assert.Equal(1.7f, submitted.GasConsumption);
        Assert.Equal(0.2f, submitted.OilConsumption);
        Assert.Equal(4.4f, submitted.MaxElectricity);
    }

    [Fact]
    public void SubmitCommand_Uses_Original_Id_In_Edit_Mode()
    {
        var vm = new AddAssetDialogViewModel();
        Asset? submitted = null;
        vm.OnAssetAdded = a => submitted = a;

        vm.InitializeForEdit(new Asset { Id = 99, Name = "Old" });
        vm.Name = "Edited";

        vm.SubmitCommand.Execute(null);

        Assert.NotNull(submitted);
        Assert.Equal(99, submitted!.Id);
        Assert.Equal("Edited", submitted.Name);
    }

    [Fact]
    public void CancelCommand_Invokes_OnCanceled()
    {
        var vm = new AddAssetDialogViewModel();
        var canceled = false;
        vm.OnCanceled = () => canceled = true;

        vm.CancelCommand.Execute(null);

        Assert.True(canceled);
    }

    [Fact]
    public void DeleteCommand_Invokes_OnAssetDeleted()
    {
        var vm = new AddAssetDialogViewModel();
        var deleted = false;
        vm.OnAssetDeleted = () => deleted = true;

        vm.DeleteCommand.Execute(null);

        Assert.True(deleted);
    }
}

