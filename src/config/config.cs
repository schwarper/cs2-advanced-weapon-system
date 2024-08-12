using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace AdvancedWeaponSystem;

public class Config : BasePluginConfig
{
    public class WeaponData
    {
        public int? Clip { get; set; }
        public int? Ammo { get; set; }
        public bool? BlockUsing { get; set; }
        public bool? ReloadAfterShoot { get; set; }
        public bool? UnlimitedAmmo { get; set; }
        public bool? UnlimitedClip { get; set; }
        public string? Model { get; set; }
        public string[]? AdminFlagToIgnoreBlockUsing { get; set; }
    }

    [JsonPropertyName("Weapons")]
    public Dictionary<string, WeaponData> WeaponDataList { get; set; } = [];
}