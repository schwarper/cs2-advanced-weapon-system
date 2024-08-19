using CounterStrikeSharp.API;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;

namespace AdvancedWeaponSystem;

public static class Config
{
    public static Dictionary<string, WeaponData> WeaponDataList { get; set; } = [];

    public static void Load()
    {
        string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? "";
        string CfgPath = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/plugins/{AssemblyName}";

        LoadConfig($"{CfgPath}/config.toml");
    }

    private static void LoadConfig(string configPath)
    {
        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Configuration file not found: {configPath}");
        }

        string configText = File.ReadAllText(configPath);
        TomlTable model = Toml.ToModel(configText);

        foreach (KeyValuePair<string, object> weapon in model)
        {
            string weaponName = weapon.Key;
            TomlTable weaponAttributes = (TomlTable)weapon.Value;

            WeaponData weaponData = new();

            if (weaponAttributes.TryGetValue("Clip", out object? clip) && clip is long lclip)
            {
                weaponData.Clip = (int)lclip;
            }

            if (weaponAttributes.TryGetValue("Ammo", out object? ammo) && ammo is long lammo)
            {
                weaponData.Ammo = (int)lammo;
            }

            if (weaponAttributes.TryGetValue("BlockUsing", out object? blockUsing) && blockUsing is bool bblockUsing)
            {
                weaponData.BlockUsing = bblockUsing;
            }

            if (weaponAttributes.TryGetValue("ReloadAfterShoot", out object? reloadAfterShoot) && reloadAfterShoot is bool breloadAfterShoot)
            {
                weaponData.ReloadAfterShoot = breloadAfterShoot;
            }

            if (weaponAttributes.TryGetValue("UnlimitedAmmo", out object? unlimitedAmmo) && unlimitedAmmo is bool bunlimitedAmmo)
            {
                weaponData.UnlimitedAmmo = bunlimitedAmmo;
            }

            if (weaponAttributes.TryGetValue("UnlimitedClip", out object? unlimitedClip) && unlimitedClip is bool bunlimitedClip)
            {
                weaponData.UnlimitedClip = bunlimitedClip;
            }

            if (weaponAttributes.TryGetValue("OnlyHeadshot", out object? onlyHeadshot) && onlyHeadshot is bool bonlyHeadshot)
            {
                weaponData.OnlyHeadshot = bonlyHeadshot;
            }

            if (weaponAttributes.TryGetValue("Model", out object? modelValue) && modelValue is string smodel)
            {
                weaponData.Model = smodel;
            }

            if (weaponAttributes.TryGetValue("AdminFlagsToIgnoreBlockUsing", out object? adminFlags) && adminFlags is TomlArray adminFlagsArray)
            {
                foreach (object? flag in adminFlagsArray)
                {
                    weaponData.AdminFlagsToIgnoreBlockUsing.Add(flag!.ToString()!);
                }
            }

            if (weaponAttributes.TryGetValue("WeaponQuota", out object? weaponQuota) && weaponQuota is TomlTable weaponQuotaTable)
            {
                foreach (KeyValuePair<string, object> kvp in weaponQuotaTable)
                {
                    weaponData.WeaponQuota.Add(int.Parse(kvp.Key), int.Parse(kvp.Value.ToString()!));
                }
            }

            if (weaponAttributes.TryGetValue("Damage", out object? damage) && damage is string sdamage)
            {
                weaponData.Damage = sdamage;
            }

            WeaponDataList[weaponName] = weaponData;
        }
    }

    public class WeaponData
    {
        public int? Clip { get; set; }
        public int? Ammo { get; set; }
        public bool? BlockUsing { get; set; }
        public bool? ReloadAfterShoot { get; set; }
        public bool? UnlimitedAmmo { get; set; }
        public bool? UnlimitedClip { get; set; }
        public bool? OnlyHeadshot { get; set; }
        public string? Model { get; set; }
        public List<string> AdminFlagsToIgnoreBlockUsing { get; set; } = [];
        public Dictionary<int, int> WeaponQuota { get; set; } = [];
        public string? Damage { get; set; }
    }
}
