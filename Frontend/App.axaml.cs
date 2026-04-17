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
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:8080/")
            };

            httpClient.BaseAddress = new Uri("http://localhost:8080/");

            var sourceClient = new SourceClient(httpClient);
            var resultListClient = new ResultListClient(httpClient);
            var assetClient = new AssetClient(httpClient);

            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins

            DisableAvaloniaDataAnnotationValidation();

            desktop.MainWindow = new MainWindow(new ResultListClient(httpClient))
            {
                DataContext = new MainWindowViewModel(sourceClient, assetClient),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}