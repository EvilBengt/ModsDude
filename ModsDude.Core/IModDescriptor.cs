using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModsDude.Core;

public interface IModDescriptor
{
    string Name { get; }
    DateTimeOffset Updated { get; }
}
