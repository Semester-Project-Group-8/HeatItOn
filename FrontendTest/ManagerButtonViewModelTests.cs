using Frontend.ViewModels;

namespace FrontendTest;

public class ManagerButtonViewModelTests
{
    [Fact]
    public void Constructor_Initializes_All_Commands()
    {
        var vm = new ManagerButtonViewModel();

        Assert.NotNull(vm.AddNewAssetCommand);
        Assert.NotNull(vm.ImportAssetsCommand);
        Assert.NotNull(vm.ExportAssetsCommand);
        Assert.NotNull(vm.CancelCommand);
    }

    [Fact]
    public void AddNewAssetCommand__AddRequested()
    {
        var vm = new ManagerButtonViewModel();
        var count = 0;
        vm.AddRequested += () => count++;

        vm.AddNewAssetCommand.Execute(null);

        Assert.Equal(1, count);
    }

    [Fact]
    public void ImportAssetsCommand_ImportRequested()
    {
        var vm = new ManagerButtonViewModel();
        var count = 0;
        vm.ImportRequested += () => count++;

        vm.ImportAssetsCommand.Execute(null);

        Assert.Equal(1, count);
    }

    [Fact]
    public void ExportAssetsCommand_ExportRequested()
    {
        var vm = new ManagerButtonViewModel();
        var raisedCount = 0;
        vm.ExportRequested += () => raisedCount++;

        vm.ExportAssetsCommand.Execute(null);

        Assert.Equal(1, raisedCount);
    }

    [Fact]
    public void CancelCommand_CancelRequested()
    {
        var vm = new ManagerButtonViewModel();
        var raisedCount = 0;
        vm.CancelRequested += () => raisedCount++;

        vm.CancelCommand.Execute(null);

        Assert.Equal(1, raisedCount);
    }

    [Fact]
    public void Commands_Do_Not_Throw_When_No_Subscribers()
    {
        var vm = new ManagerButtonViewModel();

        var exception = Record.Exception(() =>
        {
            vm.AddNewAssetCommand.Execute(null);
            vm.ImportAssetsCommand.Execute(null);
            vm.ExportAssetsCommand.Execute(null);
            vm.CancelCommand.Execute(null);
        });

        Assert.Null(exception);
    }
}
