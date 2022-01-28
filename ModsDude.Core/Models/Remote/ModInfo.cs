using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModsDude.Core.Models.Remote;

public class ModInfo
{
    [JsonPropertyName("hash")]
    public string? Hash { get; set; }

    [JsonPropertyName("size")]
    public float Size { get; set; }
}
