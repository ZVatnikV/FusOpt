# FusOpt

Open-source **Fusion lobby optimizer** for BONELAB.

Big lobbies (20+ players) + prop spam = uneven FPS. FusOpt fixes *your* machine first, and gives hosts optional auto-clean tools.

![FusOpt](https://raw.githubusercontent.com/ZVatnikV/FusOpt/main/docs/assets/hero.jpg)

## What it does

| Feature | Effect |
| --- | --- |
| Prop Cull | Hides distant clutter renderers locally |
| Rigidbody Sleep | Sleeps far props to cut physics |
| Particle / Shadow Cull | Pauses far FX, drops distant shadows |
| Adaptive Quality | Scales shadows & lights from FPS |
| Host Clean | `DespawnAll` + interval / threshold |
| BoneMenu | Full control, settings persist |

## Presets

- **Balanced** — everyday play
- **Performance** — solid FPS focus
- **Crowded** — 15–30 player lobbies (default)
- **Extreme** — survival mode for potato PCs / chaos servers

## Install

1. MelonLoader + BoneLib + LabFusion
2. Install this package (or drop `FusOpt.dll` into `Mods`)
3. BoneMenu → **FusOpt**

## Links

- Source: https://github.com/ZVatnikV/FusOpt
- Site: https://zvatnikv.github.io/FusOpt
- License: MIT

## Notes

- Client culling does **not** despawn networked props for others.
- Host cleanup requires you to be the Fusion host.
