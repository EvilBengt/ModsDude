using ModsDude.Core.Services;
using ModsDude.WPF.Commands;
using ModsDude.WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ModsDude.WPF.ViewModels;

internal class MainWindowViewModel : ViewModel
{
    private readonly Remote _remote;
    private readonly SavegameReader _savegameReader;
    private readonly ProfileEditorInitializer _profileEditorInitializer;


    /// <summary>
    /// Design-time only constructor!
    /// </summary>
    public MainWindowViewModel()
    {
        _remote = null!;
        _savegameReader = null!;
        _profileEditorInitializer = null!;

        RefreshProfilesCommand = null!;
        CreateProfileCommand = null!;
        RemoveProfileCommand = null!;
        UpdateProfileFromSavegameCommand = null!;
        OpenProfileEditorCommand = null!;

        Profiles = new()
        {
            "profile1.json",
            "profile2.json",
            "profile3.json"
        };

        Savegames = InitializeSavegameList();
    }

    /// <summary>
    /// Main constructor
    /// </summary>
    public MainWindowViewModel(Remote remote, SavegameReader savegameReader, ProfileEditorInitializer profileEditorInitializer)
    {
        _remote = remote;
        _savegameReader = savegameReader;
        _profileEditorInitializer = profileEditorInitializer;
        RefreshProfilesCommand = new AsyncRelayCommand(RefreshProfiles, OnCommandException);
        CreateProfileCommand = new AsyncRelayCommand(CreateProfile, OnCommandException)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(NewProfileName) == false
        };
        RemoveProfileCommand = new AsyncRelayCommand(RemoveProfile, OnCommandException);
        UpdateProfileFromSavegameCommand = new AsyncRelayCommand(UpdateProfileFromSavegame, OnCommandException)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(SelectedSavegame) == false
        };
        OpenProfileEditorCommand = new(OpenProfileEditor);

        RefreshProfilesCommand.Execute(null);

        Savegames = InitializeSavegameList();
    }


    public AsyncRelayCommand RefreshProfilesCommand { get; }
    public AsyncRelayCommand CreateProfileCommand { get; }
    public AsyncRelayCommand RemoveProfileCommand { get; }
    public AsyncRelayCommand UpdateProfileFromSavegameCommand { get; }
    public RelayCommand OpenProfileEditorCommand { get; }


    private ObservableCollection<string>? _profiles;
    public ObservableCollection<string>? Profiles
    {
        get
        {
            return _profiles;
        }
        set
        {
            _profiles = value;
            OnPropertyChanged();
        }
    }

    private string? _selectedProfile;
    public string? SelectedProfile
    {
        get
        {
            return _selectedProfile;
        }
        set
        {
            _selectedProfile = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedProfile));
        }
    }
    public bool HasSelectedProfile => SelectedProfile is not null;

    private string _newProfileName = "";
    public string NewProfileName
    {
        get
        {
            return _newProfileName;
        }
        set
        {
            _newProfileName = value;
            OnPropertyChanged();
            CreateProfileCommand.OnCanExecuteChanged();
        }
    }

    public IEnumerable<string> Savegames { get; }
    private string? _selectedSavegame;
    public string? SelectedSavegame
    {
        get
        {
            return _selectedSavegame;
        }
        set
        {
            _selectedSavegame = value;
            OnPropertyChanged();
            UpdateProfileFromSavegameCommand.OnCanExecuteChanged();
        }
    }


    private async Task RefreshProfiles()
    {
        Profiles = new(await _remote.FetchProfiles());
    }

    private async Task CreateProfile()
    {
        await _remote.CreateProfile(NewProfileName);
        NewProfileName = "";
        RefreshProfilesCommand.Execute(null);
    }

    private async Task RemoveProfile()
    {
        if (string.IsNullOrWhiteSpace(SelectedProfile))
        {
            return;
        }

        await _remote.RemoveProfile(SelectedProfile);
        RefreshProfilesCommand.Execute(null);
    }

    private Task UpdateProfileFromSavegame()
    {
        if (SelectedProfile is null || SelectedSavegame is null)
        {
            return Task.CompletedTask;
        }

        return _remote.UpdateProfile(SelectedProfile, _savegameReader.GetFilenames(SelectedSavegame));
    }

    private void OpenProfileEditor()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        _profileEditorInitializer.Open(SelectedProfile);
    }


    private static IEnumerable<string> InitializeSavegameList()
    {
        for (int i = 1; i <= 20; i++)
        {
            yield return $"savegame{i}";
        }
    }
}
