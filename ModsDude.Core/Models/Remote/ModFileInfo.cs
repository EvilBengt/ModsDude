using System.Text.Json.Serialization;

namespace ModsDude.Core.Models.Remote;

public class ModFileInfo
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; } = null!;
}
