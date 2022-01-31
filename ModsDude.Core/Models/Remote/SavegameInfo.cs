using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModsDude.Core.Models.Remote;

public class SavegameInfo
{
    [JsonPropertyName("checkedOut")]
    public SavegameCheckedOutInfo? CheckedOut { get; set; }
}
