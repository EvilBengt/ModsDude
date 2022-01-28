using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModsDude.Core.Models.Remote;

public class NeededMods
{
    [JsonPropertyName("needed")]
    public IEnumerable<NeededMod> Needed { get; set; } = null!;

    [JsonPropertyName("unneeded")]
    public IEnumerable<string> Unneeded { get; set; } = null!;
}
