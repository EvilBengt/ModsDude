using System.Text.Json.Serialization;

namespace ModsDude.Core.Models.Remote;

public class NeededMod
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("fileInfo")]
    public ModFileInfo FileInfo { get; set; } = null!;
}
