using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Frontend.ViewModels;

namespace Frontend.Views;

public partial class ResultsTabView : UserControl
{
    public ResultsTabView()
    {
        InitializeComponent();
    }

    private void OnSearchClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ResultsTabViewModel viewModel)
        {
            var fromDate = SearchFromDatePicker.SelectedDate?.Date ?? DateTime.MinValue;
            var fromTime = SearchFromTimePicker.SelectedTime ?? TimeSpan.Zero;
            var toDate = SearchToDatePicker.SelectedDate?.Date ?? DateTime.MinValue;
            var toTime = SearchToTimePicker.SelectedTime ?? new TimeSpan(23, 59, 59);

            viewModel.ApplySearch(fromDate.Add(fromTime), toDate.Add(toTime));
        }
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ResultsTabViewModel viewModel)
        {
            SearchFromDatePicker.SelectedDate = null;
            SearchFromTimePicker.SelectedTime = null;
            SearchToDatePicker.SelectedDate = null;
            SearchToTimePicker.SelectedTime = null;
            viewModel.ClearSearch();
        }
    }
}