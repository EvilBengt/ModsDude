using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModsDude.Core.Models.Remote;

internal class SavegamesResponse
{
    [JsonPropertyName("savegames")]
    public IEnumerable<string>? Savegames { get; set; }
}
