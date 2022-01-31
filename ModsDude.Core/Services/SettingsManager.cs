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


    public SettingsManager(string myAppDataPath)
    {
        _myAppDataPath = myAppDataPath;
    }


    public string GetOrCreateSettingsFilePath()
    {
        if (Directory.Exists(_myAppDataPath) == false)
        {
            Directory.CreateDirectory(_myAppDataPath);
        }

        string filePath = Path.Combine(_myAppDataPath, _settingsFileName);

        if (File.Exists(filePath) == false)
        {
            WriteSettings(new());
        }

        return filePath;
    }

    public ApplicationSettings LoadSettings()
    {
        string filePath = GetOrCreateSettingsFilePath();

        string jsonString = File.ReadAllText(filePath);

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

        File.WriteAllText(GetOrCreateSettingsFilePath(), json);
    }
}
