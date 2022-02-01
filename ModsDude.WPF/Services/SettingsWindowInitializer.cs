using ModsDude.Core.Models.Settings;
using ModsDude.Core.Services;
using ModsDude.WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModsDude.WPF.Services;

internal class SettingsWindowInitializer
{
    private readonly ApplicationSettings _settings;
    private readonly SettingsManager _settingsManager;


    public SettingsWindowInitializer(ApplicationSettings settings, SettingsManager settingsManager)
    {
        _settings = settings;
        _settingsManager = settingsManager;
    }


    public void Open(Action applyCallback)
    {
        SettingsWindowViewModel viewModel = new(_settings, _settingsManager, applyCallback);
        SettingsWindow window = new()
        {
            DataContext = viewModel,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        window.Show();
    }
}
