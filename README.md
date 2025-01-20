# CounterstrikeSharp - Roll The Dice

[![UpdateManager Compatible](https://img.shields.io/badge/CS2-UpdateManager-darkgreen)](https://github.com/Kandru/cs2-update-manager/)
[![Discord Support](https://img.shields.io/discord/289448144335536138?label=Discord%20Support&color=darkgreen)](https://discord.gg/bkuF8xKHUt)
[![GitHub release](https://img.shields.io/github/release/Kandru/cs2-roll-the-dice?include_prereleases=&sort=semver&color=blue)](https://github.com/Kandru/cs2-roll-the-dice/releases/)
[![License](https://img.shields.io/badge/License-GPLv3-blue)](#license)
[![issues - cs2-roll-the-dice](https://img.shields.io/github/issues/Kandru/cs2-roll-the-dice?color=darkgreen)](https://github.com/Kandru/cs2-roll-the-dice/issues)
[![](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=C2AVYKGVP9TRG)

This plugin lets your players roll the dice each round (at any time during an round) to get either a positive or negative effect for the current round.

## Current Features

- Bigger Taser Battery (between 2 and 10 instant taser shots)
- Change player name (changes the player name randomly)
- Change player model (disguise as enemy player model)
- Chicken Leader (spawns chickens around the player)
- Decrease health (-10 to -30 health)
- Instant bomb plant or bomb defuse
- Give between 1 to 5 health shots
- Increase health (10 to 30 health)
- Increase speed (+50% to +100%)
- No explosives (no grenades)
- Change player to a big chicken
- Cloak (Player is invisible after 2 seconds without movement)
- Disguise as Plant (gives player a random prop model)
- High Gravity
- Invisibility (Player 50% visible)
- Low Gravity
- Make fake gun sounds
- Make fake hostage sounds
- One HP
- Respawn after death
- Suicide
- Vampire (get the damage as health, max. 200hp)
- Strip weapons

## Plugin Installation

1. Download and extract the latest release from the [GitHub releases page](https://github.com/Kandru/cs2-roll-the-dice/releases/).
2. Move the "RollTheDice" folder to the `/addons/counterstrikesharp/configs/plugins/` directory of your gameserver.
3. Restart the server.

## Plugin Update

Simply overwrite all plugin files and they will be reloaded automatically or just use the [Update Manager](https://github.com/Kandru/cs2-update-manager/) itself for an easy automatic or manual update by using the *um update RollTheDice* command.

## Commands

There is currently one client-side command available for this plugin:

### !dice / !rtd

This command triggers the dice for a player. To activate the dice with a button paste the following to the client console:

```
bind "o" rtd
```

## Configuration

This plugin automatically creates a readable JSON configuration file. This configuration file can be found in `/addons/counterstrikesharp/configs/plugins/RollTheDice/RollTheDice.json`.

```json
{
  "enabled": true,
  "debug": false,
  "dices": {
    "DiceChangePlayerModel": true,
    "DiceIncreaseHealth": true,
    "DiceDecreaseHealth": true,
    "DiceIncreaseSpeed": true,
    "DiceChangeName": true,
    "DicePlayerInvisible": true,
    "DicePlayerSuicide": false,
    "DicePlayerRespawn": true,
    "DiceStripWeapons": true,
    "DiceChickenLeader": true,
    "DiceFastBombAction": true,
    "DicePlayerVampire": true,
    "DicePlayerLowGravity": true,
    "DicePlayerHighGravity": true,
    "DicePlayerOneHP": true,
    "DicePlayerDisguiseAsPlant": true,
    "DicePlayerAsChicken": true,
    "DicePlayerMakeHostageSounds": true,
    "DicePlayerMakeFakeGunSounds": true,
    "DiceBigTaserBattery": true,
    "DicePlayerCloak": true,
    "DiceGiveHealthShot": true,
    "DiceNoExplosives": true
  },
  "maps": {},
  "ConfigVersion": 1
}
```

You can either disable the complete RollTheDice Plugin by simply setting the *enable* boolean to *false* or disable single dices from being updated. You can also specify a specific map where you want all or specific dices to be disabled (or enabled). This allows for a maximum customizability.

## Compile Yourself

Clone the project:

```bash
git clone https://github.com/Kandru/cs2-roll-the-dice.git
```

Go to the project directory

```bash
  cd cs2-roll-the-dice
```

Install dependencies

```bash
  dotnet restore
```

Build debug files (to use on a development game server)

```bash
  dotnet build
```

Build release files (to use on a production game server)

```bash
  dotnet publish
```

## License

Released under [GPLv3](/LICENSE) by [@Kandru](https://github.com/Kandru).

## Authors

- [@derkalle4](https://www.github.com/derkalle4)
