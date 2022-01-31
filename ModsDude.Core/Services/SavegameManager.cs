using Microsoft.VisualBasic.FileIO;
using ModsDude.Core.Models;
using ModsDude.Core.Models.Settings;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ModsDude.Core.Services;

public class SavegameManager
{
    private const string _tempFileName = "savegameTemp.zip";

    private readonly string _myAppDataPath;
    private readonly ApplicationSettings _settings;


    public SavegameManager(ApplicationSettings settings, string myAppDataPath)
    {
        _settings = settings;
        _myAppDataPath = myAppDataPath;
    }


    public IEnumerable<string> GetNeededMods(string savegame)
    {
        string gameDataFolderPath = _settings.GetValidGameDataFolder();

        string filePath = Path.Combine(gameDataFolderPath, savegame, "careerSavegame.xml");

        XElement careerSavegame = XElement.Load(filePath);

        return careerSavegame.Descendants("mod").Select(mod => mod.Attribute("modName")!.Value + ".zip");
    }

    public void Clear(string savegame)
    {
        string gameDataFolderPath = _settings.GetValidGameDataFolder();

        IEnumerable<FileInfo> files = new DirectoryInfo(Path.Combine(gameDataFolderPath, savegame)).EnumerateFiles();

        foreach (FileInfo file in files)
        {
            FileSystem.DeleteFile(file.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
        }
    }

    public Task<Stream> PackageAsync(string savegame)
    {
        return Task.Run(() =>
        {
            return Package(savegame);
        });
    }

    public Task UnpackAsync(Stream stream, string savegame)
    {
        return Task.Run(() =>
        {
            Unpack(stream, savegame);
        });
    }


    private Stream Package(string savegame)
    {
        string gameDataFolderPath = _settings.GetValidGameDataFolder();
        string tempFilePath = Path.Combine(_myAppDataPath, _tempFileName);

        DirectoryInfo savegameFolder = new(Path.Combine(gameDataFolderPath, savegame));

        if (savegameFolder.GetFiles().Take(2).Count() < 2)
        {
            throw new Exception($"Local savegame \"{savegame}\" is empty.");
        }

        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }

        ZipFile.CreateFromDirectory(savegameFolder.FullName, tempFilePath);

        return File.OpenRead(tempFilePath);
    }

    private void Unpack(Stream stream, string savegame)
    {
        string gameDataFolderPath = _settings.GetValidGameDataFolder();
        string tempFilePath = Path.Combine(_myAppDataPath, _tempFileName);

        DirectoryInfo savegameFolder = new(Path.Combine(gameDataFolderPath, savegame));

        if (savegameFolder.GetFiles().Take(2).Count() > 1)
        {
            throw new Exception($"Local savegame \"{savegame}\" is not empty.");
        }

        if (File.Exists(tempFilePath))
        {
            File.Delete(tempFilePath);
        }

        FileStream fileStream = File.OpenWrite(tempFilePath);
        stream.CopyTo(fileStream);
        fileStream.Dispose();
        stream.Dispose();

        ZipFile.ExtractToDirectory(tempFilePath, savegameFolder.FullName);
    }
}
