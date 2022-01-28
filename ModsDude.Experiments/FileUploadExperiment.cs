using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModsDude.Core;
using ModsDude.Core.Services;

namespace ModsDude.Experiments;

internal static class FileUploadExperiment
{
    public static async Task Run()
    {
        Remote remote = new();

        FileStream fileStream = File.OpenRead(@"D:\Misc\Farming Simulator 2022 Fake Mods Folder\FS22_animalLimitIncreaser64.zip");

        await remote.UploadMod(fileStream, "fakehash");
    }
}
