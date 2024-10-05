using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
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
    private enum AcquireMethod : int
    {
        PickUp = 0,
        Buy,
    };

    private enum AcquireResult : int
    {
        Allowed = 0,
        InvalidItem,
        AlreadyOwned,
        AlreadyPurchased,
        ReachedGrenadeTypeLimit,
        ReachedGrenadeTotalLimit,
        NotAllowedByTeam,
        NotAllowedByMap,
        NotAllowedByMode,
        NotAllowedForPurchase,
        NotAllowedByProhibition,
    };

    private static readonly MemoryFunctionWithReturn<int, string, CCSWeaponBaseVData> GetCSWeaponDataFromKeyFunc =
        new(GameData.GetSignature("GetCSWeaponDataFromKey"));

    private static readonly MemoryFunctionWithReturn<CCSPlayer_ItemServices, CEconItemView, AcquireMethod, NativeObject, AcquireResult> CCSPlayer_CanAcquireFunc =
        new(GameData.GetSignature("CCSPlayer_CanAcquire"));

    public static void Load()
    {
        Instance.RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
        Instance.RegisterEventHandler<EventItemEquip>(OnItemEquip);
        Instance.RegisterEventHandler<EventItemPurchase>(OnItemPurchase, HookMode.Pre);
        Instance.RegisterListener<OnEntitySpawned>(OnEntitySpawned);
        Instance.RegisterListener<OnEntityCreated>(OnEntityCreated);
        Instance.RegisterListener<OnServerPrecacheResources>(OnServerPrecacheResources);

        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
        CCSPlayer_CanAcquireFunc.Hook(OnWeaponCanAcquire, HookMode.Pre);
    }

    public static void Unload()
    {
        Instance.RemoveListener<OnEntitySpawned>(OnEntitySpawned);
        Instance.RemoveListener<OnEntityCreated>(OnEntityCreated);
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
        CCSPlayer_CanAcquireFunc.Unhook(OnWeaponCanAcquire, HookMode.Pre);
    }

    private static HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
    {
        if (!WeaponDataList.TryGetValue(@event.Weapon, out WeaponData? weaponData) || weaponData == null)
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
        if (@event.Userid is not CCSPlayerController player)
        {
            return HookResult.Continue;
        }

        if (player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value is not CBasePlayerWeapon activeweapon)
        {
            return HookResult.Continue;
        }

        string globalname = activeweapon.Globalname;

        if (!string.IsNullOrEmpty(globalname))
        {
            ViewModel(player)?.SetModel(globalname.Split(',')[1]);
        }

        return HookResult.Continue;
    }

    private static HookResult OnItemPurchase(EventItemPurchase @event, GameEventInfo info)
    {
        string weapon = @event.Weapon;

        if (!WeaponDataList.TryGetValue(weapon, out WeaponData? weaponData))
        {
            return HookResult.Continue;
        }

        if (@event.Userid is not CCSPlayerController player)
        {
            return HookResult.Continue;
        }

        if (!Weapon.IsRestricted(player, weapon, weaponData))
        {
            return HookResult.Continue;
        }

        int refund = Weapon.Price(weapon);
        player.InGameMoneyServices!.Account += refund;
        return HookResult.Continue;
    }

    private static void OnEntitySpawned(CEntityInstance entity)
    {
        if (!WeaponDataList.TryGetValue(entity.DesignerName, out WeaponData? weaponData))
        {
            return;
        }

        if (entity.As<CCSWeaponBase>().VData is not CCSWeaponBaseVData weaponVData)
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
        if (!WeaponDataList.TryGetValue(entity.DesignerName, out WeaponData? weaponData) || string.IsNullOrEmpty(weaponData.Model))
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

            if (Utilities.GetPlayerFromSteamId(weapon.OriginalOwnerXuidLow) is not CCSPlayerController player)
            {
                return;
            }

            weapon.Globalname = $"{ViewModel(player)?.VMName},{weaponData.Model}";
            weapon.SetModel(weaponData.Model);
        });
    }

    private static void OnServerPrecacheResources(ResourceManifest manifest)
    {
        foreach (KeyValuePair<string, WeaponData> weaponData in WeaponDataList)
        {
            if (!string.IsNullOrEmpty(weaponData.Value.Model))
            {
                manifest.AddResource(weaponData.Value.Model);
            }
        }
    }

    private static HookResult OnTakeDamage(DynamicHook hook)
    {
        if (hook.GetParam<CEntityInstance>(0).DesignerName is not "player")
        {
            return HookResult.Continue;
        }

        CTakeDamageInfo info = hook.GetParam<CTakeDamageInfo>(1);
        CBaseEntity? weapon = info.Ability.Value;

        if (weapon == null || !WeaponDataList.TryGetValue(weapon.DesignerName, out WeaponData? weaponData) || weaponData == null)
        {
            return HookResult.Continue;
        }

        if (weaponData.OnlyHeadshot == true && GetHitGroup(hook) != HitGroup_t.HITGROUP_HEAD)
        {
            return HookResult.Handled;
        }

        if (weaponData.Damage != null)
        {
            info.Damage = SetDamage(info.Damage);
            return HookResult.Changed;
        }

        return HookResult.Continue;

        static unsafe HitGroup_t GetHitGroup(DynamicHook hook)
        {
            nint info = hook.GetParam<nint>(1);
            nint v4 = *(nint*)(info + 0x78);

            if (v4 == nint.Zero)
            {
                return HitGroup_t.HITGROUP_INVALID;
            }

            nint v1 = *(nint*)(v4 + 16);

            HitGroup_t hitgroup = HitGroup_t.HITGROUP_GENERIC;

            if (v1 != nint.Zero)
            {
                hitgroup = (HitGroup_t)(*(uint*)(v1 + 56));
            }

            return hitgroup;
        }

        float SetDamage(float oldDamage)
        {
            char operation = weaponData.Damage[0];
            string valuePart = weaponData.Damage[1..];

            if (!int.TryParse(valuePart, out int value))
            {
                return oldDamage;
            }

            return operation switch
            {
                '+' => oldDamage + value,
                '-' => oldDamage - value,
                '*' => oldDamage * value,
                '/' => value != 0 ? oldDamage / value : oldDamage,
                _ => int.TryParse(weaponData.Damage, out value) ? value : oldDamage
            };
        }
    }

    private static unsafe CBaseViewModel? ViewModel(CCSPlayerController player)
    {
        if (player.PlayerPawn.Value?.WeaponServices?.Handle is not nint handle)
        {
            return null;
        }

        CCSPlayer_ViewModelServices viewModelServices = new(handle);

        nint ptr = viewModelServices.Handle + Schema.GetSchemaOffset("CCSPlayer_ViewModelServices", "m_hViewModel");
        Span<nint> viewModels = MemoryMarshal.CreateSpan(ref ptr, 3);

        CHandle<CBaseViewModel> viewModel = new(viewModels[0]);

        return viewModel.Value;
    }

    public static HookResult OnWeaponCanAcquire(DynamicHook hook)
    {
        CCSWeaponBaseVData vdata = GetCSWeaponDataFromKeyFunc.Invoke(-1, hook.GetParam<CEconItemView>(1).ItemDefinitionIndex.ToString()) ?? throw new Exception("Failed to get CCSWeaponBaseVData");

        if (!WeaponDataList.TryGetValue(vdata.Name, out WeaponData? weaponData) || weaponData == null)
        {
            return HookResult.Continue;
        }

        if (hook.GetParam<CCSPlayer_ItemServices>(0).Pawn.Value?.Controller.Value?.As<CCSPlayerController>() is not CCSPlayerController player)
        {
            return HookResult.Continue;
        }

        if (!Weapon.IsRestricted(player, vdata.Name, weaponData))
        {
            return HookResult.Continue;
        }

        player.PrintToCenterAlert($"You cannot use {vdata.Name}");
        hook.SetReturn(AcquireResult.NotAllowedByProhibition);
        return HookResult.Handled;
    }
}