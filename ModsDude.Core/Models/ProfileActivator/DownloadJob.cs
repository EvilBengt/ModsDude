using ModsDude.Core.Models.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core.Models.ProfileActivator;

public class DownloadJob
{
    public DownloadJob(IEnumerable<NeededMod> outdated, IEnumerable<NeededMod> missing)
    {
        Outdated = outdated;
        Missing = missing;

        TotalSize = outdated.Sum(mod => (long)mod.FileInfo.Size) + missing.Sum(mod => (long)mod.FileInfo.Size);
        MissingCount = missing.Count();
        UpdateCount = outdated.Count();
    }


    public IEnumerable<NeededMod> Outdated { get; }
    public IEnumerable<NeededMod> Missing { get; }
    public long TotalSize { get; }
    public int MissingCount { get; }
    public int UpdateCount { get; }
}
