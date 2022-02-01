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
using System.IO;
using ModsDude.Core.Models;
using ModsDude.Core.Models.ProfileActivator;
using ModsDude.Core.Models.Settings;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;

namespace ModsDude.WPF.ViewModels;

internal class MainWindowViewModel : ViewModel
{
    private readonly Remote _remote;
    private readonly SavegameManager _savegameManager;
    private readonly ProfileEditorInitializer _profileEditorInitializer;
    private readonly SettingsWindowInitializer _settingsWindowInitializer;
    private readonly ProfileActivator _profileActivator;
    private readonly UpdatePusher _updatePusher;
    private readonly ModBrowser _modBrowser;
    private readonly ApplicationSettings _settings;


    /// <summary>
    /// Design-time only constructor!
    /// </summary>
    public MainWindowViewModel()
    {
        _remote = null!;
        _savegameManager = null!;
        _profileEditorInitializer = null!;
        _settingsWindowInitializer = null!;
        _profileActivator = null!;
        _updatePusher = null!;
        _modBrowser = null!;
        _settings = null!;

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
        ImportModsCommand = null!;
        RemoveUnusedLocalCommand = null!;
        ClearLocalSavegameCommand = null!;
        OpenSettingsCommand = null!;
        RefreshSavegamesCommand = null!;
        UploadNewSavegameCommand = null!;
        RemoveRemoteSavegameCommand = null!;
        UploadToExistingSavegameCommand = null!;
        DownloadSavegameCommand = null!;
        OpenGameDataFolderCommand = null!;
        OpenModsFolderCommand = null!;
        OpenCacheFolderCommand = null!;

        Profiles = new()
        {
            "profile1.json",
            "profile2.json",
            "profile3.json"
        };
        SelectedProfile = Profiles[0];

        Savegames = InitializeSavegameList();
    }

    /// <summary>
    /// Main constructor
    /// </summary>
    public MainWindowViewModel(Remote remote,
                               SavegameManager savegameManager,
                               ProfileEditorInitializer profileEditorInitializer,
                               SettingsWindowInitializer settingsWindowInitializer,
                               ProfileActivator profileActivator,
                               UpdatePusher updatePusher,
                               ModBrowser modBrowser,
                               ApplicationSettings settings)
    {
        _remote = remote;
        _savegameManager = savegameManager;
        _profileEditorInitializer = profileEditorInitializer;
        _settingsWindowInitializer = settingsWindowInitializer;
        _profileActivator = profileActivator;
        _updatePusher = updatePusher;
        _modBrowser = modBrowser;
        _settings = settings;
        RefreshProfilesCommand = new AsyncRelayCommand(RefreshProfiles, OnException);
        CreateProfileCommand = new AsyncRelayCommand(CreateProfile, OnException)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(CreateProfileName) == false
        };
        RemoveProfileCommand = new AsyncRelayCommand(RemoveProfile, OnException);
        UpdateProfileFromSavegameCommand = new AsyncRelayCommand(UpdateProfileFromSavegame, OnException)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(UpdateProfileSelectedSaveGame) == false
        };
        OpenProfileEditorCommand = new(OpenProfileEditor, OnException);
        RenameProfileCommand = new(RenameProfile, OnException)
        {
            CanExecuteDelegate = () => string.IsNullOrWhiteSpace(RenameProfileName) == false
        };
        ActivateProfileCommand = new(() => ActivateProfile(false), OnException);
        ClearAndActivateProfileCommand = new(() => ActivateProfile(true), OnException);
        PushUpdatesCommand = new(PushUpdates, OnException);
        RemoveUnusedFromRemoteCommand = new(RemoveUnusedFromRemote, OnException);
        ImportModsCommand = new(ImportMods, OnException);
        RemoveUnusedLocalCommand = new(RemoveUnusedLocalMods, OnException);
        ClearLocalSavegameCommand = new(ClearLocalSavegame, OnException)
        {
            CanExecuteDelegate = () => SelectedLocalSavegame is not null
        };
        RefreshSavegamesCommand = new(RefreshSavegames, OnException);
        UploadNewSavegameCommand = new(UploadNewSavegame, OnException)
        {
            CanExecuteDelegate = () => SelectedLocalSavegame is not null && (string.IsNullOrWhiteSpace(NewSavegameSlotName) == false)
        };
        RemoveRemoteSavegameCommand = new(RemoveRemoteSavegame, OnException)
        {
            CanExecuteDelegate = () => SelectedRemoteSavegame is not null
        };
        UploadToExistingSavegameCommand = new(UploadToExistingSavegame, OnException)
        {
            CanExecuteDelegate = () => SelectedRemoteSavegame is not null && SelectedLocalSavegame is not null
        };
        DownloadSavegameCommand = new(DownloadSavegame, OnException)
        {
            CanExecuteDelegate = () => SelectedRemoteSavegame is not null && SelectedLocalSavegame is not null
        };
        OpenSettingsCommand = new(() => _settingsWindowInitializer.Open(() =>
        {
            RefreshProfilesCommand.Execute(null);
            RefreshSavegamesCommand.Execute(null);
        }), OnException);
        OpenGameDataFolderCommand = new(() => Process.Start("explorer", _settings.GetValidGameDataFolder()), OnException);
        OpenModsFolderCommand = new(() => Process.Start("explorer", _settings.GetValidModsFolder()), OnException);
        OpenCacheFolderCommand = new(() => Process.Start("explorer", _settings.GetValidCacheFolder()), OnException);
        

        RefreshProfilesCommand.Execute(null);
        RefreshSavegamesCommand.Execute(null);
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
    public RelayCommand ImportModsCommand { get; }
    public AsyncRelayCommand RemoveUnusedLocalCommand { get; }
    public RelayCommand ClearLocalSavegameCommand { get; }
    public RelayCommand OpenSettingsCommand { get; }
    public AsyncRelayCommand RefreshSavegamesCommand { get; }
    public AsyncRelayCommand UploadNewSavegameCommand { get; }
    public AsyncRelayCommand RemoveRemoteSavegameCommand { get; }
    public AsyncRelayCommand UploadToExistingSavegameCommand { get; }
    public AsyncRelayCommand DownloadSavegameCommand { get; }
    public RelayCommand OpenGameDataFolderCommand { get; }
    public RelayCommand OpenModsFolderCommand { get; }
    public RelayCommand OpenCacheFolderCommand { get; }


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
    private string? _updateProfileSelectedSavegame;
    public string? UpdateProfileSelectedSaveGame
    {
        get
        {
            return _updateProfileSelectedSavegame;
        }
        set
        {
            _updateProfileSelectedSavegame = value;
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

    private string? _selectedLocalSavegame;
    public string? SelectedLocalSavegame
    {
        get
        {
            return _selectedLocalSavegame;
        }
        set
        {
            _selectedLocalSavegame = value;
            OnPropertyChanged();
            ClearLocalSavegameCommand.OnCanExecuteChanged();
            UploadNewSavegameCommand.OnCanExecuteChanged();
            UploadToExistingSavegameCommand.OnCanExecuteChanged();
            DownloadSavegameCommand.OnCanExecuteChanged();
        }
    }

    private ObservableCollection<string>? _remoteSavegames;
    public ObservableCollection<string>? RemoteSavegames
    {
        get
        {
            return _remoteSavegames;
        }
        set
        {
            _remoteSavegames = value;
            OnPropertyChanged();
        }
    }

    private string? _selectedRemoteSavegame;
    public string? SelectedRemoteSavegame
    {
        get
        {
            return _selectedRemoteSavegame;
        }
        set
        {
            _selectedRemoteSavegame = value;
            OnPropertyChanged();
            RemoveRemoteSavegameCommand.OnCanExecuteChanged();
            UploadToExistingSavegameCommand.OnCanExecuteChanged();
            DownloadSavegameCommand.OnCanExecuteChanged();
        }
    }

    private string? _newSavegameSlotName;
    public string? NewSavegameSlotName
    {
        get
        {
            return _newSavegameSlotName;
        }
        set
        {
            _newSavegameSlotName = value;
            OnPropertyChanged();
            UploadNewSavegameCommand.OnCanExecuteChanged();
        }
    }


    public ProgressBarViewModel ApplyProfileProgressBarViewModel { get; set; } = new();
    public ProgressBarViewModel PushUpdatesProgressBarViewModel { get; set; } = new();
    public ProgressBarViewModel RemoveUnusedLocalProgressBarViewModel { get; set; } = new();


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

    private async Task UpdateProfileFromSavegame()
    {
        if (SelectedProfile is null || UpdateProfileSelectedSaveGame is null)
        {
            return;
        }

        await _remote.UpdateProfile(SelectedProfile, _savegameManager.GetNeededMods(UpdateProfileSelectedSaveGame));

        UpdateProfileSelectedSaveGame = null;
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

        ApplyProfileProgressBarViewModel.Bind(_profileActivator.FileOperation);

        (DownloadJob job, IEnumerable<string> completelyMissing) = await _profileActivator.CreateJob(clearBeforeApplying, SelectedProfile);

        ApplyProfileProgressBarViewModel.Reset();

        if (completelyMissing.Any())
        {
            string list = string.Join("\n", completelyMissing.Take(20));
            int count = completelyMissing.Count();
            string listFooter = count > 20 ? $"\n...\n({count - 20} more)" : "";

            MessageBox.Show($"The following required mods could not be found:\n\n{list}{listFooter}", "Missing mods");
        }

        if (job.MissingCount + job.UpdateCount == 0)
        {
            if (completelyMissing.Any())
            {
                MessageBox.Show("No files to download.", "No files");
            }
            else
            {
                MessageBox.Show("No need to download any files.", "No download");
            }

            return;
        }

        bool download = MessageBox.Show(
            $"Do you want to download\n{job.MissingCount} missing and\n{job.UpdateCount} outdated files?\n\n Total: {job.MissingCount + job.UpdateCount} mods, {job.TotalSize.ToBytesCount()}",
            "Download",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes;

        if (download == false)
        {
            return;
        }

        ApplyProfileProgressBarViewModel.Bind(_profileActivator.FileOperation);

        await _profileActivator.PerformJob(job);

        ApplyProfileProgressBarViewModel.Reset();
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
                MessageBox.Show("No files to upload.", "No files");
            }
            else
            {
                MessageBox.Show("Remote is up to date.", "Up to date");
            }

            return;
        }

        bool upload = MessageBox.Show(
            $"Do you want to upload\n{update.Missing.Count()} missing and\n{update.Updates.Count()} updated files?\n\n Total: {update.TotalCount} mods, {update.TotalSize.ToBytesCount()}",
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
            MessageBox.Show("No unused mods found on remote.", "Nothing to remove.", MessageBoxButton.OK);
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

    private void ImportMods()
    {
        try
        {
            OpenFileDialog dialog = new();

            dialog.AddExtension = true;
            dialog.DefaultExt = "zip";
            dialog.InitialDirectory = _modBrowser.DefaultImportPath;
            dialog.Multiselect = true;

            if (dialog.ShowDialog() ?? false)
            {
                foreach (string fullName in dialog.FileNames)
                {
                    _modBrowser.Import(fullName);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task RemoveUnusedLocalMods()
    {
        IEnumerable<NeededMod> needed = (await _remote.FetchNeededModsList()).Needed;
        IEnumerable<FileInfo> existing = _modBrowser.GetActive().Concat(_modBrowser.GetCached());

        IEnumerable<FileInfo> toDelete = existing.Where(file => needed.Any(mod => mod.Name == file.Name) == false);

        if (toDelete.Any() == false)
        {
            MessageBox.Show("No unused mods found locally.", "Nothing to remove", MessageBoxButton.OK);
            return;
        }

        string list = string.Join("\n", toDelete.Take(20).Select(file => file.Name));
        int count = toDelete.Count();
        string listFooter = count > 20 ? $"\n...\n({count} more)" : "";

        bool delete = MessageBox.Show(
            $"Do you want to move the following files to the trash?\n\n{list}{listFooter}", "Remove files?",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes;

        if (!delete)
        {
            return;
        }

        RemoveUnusedLocalProgressBarViewModel.Bind(_modBrowser.FileOperation);

        await _modBrowser.Recycle(toDelete);

        RemoveUnusedLocalProgressBarViewModel.Reset();
    }

    private void ClearLocalSavegame()
    {
        if (SelectedLocalSavegame is null)
        {
            return;
        }

        bool proceed = MessageBox.Show(
            $"Are you sure you want to move all files in {SelectedLocalSavegame} to the trash?", "Clear local savegame",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question)
            == MessageBoxResult.Yes;

        if (proceed == false)
        {
            return;
        }

        _savegameManager.Clear(SelectedLocalSavegame);
    }

    private async Task UploadNewSavegame()
    {
        if (SelectedLocalSavegame is null || string.IsNullOrWhiteSpace(NewSavegameSlotName) || RemoteSavegames is null)
        {
            return;
        }

        if (RemoteSavegames.Contains(NewSavegameSlotName))
        {
            throw new Exception("Name already taken");
        }

        Stream stream = await _savegameManager.PackageAsync(SelectedLocalSavegame);
        await _remote.UploadSavegame(stream, NewSavegameSlotName);
        stream.Dispose();

        _savegameManager.Clear(SelectedLocalSavegame);

        await RefreshSavegamesCommand.ExecuteAsync();

        NewSavegameSlotName = null;
    }
    
    private async Task UploadToExistingSavegame()
    {
        if (SelectedLocalSavegame is null || SelectedRemoteSavegame is null)
        {
            return;
        }

        bool proceed = MessageBox.Show(
            $"Are you sure you want to OVERWRITE {SelectedRemoteSavegame}?", "Overwrite remote savegame",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question)
            == MessageBoxResult.Yes;

        if (proceed == false)
        {
            return;
        }

        SavegameInfo savegameInfo = await _remote.FetchSavegameInfo(SelectedRemoteSavegame);
        if (savegameInfo.CheckedOut is not null && savegameInfo.CheckedOut.Username != _settings.RemoteUsername)
        {
            DateTime time = DateTimeOffset.FromUnixTimeSeconds(savegameInfo.CheckedOut.Timestamp).LocalDateTime;

            proceed = MessageBox.Show(
                $"Savegame {SelectedRemoteSavegame} was last checked out\n\nby: {savegameInfo.CheckedOut.Username}\nat: {time}.\n\n Do you want to proceed?",
                "Download checked out savegame", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning) == MessageBoxResult.Yes;

            if (proceed == false)
            {
                return;
            }
        }

        Stream stream = await _savegameManager.PackageAsync(SelectedLocalSavegame);
        await _remote.UploadSavegame(stream, SelectedRemoteSavegame);
        stream.Dispose();

        _savegameManager.Clear(SelectedLocalSavegame);
    }

    private async Task DownloadSavegame()
    {
        if (SelectedLocalSavegame is null || SelectedRemoteSavegame is null)
        {
            return;
        }

        ClearLocalSavegameCommand.Execute(null);

        SavegameInfo savegameInfo = await _remote.FetchSavegameInfo(SelectedRemoteSavegame);
        if (savegameInfo.CheckedOut is not null && savegameInfo.CheckedOut.Username != _settings.RemoteUsername)
        {
            DateTime time = DateTimeOffset.FromUnixTimeSeconds(savegameInfo.CheckedOut.Timestamp).LocalDateTime;

            bool proceed = MessageBox.Show(
                $"Savegame {SelectedRemoteSavegame} was last checked out\n\nby: {savegameInfo.CheckedOut.Username}\nat: {time}.\n\n Do you want to proceed?",
                "Download checked out savegame", MessageBoxButton.YesNoCancel, MessageBoxImage.Question) == MessageBoxResult.Yes;

            if (proceed == false)
            {
                return;
            }
        }

        await _remote.CheckoutSavegame(SelectedRemoteSavegame);

        Stream stream = await _remote.DownloadSavegame(SelectedRemoteSavegame);
        await _savegameManager.UnpackAsync(stream, SelectedLocalSavegame);
    }

    private async Task RemoveRemoteSavegame()
    {
        if (SelectedRemoteSavegame is null)
        {
            return;
        }

        bool proceed = MessageBox.Show(
            $"Are you sure you want to PERMANENTLY delete {SelectedRemoteSavegame}?", "Clear remote savegame",
            MessageBoxButton.YesNoCancel, MessageBoxImage.Question)
            == MessageBoxResult.Yes;

        if (proceed == false)
        {
            return;
        }

        await _remote.RemoveSavegame(SelectedRemoteSavegame);

        await RefreshSavegamesCommand.ExecuteAsync();
    }

    private async Task RefreshSavegames()
    {
        RemoteSavegames = new(await _remote.FetchSavegames());
    }


    private static IEnumerable<string> InitializeSavegameList()
    {
        for (int i = 1; i <= 20; i++)
        {
            yield return $"savegame{i}";
        }
    }
}
