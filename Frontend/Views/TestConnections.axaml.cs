using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Frontend.Models;
using Frontend.ViewModels;

namespace Frontend.Views;

public partial class TestConnections : Window
{
    TestViewModel? ViewModel => (TestViewModel)DataContext!;
    public TestConnections()
    {
        InitializeComponent();
    }
    private async void GetSources()
    {
        ListBox list = this.FindControl<ListBox>("resultBox");
        List<Source>?  sources= await ViewModel.SourceClient.GetAll();
        list.Items.Clear();
        if (sources == null)
        {
            ListBoxItem asd = new ListBoxItem()                {
                Content = "No assets found"
            };
            list.Items.Add(asd);
        }
        foreach (Source source in sources)
        {
            ListBoxItem? lbItem = new ListBoxItem
            {
                Content = $"Id: {source.Id} HeatDemand: {source.HeatDemand} ElectricityPrice: {source.ElectricityPrice} TimeFrom: {source.TimeFrom} TimeTo: {source.TimeTo}"
            };
            list.Items.Add(lbItem);
        }
    }

    private async void GetAssets()
    {
        ListBox list = this.FindControl<ListBox>("resultBox");
        List<Asset>?  assets= await ViewModel.AssetClient.GetAll();
        list.Items.Clear();
        if (assets == null)
        {
            ListBoxItem asd = new ListBoxItem()                {
                Content = "No assets found"
            };
            list.Items.Add(asd);
        }
        foreach (Asset asset in assets)
        {
            ListBoxItem? lbItem = new ListBoxItem
            {
                Content = $"Id: {asset.Id} Name: {asset.Name} HeatProduction: {asset.MaxHeat} ElectricityConsumption: {asset.CO2Emission} TimeFrom: {asset.GasConsumption} TimeTo: {asset.MaxElectricity}"
            };
            list.Items.Add(lbItem);
        }
    }

    private async void RequestAllSourceButtonClick(object? sender, RoutedEventArgs e)
    {
        GetSources();
    }

    private async void RequestAllAssetButtonClick(object? sender, RoutedEventArgs e)
    {
        GetAssets();
    }

    private async void DeleteAssetButtonClick(object? sender, RoutedEventArgs e)
    {
        ViewModel.AssetClient.Delete(6001);
        GetAssets();
    }

    private async void DeleteButtonClick(object? sender, RoutedEventArgs e)
    {
        var result = await ViewModel.SourceClient.Delete(6001);
        GetSources();
    }

    private async void AddAssetButtonClick(object? sender, RoutedEventArgs e)
    {
        Asset asset = new Asset();
        asset.Id = 6001;
        asset.Name = "TestAsset";
        asset.MaxHeat = 10.0f;
        asset.ProductionCost = 100;
        asset.CO2Emission = 50;
        asset.GasConsumption = 50;
        asset.OilConsumption = 0;
        asset.MaxElectricity = 20;
        var result = await ViewModel.AssetClient.Post(asset);
        GetAssets();
    }

    private async void AddSourceButtonClick(object? sender, RoutedEventArgs e)
    {
        Source source = new Source();
        source.Id = 6001;
        source.TimeFrom = DateTime.Parse("2026-02-12T16:00Z");
        source.TimeTo = DateTime.Parse("2026-02-12T17:00Z");
        source.HeatDemand = 8.6f;
        source.ElectricityPrice = 730;
        var result = await ViewModel.SourceClient.Post(source);
        GetSources();
    }
}