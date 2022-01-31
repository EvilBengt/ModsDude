using ModsDude.Core.Models.Settings;
using ModsDude.Core.Services;
using ModsDude.WPF.Services;
using ModsDude.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ModsDude.WPF;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string modImportPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        string myAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ModsDude");

        SettingsManager settingsManager = new(myAppDataPath);
        ApplicationSettings settings = settingsManager.LoadSettings();

        Remote remote = new(settings);
        SavegameManager savegameManager = new(settings, myAppDataPath);
        ModBrowser modBrowser = new(settings, modImportPath);
        ProfileActivator profileActivator = new(modBrowser, remote);
        UpdatePusher updatePusher = new(remote, modBrowser);
        ProfileEditorInitializer profileEditorInitializer = new(modBrowser, remote);
        SettingsWindowInitializer settingsWindowInitializer = new(settings, settingsManager);

        MainWindowViewModel mainWindowViewModel = new(remote,
                                                      savegameManager,
                                                      profileEditorInitializer,
                                                      settingsWindowInitializer,
                                                      profileActivator,
                                                      updatePusher,
                                                      modBrowser,
                                                      settings);

        Window window = new MainWindow
        {
            DataContext = mainWindowViewModel,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        window.Show();
    }
}
