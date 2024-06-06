using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using static AdvancedWeaponSystem.AdvancedWeaponSystem;
using static CounterStrikeSharp.API.Core.Listeners;

namespace AdvancedWeaponSystem;

public static class Event
{
    public static void Load()
    {
        Instance.RegisterListener<OnEntitySpawned>(OnEntitySpawned);
        Instance.RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
        VirtualFunctions.CCSPlayer_WeaponServices_CanUseFunc.Hook(OnWeaponPickUp, HookMode.Pre);
    }

    public static void Unload()
    {
        Instance.RemoveListener<OnEntitySpawned>(OnEntitySpawned);
        VirtualFunctions.CCSPlayer_WeaponServices_CanUseFunc.Unhook(OnWeaponPickUp, HookMode.Pre);
    }

    public static HookResult OnWeaponPickUp(DynamicHook hook)
    {
        CBasePlayerWeapon weapon = hook.GetParam<CBasePlayerWeapon>(1);

        if (Instance.Config.WeaponDataList.FirstOrDefault(w => w.WeaponName == weapon.DesignerName && w.BlockToUse == true) == null)
        {
            return HookResult.Continue;
        }

        hook.SetReturn(false);
        return HookResult.Changed;
    }

    public static HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
    {
        WeaponData? weaponData;

        if ((weaponData = Instance.Config.WeaponDataList.FirstOrDefault(w => w.WeaponName == @event.Weapon)) == null)
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
            player!.ExecuteClientCommand("slot3");

            Instance.AddTimer(0.1f, () =>
            {
                player.ExecuteClientCommand("slot2");
            });
        }

        return HookResult.Continue;
    }

    public static void OnEntitySpawned(CEntityInstance entity)
    {
        WeaponData? weaponData;

        if ((weaponData = Instance.Config.WeaponDataList.FirstOrDefault(w => w.WeaponName == entity.DesignerName)) == null)
        {
            return;
        }

        CBasePlayerWeapon? weapon = entity.As<CBasePlayerWeapon>();

        if (weapon == null)
        {
            return;
        }

        CCSWeaponBaseVData? weaponbaseData = weapon.As<CCSWeaponBase>().VData;

        if (weaponbaseData != null)
        {
            if (weaponData.Clip.HasValue)
            {
                weaponbaseData.MaxClip1 = weaponData.Clip.Value;
                weaponbaseData.DefaultClip1 = weaponData.Clip.Value;
            }

            if (weaponData.Ammo.HasValue)
            {
                weaponbaseData.SecondaryReserveAmmoMax = weaponData.Ammo.Value;
            }

            if (weaponData.Slot.HasValue)
            {
                weaponbaseData.DefaultLoadoutSlot = (loadout_slot_t)weaponData.Slot.Value;
            }
        }

        if (weaponData.Clip.HasValue)
        {
            weapon.Clip1 = weaponData.Clip.Value;
        }

        if (weaponData.Ammo.HasValue)
        {
            weapon.ReserveAmmo[0] = weaponData.Ammo.Value;
        }
    }
}