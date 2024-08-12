using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using System.Runtime.InteropServices;
using static AdvancedWeaponSystem.AdvancedWeaponSystem;
using static AdvancedWeaponSystem.Config;
using static CounterStrikeSharp.API.Core.Listeners;

namespace AdvancedWeaponSystem;

public static class Event
{
    public static void Load()
    {
        Instance.RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
        Instance.RegisterEventHandler<EventItemEquip>(OnItemEquip);
        Instance.RegisterListener<OnEntitySpawned>(OnEntitySpawned);
        Instance.RegisterListener<OnEntityCreated>(OnEntityCreated);
        Instance.RegisterListener<OnServerPrecacheResources>(OnServerPrecacheResources);

        VirtualFunctions.CCSPlayer_WeaponServices_CanUseFunc.Hook(OnCanUse, HookMode.Pre);
    }

    public static void Unload()
    {
        Instance.RemoveListener<OnEntitySpawned>(OnEntitySpawned);
        Instance.RemoveListener<OnEntityCreated>(OnEntityCreated);
        VirtualFunctions.CCSPlayer_WeaponServices_CanUseFunc.Unhook(OnCanUse, HookMode.Pre);
    }

    private static HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
    {
        if (!Instance.Config.WeaponDataList.TryGetValue(@event.Weapon, out WeaponData? weaponData) || weaponData == null)
        {
            return HookResult.Continue;
        }

        CCSPlayerController? player = @event.Userid;
        CBasePlayerWeapon? activeWeapon = player?.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value;

        if (activeWeapon == null)
        {
            return HookResult.Continue;
        }

        if (weaponData.UnlimitedClip == true)
        {
            activeWeapon.Clip1 += 1;
        }

        if (weaponData.UnlimitedAmmo == true)
        {
            activeWeapon.ReserveAmmo[0] += 1;
        }

        if (weaponData.ReloadAfterShoot == true)
        {
            CCSWeaponBaseVData? weaponSlot = activeWeapon.As<CCSWeaponBase>().VData;

            if (weaponSlot == null)
            {
                return HookResult.Continue;
            }

            player!.ExecuteClientCommand("slot3");

            Instance.AddTimer(0.1f, () =>
            {
                player.ExecuteClientCommand($"slot{(uint)weaponSlot.GearSlot + 1}");
            });
        }

        return HookResult.Continue;
    }

    private static HookResult OnItemEquip(EventItemEquip @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;
        CBasePlayerWeapon? activeweapon = player?.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value;

        if (activeweapon == null)
        {
            return HookResult.Continue;
        }

        string globalname = activeweapon.Globalname;

        if (!string.IsNullOrEmpty(globalname))
        {
            ViewModel(player!)?.SetModel(globalname.Split(',')[1]);
        }

        return HookResult.Continue;
    }

    private static void OnEntitySpawned(CEntityInstance entity)
    {
        if (!Instance.Config.WeaponDataList.TryGetValue(entity.DesignerName, out WeaponData? weaponData))
        {
            return;
        }

        CCSWeaponBaseVData? weaponVData = entity.As<CCSWeaponBase>().VData;

        if (weaponVData == null)
        {
            return;
        }

        if (weaponData.Clip.HasValue)
        {
            weaponVData.MaxClip1 = weaponData.Clip.Value;
        }

        if (weaponData.Ammo.HasValue)
        {
            weaponVData.PrimaryReserveAmmoMax = weaponData.Ammo.Value;
        }
    }

    private static void OnEntityCreated(CEntityInstance entity)
    {
        if (!Instance.Config.WeaponDataList.TryGetValue(entity.DesignerName, out WeaponData? weaponData) || string.IsNullOrEmpty(weaponData.Model))
        {
            return;
        }

        CBasePlayerWeapon weapon = entity.As<CBasePlayerWeapon>();

        Server.NextWorldUpdate(() =>
        {
            if (!weapon.IsValid || weapon.OriginalOwnerXuidLow <= 0)
            {
                return;
            }

            CCSPlayerController? player = Utilities.GetPlayerFromSteamId(weapon.OriginalOwnerXuidLow);

            if (player == null)
            {
                return;
            }

            weapon.Globalname = $"{ViewModel(player)?.VMName},{weaponData.Model}";
            weapon.SetModel(weaponData.Model);
        });
    }

    private static void OnServerPrecacheResources(ResourceManifest manifest)
    {
        foreach (KeyValuePair<string, WeaponData> weaponData in Instance.Config.WeaponDataList)
        {
            if (!string.IsNullOrEmpty(weaponData.Value.Model))
            {
                manifest.AddResource(weaponData.Value.Model);
            }
        }
    }

    private static HookResult OnCanUse(DynamicHook hook)
    {
        CBasePlayerWeapon weapon = hook.GetParam<CBasePlayerWeapon>(1);

        if (!Instance.Config.WeaponDataList.TryGetValue(weapon.DesignerName, out WeaponData? weaponData) || weaponData?.BlockUsing != true)
        {
            return HookResult.Continue;
        }

        CCSPlayer_WeaponServices weaponServices = hook.GetParam<CCSPlayer_WeaponServices>(0);
        CCSPlayerController? player = weaponServices.Pawn.Value.Controller.Value?.As<CCSPlayerController>();

        if (player == null)
        {
            return HookResult.Continue;
        }

        string[]? flags = weaponData.AdminFlagsToIgnoreBlockUsing;

        if (flags?.Length > 0 && AdminManager.PlayerHasPermissions(player, flags))
        {
            return HookResult.Continue;
        }

        player.PrintToChat($"You cannot use {weapon.DesignerName}");

        weapon.Remove();
        hook.SetReturn(false);
        return HookResult.Handled;
    }

    private static unsafe CBaseViewModel? ViewModel(CCSPlayerController player)
    {
        nint? handle = player.PlayerPawn.Value?.ViewModelServices?.Handle;

        if (handle == null || !handle.HasValue)
        {
            return null;
        }

        CCSPlayer_ViewModelServices viewModelServices = new(handle.Value);

        nint ptr = viewModelServices.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
        Span<nint> viewModels = MemoryMarshal.CreateSpan(ref ptr, 3);

        CHandle<CBaseViewModel> viewModel = new(viewModels[0]);

        return viewModel.Value;
    }
}