using ChatProcessorTags.Config;
using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace ChatProcessor;

public class TagsConfig : BasePluginConfig
{
    [JsonPropertyName("Tags.SteamID")]
    public Dictionary<string, Tag> SteamID { get; set; } = [];

    [JsonPropertyName("Tags.Group")]
    public Dictionary<string, Tag> Group { get; set; } = [];

    [JsonPropertyName("Tags.Permission")]
    public Dictionary<string, Tag> Permission { get; set; } = [];
}
