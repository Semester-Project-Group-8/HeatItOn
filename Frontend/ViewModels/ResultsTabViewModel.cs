using Frontend.Data;
using Frontend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frontend.ViewModels;

public class ResultsTabViewModel : ViewModelBase
{
    private readonly ResultClient _resultClient;
    private readonly ResultListClient _resultListClient;
    private readonly OptimizerClient _optimizerClient;

    private List<int>? _resultLists;
    public List<int>? ResultLists
    {
        get => _resultLists;
        set
        {
            _resultLists = value;
            OnPropertyChanged();
        }
    }

    private int? _selectedResultListId;
    public int? SelectedResultListId
    {
        get => _selectedResultListId;
        set
        {
            _selectedResultListId = value;
            OnPropertyChanged();
            _ = LoadResultList();
        }
    }

    private ResultList? _currentResultList;
    public ResultList? CurrentResultList
    {
        get => _currentResultList;
        set
        {
            _currentResultList = value;
            OnPropertyChanged();
        }
    }

    public ResultsTabViewModel(OptimizerClient optimizerClient, ResultClient resultClient, ResultListClient resultListClient)
    {
        _optimizerClient = optimizerClient;
        _resultClient = resultClient;
        _resultListClient = resultListClient;

        _ = LoadResultLists();
    }

    public async Task LoadResultLists()
    {
        try
        {
            ResultLists = await _resultListClient.ListResultLists() ?? new List<int>();
        }
        catch
        {
            ResultLists = new List<int>();
        }
    }

    public async Task LoadResultList()
    {
        if (SelectedResultListId is null)
        {
            CurrentResultList = null;
            return;
        }

        try
        {
            CurrentResultList = await _resultListClient.GetResultListById(SelectedResultListId.Value);
        }
        catch
        {
            CurrentResultList = null;
        }
    }

}