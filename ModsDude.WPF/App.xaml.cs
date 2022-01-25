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

        Remote remote = new();
        SavegameReader savegameReader = new();
        ModBrowser modBrowser = new();
        ProfileEditorInitializer profileEditorInitializer = new(modBrowser, remote);

        MainWindowViewModel mainWindowViewModel = new(remote, savegameReader, profileEditorInitializer);
        
        Window window = new MainWindow
        {
            DataContext = mainWindowViewModel,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };
        window.Show();
    }
}
