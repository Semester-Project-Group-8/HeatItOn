using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Frontend.Data;
using Frontend.Models;
using Frontend.ViewModels;

namespace Frontend.Views;

public partial class MainWindow : Window
{
    MainWindowViewModel? ViewModel => (MainWindowViewModel)DataContext!;
    public MainWindow()
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

    private void DeleteAssetButtonClick(object? sender, RoutedEventArgs e)
    {
        
    }

    private async void DeleteButtonClick(object? sender, RoutedEventArgs e)
    {
        var result = await ViewModel.SourceClient.Delete(6001);
        GetSources();
    }

    private void AddAssetButtonClick(object? sender, RoutedEventArgs e)
    {
    }

    private async void AddSourceButtonClick(object? sender, RoutedEventArgs e)
    {
        Source source = new Source
        (
            6001,
            DateTime.Parse("2026-02-12T16:00Z"),
            DateTime.Parse("2026-02-12T17:00Z"),
            8.6f,
            730
        );
        
        var result = await ViewModel.SourceClient.Post(source);
        GetSources();
    }
}