using ModsDude.Core.Services;
using ModsDude.WPF.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.WPF.ViewModels;

internal class ProfileEditorViewModel : ViewModel
{
    private readonly ModBrowser _modBrowser;
    private readonly Remote _remote;
    private readonly string _profileName;


    /// <summary>
    /// Design-time constructor
    /// </summary>
    public ProfileEditorViewModel()
    {
        _modBrowser = null!;
        _remote = null!;
        _profileName = "Profile name";

        AvailableMods = new ObservableCollection<string>()
        {
            "mod1",
            "mod2",
            "mod3",
            "mod4",
            "mod5"
        };
        EnabledMods = new ObservableCollection<string>()
        {
            "mod6",
            "mod7",
            "mod8",
            "mod9",
            "mod10"
        };
        AvailableSearcher = new(AvailableMods);
        EnabledSearcher = new(EnabledMods);

        ReloadEnabledModsCommand = null!;
        EnableModCommand = null!;
        DisableModCommand = null!;
        SaveChangesCommand = null!;
    }

    /// <summary>
    /// Main constructor
    /// </summary>
    public ProfileEditorViewModel(ModBrowser modBrowser, Remote remote, string profileName)
    {
        AvailableMods = new ObservableCollection<string>();
        EnabledMods = new ObservableCollection<string>();
        _modBrowser = modBrowser;
        _remote = remote;
        _profileName = profileName;
        AvailableSearcher = new(AvailableMods);
        EnabledSearcher = new(EnabledMods);

        ReloadEnabledModsCommand = new(ReloadEnabledMods, OnCommandException);
        EnableModCommand = new(EnableMod)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(SelectedAvailableMod) == false
        };
        DisableModCommand = new(DisableMod)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(SelectedEnabledMod) == false
        };
        SaveChangesCommand = new(SaveChanges, OnCommandException);

        ReloadEnabledModsCommand.Execute(null);
    }


    public AsyncRelayCommand ReloadEnabledModsCommand { get; }
    public RelayCommand EnableModCommand { get; }
    public RelayCommand DisableModCommand { get; }
    public AsyncRelayCommand SaveChangesCommand { get; }

    public string ProfileName => _profileName;

    public ObservableCollection<string> AvailableMods { get; set; }
    public ObservableCollection<string> EnabledMods { get; set; }
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


    private async Task ReloadEnabledMods()
    {
        EnabledMods.Clear();

        foreach (string mod in await _remote.FetchProfile(_profileName))
        {
            EnabledMods.Add(mod);
        }
    }

    private void EnableMod()
    {
        while (SelectedAvailableMod is not null)
        {
            EnabledMods.Insert(0, SelectedAvailableMod);
            AvailableMods.Remove(SelectedAvailableMod);
        }
    }

    private void DisableMod()
    {
        while (SelectedEnabledMod is not null)
        {
            AvailableMods.Insert(0, SelectedEnabledMod);
            EnabledMods.Remove(SelectedEnabledMod);
        }
    }

    private Task SaveChanges()
    {
        return _remote.UpdateProfile(ProfileName, EnabledMods);
    }
}
