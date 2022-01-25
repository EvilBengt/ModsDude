using FuzzySharp;
using FuzzySharp.Extractor;
using ModsDude.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.WPF.ViewModels;

internal class FuzzySearcher : ViewModel
{
    private readonly ObservableCollection<string> _input;


    public FuzzySearcher(ObservableCollection<string> input)
    {
        _input = input;
        Output = input;

        _input.CollectionChanged += OnInputChanged;
    }


    private ObservableCollection<string> _output = new();
    public ObservableCollection<string> Output
    {
        get
        {
            return _output;
        }
        set
        {
            _output = value;
            OnPropertyChanged();
        }
    }

    private string _searchString = "";
    public string SearchString
    {
        get
        {
            return _searchString;
        }
        set
        {
            _searchString = value;
            OnPropertyChanged();
            Search();
        }
    }


    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchString))
        {
            Output = _input;
            return;
        }

        IEnumerable<ExtractedResult<string>> results = Process.ExtractTop(SearchString, _input, limit: 100, cutoff: 50);

        Output = new(results.Select(result => result.Value));
    }

    private void OnInputChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Search();
    }
}
