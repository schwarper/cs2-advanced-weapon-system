using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using static AdvancedWeaponSystem.Config;

namespace AdvancedWeaponSystem;

public static class Weapon
{
    public static ushort DefIndex(string weaponName)
    {
        return weaponsList[weaponName];
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

            int weaponsCount = players.Sum(player =>
            {
                int selector(CHandle<CBasePlayerWeapon> w) => Count(defIndex, w, player);
                return player.PlayerPawn.Value?.WeaponServices?.MyWeapons.Sum(selector) ?? 0;
            });

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

    private static readonly Dictionary<string, ushort> weaponsList = new()
    {
        { "weapon_m4a1", (ushort)ItemDefinition.M4A4 },
        { "weapon_m4a1_silencer", (ushort)ItemDefinition.M4A1_S },
        { "weapon_famas", (ushort)ItemDefinition.FAMAS },
        { "weapon_aug", (ushort)ItemDefinition.AUG },
        { "weapon_ak47", (ushort)ItemDefinition.AK_47 },
        { "weapon_galilar", (ushort)ItemDefinition.GALIL_AR },
        { "weapon_sg556", (ushort)ItemDefinition.SG_553 },
        { "weapon_scar20", (ushort)ItemDefinition.SCAR_20 },
        { "weapon_awp", (ushort)ItemDefinition.AWP },
        { "weapon_ssg08", (ushort)ItemDefinition.SSG_08 },
        { "weapon_g3sg1", (ushort)ItemDefinition.G3SG1 },
        { "weapon_mp9", (ushort)ItemDefinition.MP9 },
        { "weapon_mp7", (ushort)ItemDefinition.MP7 },
        { "weapon_mp5sd", (ushort)ItemDefinition.MP5_SD },
        { "weapon_ump45", (ushort)ItemDefinition.UMP_45 },
        { "weapon_p90", (ushort)ItemDefinition.P90 },
        { "weapon_bizon", (ushort)ItemDefinition.PP_BIZON },
        { "weapon_mac10", (ushort)ItemDefinition.MAC_10 },
        { "weapon_usp_silencer", (ushort)ItemDefinition.USP_S },
        { "weapon_hkp2000", (ushort)ItemDefinition.P2000 },
        { "weapon_glock", (ushort)ItemDefinition.GLOCK_18 },
        { "weapon_elite", (ushort)ItemDefinition.DUAL_BERETTAS },
        { "weapon_p250", (ushort)ItemDefinition.P250 },
        { "weapon_fiveseven", (ushort)ItemDefinition.FIVE_SEVEN },
        { "weapon_cz75a", (ushort)ItemDefinition.CZ75_AUTO },
        { "weapon_tec9", (ushort)ItemDefinition.TEC_9 },
        { "weapon_revolver", (ushort)ItemDefinition.R8_REVOLVER },
        { "weapon_deagle", (ushort)ItemDefinition.DESERT_EAGLE },
        { "weapon_nova", (ushort)ItemDefinition.NOVA },
        { "weapon_xm1014", (ushort)ItemDefinition.XM1014 },
        { "weapon_mag7", (ushort)ItemDefinition.MAG_7 },
        { "weapon_sawedoff", (ushort)ItemDefinition.SAWED_OFF },
        { "weapon_m249", (ushort)ItemDefinition.M249 },
        { "weapon_negev", (ushort)ItemDefinition.NEGEV },
        { "weapon_taser", (ushort)ItemDefinition.ZEUS_X27 },
        { "weapon_hegrenade", (ushort)ItemDefinition.HIGH_EXPLOSIVE_GRENADE },
        { "weapon_molotov", (ushort)ItemDefinition.MOLOTOV },
        { "weapon_incgrenade", (ushort)ItemDefinition.INCENDIARY_GRENADE },
        { "weapon_smokegrenade", (ushort)ItemDefinition.SMOKE_GRENADE },
        { "weapon_flashbang", (ushort)ItemDefinition.FLASHBANG },
        { "weapon_decoy", (ushort)ItemDefinition.DECOY_GRENADE }
    };

}
