using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core;

public interface IRemote
{
    Task<IEnumerable<IModDescriptor>> GetModDescriptors();
}
