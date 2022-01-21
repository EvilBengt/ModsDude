using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core;

public class ModManager
{
    readonly IRemote _remote;
    readonly IEnumerable<IModDescriptor> _cache;
    readonly IEnumerable<IModDescriptor> _active;


    public ModManager(IRemote remote, IEnumerable<IModDescriptor> cache, IEnumerable<IModDescriptor> active)
    {
        _remote = remote;
        _cache = cache;
        _active = active;
    }
}
