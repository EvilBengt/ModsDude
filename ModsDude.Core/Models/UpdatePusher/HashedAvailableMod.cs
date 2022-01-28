using ModsDude.Core.Models.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core.Models.UpdatePusher;

public class HashedAvailableMod
{
    public HashedAvailableMod(AvailableMod mod, string hash)
    {
        Mod = mod;
        Hash = hash;
    }


    public AvailableMod Mod { get; }
    public string Hash { get; }
}
