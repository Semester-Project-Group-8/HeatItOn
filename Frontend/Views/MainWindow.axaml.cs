using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Backend.Models;
using Frontend.Data;

namespace Frontend.Views;

public partial class MainWindow : Window
{
    SourceClient _sourceClient = new SourceClient();
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void RequestAllButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            ListBox list = this.FindControl<ListBox>("resultBox");
            List<Source>?  sources= await _sourceClient.GetAll();
            foreach (Source source in sources)
            {
                ListBoxItem? lbItem = new ListBoxItem();
                lbItem.Content = $"Id: {source.Id} HeatDemand: {source.HeatDemand} ElectricityPrice: {source.ElectricityPrice} TimeFrom: {source.TimeFrom} TimeTo: {source.TimeTo}";
                list.Items.Add(lbItem);
            }
        }
        catch (Exception error)
        {
            throw; // TODO handle exception
        }
    }
}