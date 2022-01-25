using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModsDude.Core.Models.Remote;

internal class ProfileUpdateRequest
{
    public ProfileUpdateRequest(IEnumerable<string> mods)
    {
        Mods = mods;
    }


    [JsonPropertyName("mods")]
    public IEnumerable<string> Mods { get; set; }
}
