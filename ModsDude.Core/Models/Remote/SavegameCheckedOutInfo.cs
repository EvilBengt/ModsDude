using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModsDude.Core.Models.Remote;

public class SavegameCheckedOutInfo
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("timestamp")]
    public int Timestamp { get; set; }
}
