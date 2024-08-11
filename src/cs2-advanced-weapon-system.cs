using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using CounterStrikeSharp.API.Modules.Utils;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using static AdvancedWeaponSystem.Config;
using static CounterStrikeSharp.API.Core.Listeners;

namespace AdvancedWeaponSystem;

public class AdvancedWeaponSystem : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Advanced Weapon System";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "schwarper";

    public static AdvancedWeaponSystem Instance { get; set; } = new();
    public Config Config { get; set; } = new Config();

    public override void Load(bool hotReload)
    {
        Instance = this;

        Event.Load();
    }

    public override void Unload(bool hotReload)
    {
        Event.Unload();
    }

    public void OnConfigParsed(Config config)
    {
        Config = config;
    }
}