using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core.Models.UpdatePusher;

public class Update
{
    public Update(IEnumerable<AvailableMod> missing, IEnumerable<HashedAvailableMod> updates)
    {
        Missing = missing;
        Updates = updates;
        
        HashSize = missing.Sum(mod => mod.File.Length);
        TotalSize = updates.Sum(mod => mod.Mod.File.Length) + HashSize;
        TotalCount = missing.Count() + updates.Count();
    }


    public IEnumerable<AvailableMod> Missing { get; }
    public IEnumerable<HashedAvailableMod> Updates { get; }

    public long HashSize { get; }
    public long TotalSize { get; }
    public long TotalCount { get; }
}
