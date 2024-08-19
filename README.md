# cs2-advanced-weapon-system

If you want to donate or need a help about plugin, you can contact me in discord private/server

Discord nickname: schwarper

Discord link : [Discord server](https://discord.gg/4zQfUzjk36)

# Config

> Clip => sets clip of weapon
> 
> Ammo => sets ammo of weapon
> 
> BlockUsing => blocks using weapon
>
> ReloadAfterShoot => forces reload after shooting (slot system, slot3, slot2 etc),
> 
> UnlimitedAmmo => sets unlimited ammo for weapon
> 
> UnlimitedClip => sets unlimited clip for weapon
>
> OnlyHeadshot => sets only headshot for weapon
>
> Model => sets the model of weapon
> 
> AdminFlagsToIgnoreBlockUsing => Ignores admins who has flags from BlockUsing and WeaponQuota
>
> WeaponQuota => Sets a limit on the number of the weapon each team can own based on the number of players.
>
> Damage => Sets weapon damage


Example;
```toml
["weapon_glock"]
Clip = 30
Ammo = 1
UnlimitedClip = true
Damage = "1000"
ReloadAfterShoot = true

["weapon_deagle"]
Clip = 1
Ammo = 1
UnlimitedAmmo = true
OnlyHeadshot = true
Damage = "*2"

["weapon_awp"]
WeaponQuota = { 4 = 1, 8 = 2, 16 = 3, 32 = 4 }
AdminFlagsToIgnoreBlockUsing = ["@css/root"]
Model = "models/.../.vmdl"

["weapon_ssg08"]
BlockUsing = true
AdminFlagsToIgnoreBlockUsing = ["@css/ban", "@css/unban"]
Damage = "/2"

["weapon_aug"]
BlockUsing = true

["weapon_ak47"]
WeaponQuota = { 0 = 1, 5 = 2, 10 = 3, 16 = 4, 32 = 5 }
```
