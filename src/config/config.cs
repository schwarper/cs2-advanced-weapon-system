using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace AdvancedWeaponSystem;

public class Config : BasePluginConfig
{
    [JsonPropertyName("weapons")]
    public List<WeaponData> WeaponDataList { get; set; } = [];
}