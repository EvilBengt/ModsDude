using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModsDude.Core.Models.Remote;

internal class RemoteProfilesResponse
{
    [JsonPropertyName("profiles")]
    public IEnumerable<string>? Profiles { get; set; }
}
