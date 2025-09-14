using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using static AdvancedWeaponSystem.Config;
using static AdvancedWeaponSystem.Weapon;
using static CounterStrikeSharp.API.Core.Listeners;

namespace AdvancedWeaponSystem;

public class AdvancedWeaponSystem : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Advanced Weapon System";
    public override string ModuleVersion => "1.10";
    public override string ModuleAuthor => "schwarper";

    public Config Config { get; set; } = new Config();
    public static AdvancedWeaponSystem Instance { get; private set; } = new();


    public override void Load(bool hotReload)
    {
        Instance = this;

        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Hook(OnTakeDamage, HookMode.Pre);
        VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Hook(OnWeaponCanAcquire, HookMode.Pre);
    }

    public override void Unload(bool hotReload)
    {
        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Unhook(OnTakeDamage, HookMode.Pre);
        VirtualFunctions.CCSPlayer_ItemServices_CanAcquireFunc.Unhook(OnWeaponCanAcquire, HookMode.Pre);
    }

    public void OnConfigParsed(Config config)
    {
        Config = config;
    }

    [GameEventHandler]
    public HookResult OnWeaponFire(EventWeaponFire @event, GameEventInfo info)
    {
        if (@event.Userid is not { } player || player.PlayerPawn.Value?.WeaponServices?.ActiveWeapon.Value is not { } activeWeapon)
            return HookResult.Continue;

        if (!Config.WeaponDatas.TryGetValue(GetDesignerName(activeWeapon), out WeaponData? weaponData))
            return HookResult.Continue;

        if (weaponData.UnlimitedClip == true)
            activeWeapon.Clip1 += 1;

        if (weaponData.UnlimitedAmmo == true)
            activeWeapon.ReserveAmmo[0] += 1;

        if (weaponData.ReloadAfterShoot == true)
        {
            if (activeWeapon.As<CCSWeaponBase>().VData is not { } weaponVData)
                return HookResult.Continue;

            player!.ExecuteClientCommand("slot3");

            Instance.AddTimer(0.1f, () =>
            {
                player.ExecuteClientCommand($"slot{(uint)weaponVData.GearSlot + 1}");
            });
        }

        return HookResult.Continue;
    }

    [ListenerHandler<OnEntitySpawned>]
    public void OnEntitySpawned(CEntityInstance entity)
    {
        if (!entity.DesignerName.StartsWith("weapon_"))
            return;

        if (!Config.WeaponDatas.TryGetValue(GetDesignerName(entity.As<CBasePlayerWeapon>()), out WeaponData? weaponData))
            return;

        if (entity.As<CCSWeaponBase>().VData is not CCSWeaponBaseVData weaponVData)
            return;

        if (weaponData.Clip.HasValue)
            weaponVData.MaxClip1 = weaponData.Clip.Value;

        if (weaponData.Ammo.HasValue)
            weaponVData.PrimaryReserveAmmoMax = weaponData.Ammo.Value;
    }

    public HookResult OnTakeDamage(DynamicHook hook)
    {
        if (hook.GetParam<CEntityInstance>(0).DesignerName is not "player")
        {
            return HookResult.Continue;
        }

        CTakeDamageInfo info = hook.GetParam<CTakeDamageInfo>(1);
        CBaseEntity? weapon = info.Ability.Value;

        if (weapon == null)
            return HookResult.Continue;

        if (!Config.WeaponDatas.TryGetValue(GetDesignerName(weapon.As<CBasePlayerWeapon>()), out WeaponData? weaponData))
            return HookResult.Continue;

        if (weaponData.OnlyHeadshot == true && info.GetHitGroup() != HitGroup_t.HITGROUP_HEAD)
            return HookResult.Handled;

        SetDamage(info, weaponData);
        return HookResult.Continue;
    }

    public HookResult OnWeaponCanAcquire(DynamicHook hook)
    {
        CEconItemView econItem = hook.GetParam<CEconItemView>(1);
        ushort defIndex = econItem.ItemDefinitionIndex;

        if (!WeaponIndexToName.TryGetValue(defIndex, out string? weaponName))
            return HookResult.Continue;

        if (!Config.WeaponDatas.TryGetValue(weaponName, out WeaponData? weaponData))
            return HookResult.Continue;

        if (hook.GetParam<CCSPlayer_ItemServices>(0).Pawn.Value?.Controller.Value?.As<CCSPlayerController>() is not CCSPlayerController player)
            return HookResult.Continue;

        if (!IsRestricted(player, weaponName, weaponData, hook.GetParam<AcquireMethod>(2)))
            return HookResult.Continue;

        if (!player.IsBot)
            Instance.Localizer.ForPlayer(player, "You cannot use this weapon", weaponName);

        hook.SetReturn(AcquireResult.NotAllowedByProhibition);
        return HookResult.Handled;
    }
}


