using ModsDude.Core.Models.Settings;
using ModsDude.Core.Services;
using ModsDude.WPF.Commands;
using ModsDude.WPF.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModsDude.WPF.ViewModels;

internal class SettingsWindowViewModel : ViewModel
{
    private readonly ApplicationSettings _model;
    private readonly SettingsManager _settingsManager;
    private readonly Action _applyCallback;

    /// <summary>
    /// Design-time only constructor!
    /// </summary>
    public SettingsWindowViewModel()
    {
        _model = null!;
        _settingsManager = null!;
        _applyCallback = null!;

        BrowseGameDataCommand = null!;
        BrowseModsCommand = null!;
        BrowseCacheCommand = null!;
        SaveCommand = null!;
    }

    /// <summary>
    /// Main constructor
    /// </summary>
    public SettingsWindowViewModel(ApplicationSettings applicationSettings, SettingsManager settingsManager, Action applyCallback)
    {
        _model = applicationSettings;
        _settingsManager = settingsManager;
        _applyCallback = applyCallback;
        BrowseGameDataCommand = new(() => Browse("Select Game data folder", (path) => GameDataFolderPath = path), OnException);
        BrowseModsCommand = new(() => Browse("Select Mods folder", (path) => ModsFolderPath = path), OnException);
        BrowseCacheCommand = new(() => Browse("Select Mods Cache folder", (path) => CacheFolderPath = path), OnException);
        SaveCommand = new(ApplyAndSaveChanges, OnException);

        GameDataFolderPath = _model.GameDataFolder;
        ModsFolderPath = _model.ModsFolder;
        CacheFolderPath = _model.CacheFolder;
        RemoteUrl = _model.RemoteUrl;
        RemoteUsername = _model.RemoteUsername;
        RemotePassword = _model.RemotePassword;
    }


    public RelayCommand BrowseGameDataCommand { get; }
    public RelayCommand BrowseModsCommand { get; }
    public RelayCommand BrowseCacheCommand { get; }
    public RelayCommand SaveCommand { get; }


    private string? _gameDataFolderPath;
    public string? GameDataFolderPath
    {
        get
        {
            return _gameDataFolderPath;
        }
        set
        {
            _gameDataFolderPath = value;
            OnPropertyChanged();
        }
    }

    private string? _modsFolderPath;
    public string? ModsFolderPath
    {
        get
        {
            return _modsFolderPath;
        }
        set
        {
            _modsFolderPath = value;
            OnPropertyChanged();
        }
    }

    private string? _cacheFolderPath;
    public string? CacheFolderPath
    {
        get
        {
            return _cacheFolderPath;
        }
        set
        {
            _cacheFolderPath = value;
            OnPropertyChanged();
        }
    }

    private string? _remoteUrl;
    public string? RemoteUrl
    {
        get
        {
            return _remoteUrl;
        }
        set
        {
            _remoteUrl = value;
            OnPropertyChanged();
        }
    }

    private string? _remoteUsername;
    public string? RemoteUsername
    {
        get
        {
            return _remoteUsername;
        }
        set
        {
            _remoteUsername = value;
            OnPropertyChanged();
        }
    }

    private string? _remotePassword;
    public string? RemotePassword
    {
        get
        {
            return _remotePassword;
        }
        set
        {
            _remotePassword = value;
            OnPropertyChanged();
        }
    }


    private void Browse(string description, Action<string> callback)
    {
        using FolderBrowserDialog dialog = new()
        {
            Description = description,
            UseDescriptionForTitle = true,
            InitialDirectory = GameDataFolderPath ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Path.DirectorySeparatorChar
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            callback(dialog.SelectedPath);
        }
    }

    private void ApplyAndSaveChanges()
    {
        _model.GameDataFolder = GameDataFolderPath;
        _model.ModsFolder = ModsFolderPath;
        _model.CacheFolder = CacheFolderPath;
        _model.RemoteUrl = RemoteUrl;
        _model.RemoteUsername = RemoteUsername;
        _model.RemotePassword = RemotePassword;

        _settingsManager.WriteSettings(_model);

        _applyCallback();
    }
}
