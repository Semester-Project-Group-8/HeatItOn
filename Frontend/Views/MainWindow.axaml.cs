using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Frontend.Data;
using Frontend.Models;

namespace Frontend.Views;

public partial class MainWindow : Window
{
    SourceClient _sourceClient = new SourceClient();
    AssetClient _assetClient = new AssetClient();
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void RequestAllSourceButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            ListBox list = this.FindControl<ListBox>("resultBox");
            List<Source>?  sources= await _sourceClient.GetAll();
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
        catch (Exception error)
        {
            throw; // TODO handle exception
        }
    }

    private async void RequestAllAssetButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            ListBox list = this.FindControl<ListBox>("resultBox");
            List<Asset>?  assets= await _assetClient.GetAll();
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
        catch (Exception error)
        {
            throw; // TODO handle exception
        }
    }
}