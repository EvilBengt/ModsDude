using ModsDude.Core.Models.Remote;
using ModsDude.Core.Models.UpdatePusher;
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
using ModsDude.WPF.Utils;
using Microsoft.Win32;

namespace ModsDude.WPF.ViewModels;

internal class MainWindowViewModel : ViewModel
{
    private readonly Remote _remote;
    private readonly SavegameReader _savegameReader;
    private readonly ProfileEditorInitializer _profileEditorInitializer;
    private readonly ProfileActivator _profileActivator;
    private readonly UpdatePusher _updatePusher;


    /// <summary>
    /// Design-time only constructor!
    /// </summary>
    public MainWindowViewModel()
    {
        _remote = null!;
        _savegameReader = null!;
        _profileEditorInitializer = null!;
        _profileActivator = null!;
        _updatePusher = null!;

        RefreshProfilesCommand = null!;
        CreateProfileCommand = null!;
        RemoveProfileCommand = null!;
        UpdateProfileFromSavegameCommand = null!;
        OpenProfileEditorCommand = null!;
        RenameProfileCommand = null!;
        ActivateProfileCommand = null!;
        ClearAndActivateProfileCommand = null!;
        PushUpdatesCommand = null!;
        RemoveUnusedFromRemoteCommand = null!;

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
    public MainWindowViewModel(Remote remote, SavegameReader savegameReader, ProfileEditorInitializer profileEditorInitializer, ProfileActivator profileActivator, UpdatePusher updatePusher)
    {
        _remote = remote;
        _savegameReader = savegameReader;
        _profileEditorInitializer = profileEditorInitializer;
        _profileActivator = profileActivator;
        _updatePusher = updatePusher;
        RefreshProfilesCommand = new AsyncRelayCommand(RefreshProfiles, OnAsyncException);
        CreateProfileCommand = new AsyncRelayCommand(CreateProfile, OnAsyncException)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(CreateProfileName) == false
        };
        RemoveProfileCommand = new AsyncRelayCommand(RemoveProfile, OnAsyncException);
        UpdateProfileFromSavegameCommand = new AsyncRelayCommand(UpdateProfileFromSavegame, OnAsyncException)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(SelectedSavegame) == false
        };
        OpenProfileEditorCommand = new(OpenProfileEditor);
        RenameProfileCommand = new(RenameProfile, OnAsyncException)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(RenameProfileName) == false
        };
        ActivateProfileCommand = new(() => ActivateProfile(false), OnAsyncException);
        ClearAndActivateProfileCommand = new(() => ActivateProfile(true), OnAsyncException);
        PushUpdatesCommand = new(PushUpdates, OnAsyncException);
        RemoveUnusedFromRemoteCommand = new(RemoveUnusedFromRemote, OnAsyncException);


        RefreshProfilesCommand.Execute(null);
        Savegames = InitializeSavegameList();
    }


    public AsyncRelayCommand RefreshProfilesCommand { get; }
    public AsyncRelayCommand CreateProfileCommand { get; }
    public AsyncRelayCommand RemoveProfileCommand { get; }
    public AsyncRelayCommand UpdateProfileFromSavegameCommand { get; }
    public RelayCommand OpenProfileEditorCommand { get; }
    public AsyncRelayCommand RenameProfileCommand { get; }
    public AsyncRelayCommand ActivateProfileCommand { get; }
    public AsyncRelayCommand ClearAndActivateProfileCommand { get; }
    public AsyncRelayCommand PushUpdatesCommand { get; }
    public AsyncRelayCommand RemoveUnusedFromRemoteCommand { get; }


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

    private string _createProfileName = "";
    public string CreateProfileName
    {
        get
        {
            return _createProfileName;
        }
        set
        {
            _createProfileName = value;
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

    private string _renameProfileName = "";
    public string RenameProfileName
    {
        get
        {
            return _renameProfileName;
        }
        set
        {
            _renameProfileName = value;
            OnPropertyChanged();
            RenameProfileCommand.OnCanExecuteChanged();
        }
    }

    public ProgressBarViewModel ApplyProfileProgressBarViewModel { get; set; } = new();
    public ProgressBarViewModel PushUpdatesProgressBarViewModel { get; set; } = new();


    private async Task RefreshProfiles()
    {
        Profiles = new(await _remote.FetchProfiles());
    }

    private async Task CreateProfile()
    {
        await _remote.CreateProfile(CreateProfileName);
        CreateProfileName = "";
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

    private async Task RenameProfile()
    {
        if (SelectedProfile is null)
        {
            return;
        }

        await _remote.RenameProfile(SelectedProfile, RenameProfileName);

        RenameProfileName = "";
        RefreshProfilesCommand.Execute(null);
    }

    private async Task ActivateProfile(bool clearBeforeApplying)
    {
        if (SelectedProfile is null)
        {
            return;
        }

        _profileActivator.HashingStarted += (count) =>
        {
            ApplyProfileProgressBarViewModel.Maximum = count;
        };

        _profileActivator.HashedFile += (size) =>
        {
            ApplyProfileProgressBarViewModel.Value += size;
        };

        (float downloadSize, IEnumerable<string> downloads) = await _profileActivator.ActivateFromLocal(SelectedProfile, clearBeforeApplying);

        ApplyProfileProgressBarViewModel.Value = 0;

        string downloadList;
        int downloadCount = downloads.Count();

        if (downloadCount > 20)
        {
            downloadList = string.Join("\n", downloads.Take(20)) + $"\n({downloadCount - 20} more)";
        }
        else
        {
            downloadList = string.Join("\n", downloads);
        }

        bool download = MessageBox.Show(
            $"Do you want to download the following mods ({downloadSize} MB)?\n\n{downloadList}",
            "Download missing mods?",
            MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                == MessageBoxResult.Yes;

        if (!download)
        {
            return;
        }

        // Open download window and start downloading
    }

    private async Task PushUpdates()
    {
        PushUpdatesProgressBarViewModel.Bind(_updatePusher.FileOperation);

        (Update update, IEnumerable<string> completelyMissing) = await _updatePusher.CreateUpdate();

        PushUpdatesProgressBarViewModel.Reset();

        if (completelyMissing.Any())
        {
            string list = string.Join("\n", completelyMissing.Take(20));
            int count = completelyMissing.Count();
            string listFooter = count > 20 ? $"\n...\n({count - 20} more)" : "";

            MessageBox.Show($"The following mods are missing from the server and could not be found locally:\n\n{list}{listFooter}", "Missing mods");
        }

        if (update.Missing.Count() + update.Updates.Count() == 0)
        {
            if (completelyMissing.Any())
            {
                MessageBox.Show("No files to upload.", "No files", MessageBoxButton.OK);
            }
            else
            {
                MessageBox.Show("Remote is up to date.", "Up to date", MessageBoxButton.OK);
            }

            return;
        }

        bool upload = MessageBox.Show(
            $"Do you want to upload\n{update.Missing.Count()} missing and\n{update.Updates.Count()} outdated files?\n\n Total: {update.TotalCount} mods, {update.TotalSize.ToBytesCount()}",
            "Upload",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes;

        if (upload == false)
        {
            return;
        }

        PushUpdatesProgressBarViewModel.Bind(_updatePusher.FileOperation);

        await _updatePusher.PerformUpdate(update);

        PushUpdatesProgressBarViewModel.Reset();
    }

    private async Task RemoveUnusedFromRemote()
    {
        IEnumerable<string> unused = (await _remote.FetchNeededModsList()).Unneeded;

        if (unused.Any() == false)
        {
            MessageBox.Show("No unused mods on remote.", "No unused mods.", MessageBoxButton.OK);
            return;
        }

        string list = string.Join("\n", unused.Take(20));
        int count = unused.Count();
        string listFooter = count > 20 ? $"\n...\n({count} more)" : "";

        bool remove = MessageBox.Show($"Do you want to remove the following mods from remote?\n\n{list}{listFooter}", "Remove mods", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes;

        if (remove == false)
        {
            return;
        }

        foreach (string mod in unused)
        {
            await _remote.RemoveMod(mod);
        }
    }


    private static IEnumerable<string> InitializeSavegameList()
    {
        for (int i = 1; i <= 20; i++)
        {
            yield return $"savegame{i}";
        }
    }
}
