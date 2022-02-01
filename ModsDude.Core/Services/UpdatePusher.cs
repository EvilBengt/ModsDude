using ModsDude.Core.Models;
using ModsDude.Core.Models.Remote;
using ModsDude.Core.Models.UpdatePusher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core.Services;

public class UpdatePusher
{
    private readonly Remote _remote;
    private readonly ModBrowser _modBrowser;


    public UpdatePusher(Remote remote, ModBrowser modBrowser)
    {
        _remote = remote;
        _modBrowser = modBrowser;
    }


    public FileOperation FileOperation { get; } = new();


    public async Task<(Update Update, IEnumerable<string> Missing)> CreateUpdate()
    {
        NeededMods neededMods = await _remote.FetchNeededModsList();

        IEnumerable<NeededMod> missing = neededMods.Needed.Where(mod => mod.FileInfo is null);
        IEnumerable<AvailableMod> missingAvailable = Available(missing);

        IEnumerable<NeededMod> notMissing = neededMods.Needed.Except(missing);
        IEnumerable<AvailableMod> notMissingAvailable = Available(notMissing);
        IEnumerable<HashedAvailableMod> updates = await CheckForUpdates(notMissingAvailable);

        IEnumerable<string> completelyMissing = missing.Except(missingAvailable.Select(mod => mod.Remote)).Select(mod => mod.Name);

        return (new(missingAvailable, updates), completelyMissing);
    }

    public async Task PerformUpdate(Update update)
    {
        FileOperation.OnStart(update.HashSize);

        List<HashedAvailableMod> hashedMissing = new();

        foreach (AvailableMod mod in update.Missing)
        {
            HashedAvailableMod hashed = await HashMod(mod);
            FileOperation.OnIncrement(hashed.Mod.File.Length);

            hashedMissing.Add(hashed);
        }

        FileOperation.OnStart(update.TotalSize);

        _remote.FileOperation.Increment += UpdateFileOperation;

        foreach (HashedAvailableMod mod in update.Updates.Concat(hashedMissing))
        {
            using FileStream stream = File.OpenRead(mod.Mod.File.FullName);

            await _remote.UploadMod(stream, await _modBrowser.Hash(mod.Mod.File.FullName));
        }

        _remote.FileOperation.Increment -= UpdateFileOperation;
    }


    private async Task<IEnumerable<HashedAvailableMod>> CheckForUpdates(IEnumerable<AvailableMod> candidates)
    {
        FileOperation.OnStart(candidates.Sum(candidate => candidate.File.Length));

        List<HashedAvailableMod> updates = new();

        foreach (AvailableMod candidate in candidates)
        {
            string localHash = await _modBrowser.Hash(candidate.File.FullName);

            FileOperation.OnIncrement(candidate.File.Length);

            if (localHash != candidate.Remote.FileInfo.Hash)
            {
                updates.Add(new(candidate, localHash));
            }
        }

        return updates;
    }

    private async Task<HashedAvailableMod> HashMod(AvailableMod mod)
    {
        return new(mod, await _modBrowser.Hash(mod.File.FullName));
    }

    private IEnumerable<AvailableMod> Available(IEnumerable<NeededMod> needed)
    {
        return needed.Select(mod =>
        {
            FileInfo? file = _modBrowser.FindFile(mod.Name);
            if (file is null)
            {
                return null;
            }

            return new AvailableMod(file, mod);
        }).OfType<AvailableMod>();
    }
    
    private void UpdateFileOperation(long bytes)
    {
        FileOperation.OnIncrement(bytes);
    }
}
