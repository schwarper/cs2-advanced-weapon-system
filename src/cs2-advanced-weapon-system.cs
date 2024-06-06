using CounterStrikeSharp.API.Core;

namespace AdvancedWeaponSystem;

public class AdvancedWeaponSystem : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Advanced Weapon System";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "schwarper";

    public Config Config { get; set; } = new Config();
    public static AdvancedWeaponSystem Instance { get; set; } = new();

    public void OnConfigParsed(Config config)
    {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        Instance = this;

        Event.Load();
    }

    public override void Unload(bool hotReload)
    {
        Event.Unload();
    }
}