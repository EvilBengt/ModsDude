using ModsDude.Core.Models.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModsDude.Core.Services;

public class SettingsManager
{
    private const string _settingsFileName = "settings.json";

    private readonly string _myAppDataPath;
    private readonly string _settingsFilePath;


    public SettingsManager(string myAppDataPath)
    {
        _myAppDataPath = myAppDataPath;
        _settingsFilePath = Path.Combine(_myAppDataPath, _settingsFileName);
    }


    public ApplicationSettings LoadSettings()
    {
        CreateSettingsFileIfNotExists();

        string jsonString = File.ReadAllText(_settingsFilePath);

        try
        {
            return JsonSerializer.Deserialize<ApplicationSettings>(jsonString) ?? new();
        }
        catch (JsonException)
        {
            return new();
        }
    }

    public void WriteSettings(ApplicationSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions()
        {
            WriteIndented = true
        });

        File.WriteAllText(_settingsFilePath, json);
    }

    private void CreateSettingsFileIfNotExists()
    {
        if (Directory.Exists(_myAppDataPath) == false)
        {
            Directory.CreateDirectory(_myAppDataPath);
        }

        if (File.Exists(_settingsFilePath) == false)
        {
            WriteSettings(new());
        }
    }
}
