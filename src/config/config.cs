using CounterStrikeSharp.API.Core;

namespace AdvancedWeaponSystem;

public class Config : BasePluginConfig
{
    public Dictionary<string, WeaponData> WeaponDatas { get; set; } = [];

    public class WeaponData
    {
        public string Weapon { get; set; } = string.Empty;
        public int? Clip { get; set; }
        public int? Ammo { get; set; }
        public bool? BlockUsing { get; set; }
        public bool? IgnorePickUpFromBlockUsing { get; set; }
        public bool? ReloadAfterShoot { get; set; }
        public bool? UnlimitedAmmo { get; set; }
        public bool? UnlimitedClip { get; set; }
        public bool? OnlyHeadshot { get; set; }
        public string? ViewModel { get; set; }
        public string? WorldModel { get; set; }
        public List<string> AdminFlagsToIgnoreBlockUsing { get; set; } = [];
        public Dictionary<int, int> WeaponQuota { get; set; } = [];

        // 🔹 Novo polje za restrikcije po mapama
        public Dictionary<string, Dictionary<int, int>> MapSpecificQuota { get; set; } = [];

        public string? Damage { get; set; }
    }
}
