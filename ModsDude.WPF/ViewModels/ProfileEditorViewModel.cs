using ModsDude.Core.Models.Remote;
using ModsDude.Core.Services;
using ModsDude.WPF.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.WPF.ViewModels;

internal class ProfileEditorViewModel : ViewModel
{
    private readonly ModBrowser _modBrowser;
    private readonly Remote _remote;
    private readonly string _profileName;
    private readonly ObservableCollection<string> _availableMods;
    private readonly ObservableCollection<string> _enabledMods;


    /// <summary>
    /// Design-time constructor
    /// </summary>
    public ProfileEditorViewModel()
    {
        _modBrowser = null!;
        _remote = null!;
        _profileName = "Profile name";

        _availableMods = new ObservableCollection<string>()
        {
            "mod1",
            "mod2",
            "mod3",
            "mod4",
            "mod5"
        };
        _enabledMods = new ObservableCollection<string>()
        {
            "mod6",
            "mod7",
            "mod8",
            "mod9",
            "mod10"
        };
        AvailableSearcher = new(_availableMods);
        EnabledSearcher = new(_enabledMods);

        EnableModCommand = null!;
        DisableModCommand = null!;
        SaveChangesCommand = null!;
    }

    /// <summary>
    /// Main constructor
    /// </summary>
    public ProfileEditorViewModel(ModBrowser modBrowser, Remote remote, string profileName)
    {
        _availableMods = new ObservableCollection<string>();
        _enabledMods = new ObservableCollection<string>();
        _modBrowser = modBrowser;
        _remote = remote;
        _profileName = profileName;
        AvailableSearcher = new(_availableMods);
        EnabledSearcher = new(_enabledMods);

        EnableModCommand = new(EnableMod)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(SelectedAvailableMod) == false
        };
        DisableModCommand = new(DisableMod)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(SelectedEnabledMod) == false
        };
        SaveChangesCommand = new(SaveChanges, OnAsyncException);

        _availableMods.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(nameof(AvailableCount));
        _enabledMods.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) => OnPropertyChanged(nameof(EnabledCount));

        LoadMods();
    }


    public RelayCommand EnableModCommand { get; }
    public RelayCommand DisableModCommand { get; }
    public AsyncRelayCommand SaveChangesCommand { get; }

    public string ProfileName => _profileName;

    public FuzzySearcher AvailableSearcher { get; }
    public FuzzySearcher EnabledSearcher { get; }

    private string? _selectedAvailableMod;
    public string? SelectedAvailableMod
    {
        get
        {
            return _selectedAvailableMod;
        }
        set
        {
            _selectedAvailableMod = value;
            OnPropertyChanged();
            EnableModCommand.OnCanExecuteChanged();
        }
    }

    private string? _selectedEnabledMod;
    public string? SelectedEnabledMod
    {
        get
        {
            return _selectedEnabledMod;
        }
        set
        {
            _selectedEnabledMod = value;
            OnPropertyChanged();
            DisableModCommand.OnCanExecuteChanged();
        }
    }

    public int EnabledCount => _enabledMods.Count;
    public int AvailableCount => _availableMods.Count;


    private async void LoadMods()
    {
        try
        {
            await LoadEnabledMods();
            await LoadAvailableMods();
        }
        catch (Exception ex)
        {
            OnAsyncException(ex);
            return;
        }
    }

    private async Task LoadEnabledMods()
    {
        foreach (string mod in await _remote.FetchProfile(ProfileName))
        {
            _enabledMods.Add(mod);
        }
    }

    private async Task LoadAvailableMods()
    {
        IEnumerable<string> localAvailableMods;
        IEnumerable<string> remoteAvailableMods;

        try
        {
            localAvailableMods = _modBrowser.GetActive().Union(_modBrowser.GetCached());

            NeededMods neededMods = await _remote.FetchNeededModsList();
            remoteAvailableMods = neededMods.Needed!.Select(mod => mod.Name!).Union(neededMods.Unneeded!);
        }
        catch (NullReferenceException ex)
        {
            OnAsyncException(new Exception("Could not load available mods. Probably invalid response from server.", ex));
            return;
        }
        catch (Exception ex)
        {
            OnAsyncException(ex);
            return;
        }

        foreach (string mod in localAvailableMods.Union(remoteAvailableMods).Except(_enabledMods))
        {
            _availableMods.Add(mod);
        }
    }

    private void EnableMod()
    {
        while (SelectedAvailableMod is not null)
        {
            _enabledMods.Insert(0, SelectedAvailableMod);
            _availableMods.Remove(SelectedAvailableMod);
        }
    }

    private void DisableMod()
    {
        while (SelectedEnabledMod is not null)
        {
            _availableMods.Insert(0, SelectedEnabledMod);
            _enabledMods.Remove(SelectedEnabledMod);
        }
    }

    private Task SaveChanges()
    {
        return _remote.UpdateProfile(ProfileName, _enabledMods);
    }
}
