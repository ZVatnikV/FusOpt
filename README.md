# FusOpt

**Open-source Fusion lobby optimizer for BONELAB.**

Crowded Fusion servers (20+ players, prop spam) crush FPS differently on every machine. FusOpt gives you client-side relief and optional host cleanup — all from BoneMenu, with settings saved between sessions.

Website: [https://zvatnikv.github.io/FusOpt](https://zvatnikv.github.io/FusOpt)

## Features

- **Presets** — Balanced / Performance / Crowded / Extreme
- **Prop cull** — hide distant clutter renderers locally (does not break Fusion sync)
- **Rigidbody sleep** — put far props to sleep to cut physics cost
- **Particle & shadow cull** — pause far FX, drop distant shadows
- **Adaptive quality** — scales shadow distance / pixel lights from FPS
- **Host tools** — DespawnAll cleanup, auto-clean interval, prop threshold, memory clean
- **BoneMenu** — every toggle persists via MelonPreferences

## Requirements

- BONELAB (PC)
- MelonLoader 0.6.x
- [BoneLib](https://thunderstore.io/c/bonelab/p/SoulWithMae/BoneLib/)
- [LabFusion](https://thunderstore.io/c/bonelab/p/Lakatrazz/Fusion/)

## Install

1. Install MelonLoader, BoneLib, and LabFusion.
2. Drop `FusOpt.dll` into `BONELAB/Mods`.
3. Launch the game → open **BoneMenu → FusOpt**.
4. Start with the **Crowded** preset on big lobbies.

Settings file: `UserData/MelonPreferences.cfg` → category `FusOpt`.

## Build

```bash
dotnet build -c Release
```

Optional: set `BonelabPath` if BONELAB is not at the default Steam path.

## Thunderstore

See `thunderstore/` for the package layout. Run:

```powershell
./scripts/pack-thunderstore.ps1
```

## License

MIT — see [LICENSE](LICENSE).

## Disclaimer

FusOpt is a community tool. Host cleanup requires Fusion host privileges. Client culling only affects what *you* render/simulate locally.
