using System;
using System.Windows.Input;

namespace Frontend.ViewModels;

public class ManagerButtonViewModel : ViewModelBase
{
	public ICommand AddNewAssetCommand { get; }
	public ICommand ImportAssetsCommand { get; }
	public ICommand ExportAssetsCommand { get; }
	public ICommand CancelCommand { get; }

	public event Action? AddRequested;
	public event Action? ImportRequested;
	public event Action? ExportRequested;
	public event Action? CancelRequested;

	public ManagerButtonViewModel()
	{
		AddNewAssetCommand = new RelayCommand(() => AddRequested?.Invoke());
		ImportAssetsCommand = new RelayCommand(() => ImportRequested?.Invoke());
		ExportAssetsCommand = new RelayCommand(() => ExportRequested?.Invoke());
		CancelCommand = new RelayCommand(() => CancelRequested?.Invoke());
	}
}
