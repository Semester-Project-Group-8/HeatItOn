using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Net.Http;
using Avalonia.Markup.Xaml;
using Frontend.Data;
using Frontend.ViewModels;
using Frontend.Views;
using System;
using System.Net.Http;

namespace Frontend;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("http://localhost:8080/");
            SourceClient sourceClient = new SourceClient(httpClient);
            AssetClient assetClient = new AssetClient(httpClient);
            OptimizerClient optimizerClient = new OptimizerClient(httpClient);
            ResultListClient resultList = new ResultListClient(httpClient);


            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            desktop.MainWindow = new MainWindow(new ResultListClient(httpClient))
            {
                DataContext = new MainWindowViewModel(sourceClient,assetClient,optimizerClient,resultList),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}