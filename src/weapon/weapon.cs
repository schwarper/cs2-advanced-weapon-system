using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using static AdvancedWeaponSystem.Config;

namespace AdvancedWeaponSystem;

public static class Weapon
{
    public static int Price(string weaponName)
    {
        return weaponsList[weaponName].price;
    }

    public static ushort DefIndex(string weaponName)
    {
        return weaponsList[weaponName].defIndex;
    }

    public static bool IsRestricted(CCSPlayerController player, string weaponName, WeaponData weaponData)
    {
        string[] flags = [.. weaponData.AdminFlagsToIgnoreBlockUsing];

        if (flags.Length > 0 && AdminManager.PlayerHasPermissions(player, flags))
        {
            return false;
        }

        if (weaponData.BlockUsing == true)
        {
            return true;
        }

        if (weaponData.WeaponQuota.Count > 0)
        {
            ushort defIndex = DefIndex(weaponName);

            List<CCSPlayerController> players = Utilities.GetPlayers().Where(p => p.Team == player.Team).ToList();
            int playerCount = players.Count;
            int maxWeapons = weaponData.WeaponQuota.OrderBy(kvp => kvp.Key)
                                       .LastOrDefault(kvp => playerCount >= kvp.Key).Value;

            int weaponsCount = players.Sum(player => player.PlayerPawn.Value?.WeaponServices?.MyWeapons.Sum(w => Count(defIndex, w, player)) ?? 0);
            return weaponsCount >= maxWeapons;
        }

        return false;
    }

    private static int Count(ushort defIndex, CHandle<CBasePlayerWeapon> weapon, CCSPlayerController player)
    {
        if (weapon.Value?.AttributeManager.Item.ItemDefinitionIndex is not ushort index || index != defIndex)
        {
            return 0;
        }

        if (player.PlayerPawn.Value?.WeaponServices is not CPlayer_WeaponServices weaponServices)
        {
            return 0;
        }

        int total = 0;

        switch (index)
        {
            case (ushort)ItemDefinition.FRAG_GRENADE:
            case (ushort)ItemDefinition.HIGH_EXPLOSIVE_GRENADE:
                total += weaponServices.Ammo[13];
                break;
            case (ushort)ItemDefinition.FLASHBANG:
                total += weaponServices.Ammo[14];
                break;
            case (ushort)ItemDefinition.SMOKE_GRENADE:
                total += weaponServices.Ammo[15];
                break;
            case (ushort)ItemDefinition.MOLOTOV:
            case (ushort)ItemDefinition.INCENDIARY_GRENADE:
                total += weaponServices.Ammo[16];
                break;
            case (ushort)ItemDefinition.DECOY_GRENADE:
                total += weaponServices.Ammo[17];
                break;
            default:
                total++;
                break;
        }

        return total;
    }

    private static readonly Dictionary<string, (int price, ushort defIndex)> weaponsList = new()
    {
        { "weapon_m4a1", (3100, (ushort)ItemDefinition.M4A4) },
        { "weapon_m4a1_silencer", (2900, (ushort)ItemDefinition.M4A1_S) },
        { "weapon_famas", (2050, (ushort)ItemDefinition.FAMAS) },
        { "weapon_aug", (3300, (ushort)ItemDefinition.AUG) },
        { "weapon_ak47", (2700, (ushort)ItemDefinition.AK_47) },
        { "weapon_galilar", (1800, (ushort)ItemDefinition.GALIL_AR) },
        { "weapon_sg556", (3000, (ushort)ItemDefinition.SG_553) },
        { "weapon_scar20", (5000, (ushort)ItemDefinition.SCAR_20) },
        { "weapon_awp", (4750, (ushort)ItemDefinition.AWP) },
        { "weapon_ssg08", (1700, (ushort)ItemDefinition.SSG_08) },
        { "weapon_g3sg1", (5000, (ushort)ItemDefinition.G3SG1) },
        { "weapon_mp9", (1250, (ushort)ItemDefinition.MP9) },
        { "weapon_mp7", (1500, (ushort)ItemDefinition.MP7) },
        { "weapon_mp5sd", (1500, (ushort)ItemDefinition.MP5_SD) },
        { "weapon_ump45", (1200, (ushort)ItemDefinition.UMP_45) },
        { "weapon_p90", (2350, (ushort)ItemDefinition.P90) },
        { "weapon_bizon", (1400, (ushort)ItemDefinition.PP_BIZON) },
        { "weapon_mac10", (1050, (ushort)ItemDefinition.MAC_10) },
        { "weapon_usp_silencer", (200, (ushort)ItemDefinition.USP_S) },
        { "weapon_hkp2000", (200, (ushort)ItemDefinition.P2000) },
        { "weapon_glock", (200, (ushort)ItemDefinition.GLOCK_18) },
        { "weapon_elite", (300, (ushort)ItemDefinition.DUAL_BERETTAS) },
        { "weapon_p250", (300, (ushort)ItemDefinition.P250) },
        { "weapon_fiveseven", (500, (ushort)ItemDefinition.FIVE_SEVEN) },
        { "weapon_cz75a", (500, (ushort)ItemDefinition.CZ75_AUTO) },
        { "weapon_tec9", (500, (ushort)ItemDefinition.TEC_9) },
        { "weapon_revolver", (600, (ushort)ItemDefinition.R8_REVOLVER) },
        { "weapon_deagle", (700, (ushort)ItemDefinition.DESERT_EAGLE) },
        { "weapon_nova", (1050, (ushort)ItemDefinition.NOVA) },
        { "weapon_xm1014", (2000, (ushort)ItemDefinition.XM1014) },
        { "weapon_mag7", (1300, (ushort)ItemDefinition.MAG_7) },
        { "weapon_sawedoff", (1100, (ushort)ItemDefinition.SAWED_OFF) },
        { "weapon_m249", (5200, (ushort)ItemDefinition.M249) },
        { "weapon_negev", (1700, (ushort)ItemDefinition.NEGEV) },
        { "weapon_taser", (200, (ushort)ItemDefinition.ZEUS_X27) },
        { "weapon_hegrenade", (300, (ushort)ItemDefinition.HIGH_EXPLOSIVE_GRENADE) },
        { "weapon_molotov", (400, (ushort)ItemDefinition.MOLOTOV) },
        { "weapon_incgrenade", (600, (ushort)ItemDefinition.INCENDIARY_GRENADE) },
        { "weapon_smokegrenade", (300, (ushort)ItemDefinition.SMOKE_GRENADE) },
        { "weapon_flashbang", (200, (ushort)ItemDefinition.FLASHBANG) },
        { "weapon_decoy", (50, (ushort)ItemDefinition.DECOY_GRENADE) }
    };
}
