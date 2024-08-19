using CounterStrikeSharp.API.Core;

namespace AdvancedWeaponSystem;

public class AdvancedWeaponSystem : BasePlugin
{
    public override string ModuleName => "Advanced Weapon System";
    public override string ModuleVersion => "0.0.3";
    public override string ModuleAuthor => "schwarper";

    public static AdvancedWeaponSystem Instance { get; set; } = new();

    public override void Load(bool hotReload)
    {
        Instance = this;

        Config.Load();
        Event.Load();
    }

    public override void Unload(bool hotReload)
    {
        Event.Unload();
    }
}