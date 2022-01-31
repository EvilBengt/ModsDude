using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core.Models.Settings;

public class ApplicationSettings
{
    public string? GameDataFolder { get; set; }
    public string? ModsFolder { get; set; }
    public string? CacheFolder { get; set; }
    public string? RemoteUrl { get; set; }
    public string? RemoteUsername { get; set; }
    public string? RemotePassword { get; set; }


    public string GetValidGameDataFolder()
    {
        if (string.IsNullOrWhiteSpace(GameDataFolder))
        {
            throw new Exception("Invalid setting: Game data folder");
        }

        return GameDataFolder;
    }

    public string GetValidModsFolder()
    {
        if (string.IsNullOrWhiteSpace(ModsFolder))
        {
            throw new Exception("Invalid setting: Mods folder");
        }

        return ModsFolder;
    }

    public string GetValidCacheFolder()
    {
        if (string.IsNullOrWhiteSpace(CacheFolder))
        {
            throw new Exception("Invalid setting: Mods cache folder");
        }

        return CacheFolder;
    }

    public string GetValidRemoteUrl()
    {
        if (string.IsNullOrWhiteSpace(RemoteUrl))
        {
            throw new Exception("Invalid setting: Remote URL");
        }

        return RemoteUrl;
    }

    public string GetValidRemoteUsername()
    {
        if (string.IsNullOrWhiteSpace(RemoteUsername))
        {
            throw new Exception("Invalid setting: Remote Username");
        }

        return RemoteUsername;
    }

    public string GetValidRemotePassword()
    {
        if (string.IsNullOrWhiteSpace(RemotePassword))
        {
            throw new Exception("Invalid setting: Remote Password");
        }

        return RemotePassword;
    }
}
