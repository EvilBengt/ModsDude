using ModsDude.Core.Services;
using ModsDude.WPF.Services;
using ModsDude.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

        //string modsFolderPath = @"D:\Misc\Farming Simulator 2022 Fake Mods Folder";
        string modsFolderPath = @"C:\Users\Anton\Documents\My Games\FarmingSimulator2022\mods";
        string cacheFolderPath = @"D:\Misc\Farming Simulator 2022 Mod Cache";

        Remote remote = new();
        SavegameReader savegameReader = new();
        ModBrowser modBrowser = new(modsFolderPath, cacheFolderPath);
        ProfileEditorInitializer profileEditorInitializer = new(modBrowser, remote);
        ProfileActivator profileActivator = new(modBrowser, remote);
        UpdatePusher updatePusher = new(remote, modBrowser);

        MainWindowViewModel mainWindowViewModel = new(remote, savegameReader, profileEditorInitializer, profileActivator, updatePusher);
        
        Window window = new MainWindow
        {
            DataContext = mainWindowViewModel,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        window.Show();
    }
}
