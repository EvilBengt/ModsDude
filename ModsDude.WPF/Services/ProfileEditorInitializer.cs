using ModsDude.Core.Services;
using ModsDude.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModsDude.WPF.Services;

internal class ProfileEditorInitializer
{
    private readonly ModBrowser _modBrowser;
    private readonly Remote _remote;


    public ProfileEditorInitializer(ModBrowser modBrowser, Remote remote)
    {
        _modBrowser = modBrowser;
        _remote = remote;
    }


    public void Open(string profileName)
    {
        ProfileEditorViewModel viewModel = new(_modBrowser, _remote, profileName);
        ProfileEditor window = new()
        {
            DataContext = viewModel,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        window.Show();
    }
}
