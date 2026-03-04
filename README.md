# MoreRushes
A mod for Neon White that adds simple Mikey Mode-style rush alternatives.

> Note: This mod replaces my previous (and confusingly named) mod, "*CustomRushMode*" (not to be confused with "*CustomRush*," which is a much better mod.)

## Included Rush Modes
- Purify Rush
- Elevate Rush
- Godspeed Rush
- Stomp Rush
- Fireball Rush
- Random Rush

You can toggle between rush modes through Melon Preferences. All modes work in both individual levels and Level Rushes.

## Random Rush Seeding
MoreRushes introduces Random Rush seeding.

Each Random Rush attempt uses a non-zero `uint` seed (1–4,294,967,295) to determine its card loadout. You can set custom seeds in Melon Preferences.

Seeds are deterministic and should be globally consistent, meaning you can share seeds with friends or use them for duels.

## Automatic Ghosts
If enabled, ghosts will automatically be created for the selected rush mode. Each completed Random Rush seed generates its own ghost.

## Important Notes
- This mod uses [NeonLite](https://github.com/Faustas156/NeonLite)'s built-in anti-cheat, so make sure you have the latest version installed.
- These rushes are just for fun and have not been extensively tested for balance/playability.

## Installation
1. Install [NeonLite](https://github.com/Faustas156/NeonLite) by following its installation instructions.
2. Download [the latest MoreRushes release](https://github.com/joeyexists/MoreRushes/releases/latest).
3. Drop `MoreRushes.dll` into your `Neon White\Mods` folder.
4. Launch the game and press `F5` to open Melon Preferences and configure the mod.
