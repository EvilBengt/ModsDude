using ModsDude.Core.Models;
using ModsDude.Core.Models.ProfileActivator;
using ModsDude.Core.Models.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModsDude.Core.Utils;

namespace ModsDude.Core.Services;

public class ProfileActivator
{
    private readonly ModBrowser _modBrowser;
    private readonly Remote _remote;


    public ProfileActivator(ModBrowser modBrowser, Remote remote)
    {
        _modBrowser = modBrowser;
        _remote = remote;

        FileOperation = modBrowser.FileOperation;
    }


    public FileOperation FileOperation { get; }


    public async Task<(DownloadJob job, IEnumerable<string> completelyMissing)> CreateJob(bool cacheUnused, string profileName)
    {
        IEnumerable<NeededMod> neededMods = (await _remote.FetchNeededModsList()).Needed;
        IEnumerable<string> profile = await _remote.FetchProfile(profileName);
        neededMods = neededMods.Where(mod => profile.Contains(mod.Name));

        // Cache unused

        if (cacheUnused)
        {
            IEnumerable<FileInfo> activeMods = _modBrowser.GetActive();
            IEnumerable<FileInfo> toCache = activeMods.Where(file => neededMods.Any(mod => mod.Name == file.Name) == false);

            await _modBrowser.CacheAsync(toCache);
        }

        // Move needed mods from cache

        IEnumerable<FileInfo> cached = _modBrowser.GetCached();
        IEnumerable<FileInfo> cachedNeeded = cached.Where(file => neededMods.Any(mod => mod.Name == file.Name));

        await _modBrowser.ActivateAsync(cachedNeeded);

        // Check now active mods for updates

        IEnumerable<AvailableMod> toHash = _modBrowser.GetActive().Select(file =>
        {
            NeededMod? needed = neededMods.FirstOrDefault(mod => mod.Name == file.Name);

            return needed is null ? null : new AvailableMod(file, needed);
        }).OfType<AvailableMod>();

        IEnumerable<HashedAvailableMod> hashedAvailable = await HashFiles(toHash);

        IEnumerable<HashedAvailableMod> outdated = hashedAvailable.Where(mod => mod.Mod.Remote.FileInfo is not null && mod.Hash != mod.Mod.Remote.FileInfo.Hash);

        // Check for further missing mods

        IEnumerable<NeededMod> downloadable = neededMods.Where(mod => mod.FileInfo is not null);
        IEnumerable<NeededMod> missing = neededMods.Where(mod => hashedAvailable.Any(file => file.Mod.Remote.Name == mod.Name) == false);
        IEnumerable<NeededMod> missingDownloadable = downloadable.Intersect(missing);

        DownloadJob job = new(outdated.Select(mod => mod.Mod.Remote), missingDownloadable);
        IEnumerable<string> completelyMissing = missing.Except(missingDownloadable).Select(mod => mod.Name);

        return (job, completelyMissing);
    }

    public async Task PerformJob(DownloadJob job)
    {
        FileOperation.OnStart(job.TotalSize);

        foreach (NeededMod mod in job.Missing.Concat(job.Outdated))
        {
            Stream stream = await _remote.DownloadMod(mod.Name);

            await _modBrowser.SaveStreamAsActiveAsync(stream, mod.Name);
        }
    }


    private async Task<IEnumerable<HashedAvailableMod>> HashFiles(IEnumerable<AvailableMod> mods)
    {
        FileOperation.OnStart(mods.Sum(mod => mod.File.Length));

        List<HashedAvailableMod> hashed = new();

        foreach (AvailableMod mod in mods)
        {
            string hash = await _modBrowser.Hash(mod.File.FullName);

            hashed.Add(new(mod, hash));

            FileOperation.OnIncrement(mod.File.Length);
        }

        return hashed;
    }
}
