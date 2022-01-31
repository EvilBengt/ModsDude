using ModsDude.Core.Models.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core.Models;

public class AvailableMod
{
    public AvailableMod(FileInfo file, NeededMod mod)
    {
        File = file;
        Remote = mod;
    }


    public FileInfo File { get; }
    public NeededMod Remote { get; }
}
