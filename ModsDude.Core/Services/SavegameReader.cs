using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ModsDude.Core.Services;

public class SavegameReader
{
    private const string _gameFolder = @"My Games\FarmingSimulator2022";


    public IEnumerable<string> GetFilenames(string savegame)
    {
        string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        string folder = Path.Combine(documentsFolder, _gameFolder, savegame);
        string filePath = Path.Combine(folder, "careerSavegame.xml");

        XElement careerSavegame = XElement.Load(filePath);

        return careerSavegame.Descendants("mod").Select(mod => mod.Attribute("modName")!.Value + ".zip");
    }
}
