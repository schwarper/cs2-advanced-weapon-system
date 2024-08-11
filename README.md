# cs2-advanced-weapon-system

If you want to donate or need a help about plugin, you can contact me in discord private/server

Discord nickname: schwarper

Discord link : [Discord server](https://discord.gg/4zQfUzjk36)

# Json

> Clip => sets clip of weapon
> 
> Ammo => sets ammo of weapon
> 
> BlockUsing => blocks using weapon
> 
> ReloadAfterShoot => forces reload after shooting (slot system, slot3, slot2 etc)
> 
> UnlimitedAmmo => sets unlimited ammo for weapon
> 
> UnlimitedClip => sets unlimited clip for weapon
> 
> Model => sets the model of weapon

Example json;
```json
{
  "Weapons": {
    "weapon_ak47": {
      "Clip": 100,
      "Ammo": 300,
      "UnlimitedClip": true
    },
    "weapon_deagle": {
      "Clip": 1,
      "Ammo": 1,
      "UnlimitedAmmo": true
    },
    "weapon_awp": {
      "BlockUsing": true
    },
    "weapon_aug": {
      "Model": "models/.../.vmdl"
    }
  }
}
```
