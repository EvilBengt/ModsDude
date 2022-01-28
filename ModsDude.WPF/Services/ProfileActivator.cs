using ModsDude.Core.Models.Remote;
using ModsDude.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.WPF.Services;

internal class ProfileActivator
{
    private readonly ModBrowser _modBrowser;
    private readonly Remote _remote;


    public ProfileActivator(ModBrowser modBrowser, Remote remote)
    {
        _modBrowser = modBrowser;
        _remote = remote;
    }


    public delegate void HashedFileDelegate(long size);
    public event HashedFileDelegate? HashedFile;

    public delegate void HashingStartedDelegate(long count);
    public event HashingStartedDelegate? HashingStarted;


    public async Task<(float, IEnumerable<string>)> ActivateFromLocal(string profileName, bool clearBeforeApplying)
    {
        IEnumerable<string> profile = await _remote.FetchProfile(profileName);

        IEnumerable<string> activeMods = _modBrowser.GetActive();

        if (clearBeforeApplying)
        {
            _modBrowser.Cache(activeMods.Except(profile));
            activeMods = _modBrowser.GetActive();
        }

        IEnumerable<string> notFoundInCache = _modBrowser.Activate(profile.Except(activeMods));
        activeMods = _modBrowser.GetActive();

        Dictionary<string, ModInfo> modDetails = await _remote.FetchModIndex();

        float downloadSize = 0;
        List<(string, float)> downloadable = new();
        List<string> unavailable = new();

        IEnumerable<KeyValuePair<string, ModInfo>> updatableFiles = modDetails.Where(kv => activeMods.Contains(kv.Key));

        long sizeOfFiles = 0;
        foreach (KeyValuePair<string, ModInfo> kv in updatableFiles)
        {
            sizeOfFiles += _modBrowser.GetFileSizeOfActive(kv.Key);
        }

        OnHashingStarted(sizeOfFiles);

        foreach (KeyValuePair<string, ModInfo> kv in updatableFiles)
        {
            (string filename, ModInfo info) = kv;

            if (CheckForUpdate(filename, info))
            {
                downloadSize += info.Size;
                downloadable.Add((filename, info.Size));
            }
        }

        foreach (string mod in notFoundInCache)
        {
            if (modDetails.TryGetValue(mod, out ModInfo? info))
            {
                downloadSize += info.Size;
                downloadable.Add((mod, info.Size));
            }
            else
            {
                unavailable.Add(mod);
            }
        }

        return (downloadSize, notFoundInCache);
    }


    private bool CheckForUpdate(string filename, ModInfo info)
    {
        throw new NotImplementedException();

        //string localHash = _modBrowser.HashActive(filename);

        //OnHashedFile(_modBrowser.GetFileSizeOfActive(filename));

        //return localHash != info.Hash;
    }

    private void OnHashedFile(long size)
    {
        HashedFile?.Invoke(size);
    }

    private void OnHashingStarted(long total)
    {
        HashingStarted?.Invoke(total);
    }
}
