# SpawnBlocker

A Valheim mod that adds a buildable **Spawn Blocker**: place one in your base and monsters stop spawning around it, and the ones that wander in are removed.

No more greydwarfs respawning between your houses, no more skeletons popping out of the nest you built next to.

## Features

- **Spawn Blocker piece** in the Hammer, *Misc* tab. Costs 2 Wood, built from the vanilla ward model.
- Inside the blocked radius (75 m by default):
  - natural spawns don't happen at all,
  - spawner nests (greydwarf nests, skeleton piles, …) stay quiet,
  - hostile monsters that walk in, or come from fixed creature spawners, are removed.
- Left alone on purpose:
  - **raids** – raid spawns and raid monsters behave normally,
  - **tamed animals** (wild ones are blocked like any other monster),
  - **bosses**,
  - **sleeping monsters** (for example in dungeons and crypts).
- The radius is configurable and synced from the server, so everyone uses the server's value.

## Installation

### Mod manager (r2modman, Gale, Thunderstore App)
Install **SpawnBlocker**. BepInExPack_Valheim and Jötunn are installed with it.

### Manual
1. Install [BepInExPack_Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) and [Jötunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/).
2. Copy `SpawnBlocker.dll` into `Valheim/BepInEx/plugins/SpawnBlocker/`.

### Multiplayer
**Every player and the dedicated server need the mod**, in the same minor version (0.1.x). Jötunn refuses the connection otherwise, because players without the mod can't see or use the piece.

## Configuration

`BepInEx/config/com.bobo.spawnblocker.cfg`, created on first start:

| Setting | Default | Description |
|---|---|---|
| `General.BlockRadius` | `75` | Radius in meters (5–150) around a Spawn Blocker where monsters can't spawn or stay. Admin only: on a server, the server's value is used by everyone. |

## How it works

- Every Spawn Blocker that is loaded registers itself; checking a position is a distance comparison against those few blockers, so the mod costs next to nothing even in large bases.
- `SpawnSystem.Spawn` and `SpawnArea.SpawnOne` are skipped inside a blocked area, so monsters are prevented instead of spawned and deleted.
- `MonsterAI.UpdateAI` removes hostile monsters that are inside a blocked area anyway (walked in, fixed creature spawners, already there when the blocker was built). It runs on whichever machine owns the monster, so it works the same on a dedicated server and in a hosted game.

## Known limitations

- A blocker only works while its area is loaded (somebody is nearby), like every other piece in Valheim.
- Fixed creature spawners (for example in points of interest) still spawn their creature, which is then removed; they respawn on their usual timer.
- The radius is a sphere around the piece, so it also reaches up and down.
- Removed monsters don't drop anything.

## Building from source

Requirements:
- .NET SDK 8 or newer (Linux or Windows)
- Valheim with BepInExPack_Valheim and Jötunn installed – the project compiles against your local game files

The project looks for Valheim in the default Steam folder. For any other location, create `Environment.props` in the repository root (it's git-ignored):

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
  <PropertyGroup>
    <VALHEIM_INSTALL>/path/to/steamapps/common/Valheim</VALHEIM_INSTALL>
    <!-- Optional: if Jotunn.dll is not in BepInEx/plugins/ValheimModding-Jotunn or BepInEx/plugins/Jotunn -->
    <!-- <JOTUNN_DLL>/path/to/Jotunn.dll</JOTUNN_DLL> -->
    <!-- Optional: Debug builds copy the dll here -->
    <!-- <MOD_DEPLOYPATH>$(VALHEIM_INSTALL)/BepInEx/plugins/SpawnBlocker</MOD_DEPLOYPATH> -->
  </PropertyGroup>
</Project>
```

Then:

```sh
dotnet build -c Debug     # SpawnBlocker/bin/Debug/SpawnBlocker.dll (+ copy to MOD_DEPLOYPATH if set)
dotnet build -c Release   # Packages/SpawnBlocker-<version>.zip, ready for Thunderstore
```

To release a new version, bump it in three places – the Release build fails if they don't match:
`<Version>` in `SpawnBlocker/SpawnBlocker.csproj`, `PluginVersion` in `SpawnBlocker/SpawnBlocker.cs` and `version_number` in `SpawnBlocker/Package/manifest.json`.

`JotunnModUnity/` is the Unity project from the original Jötunn template. The mod doesn't use any custom assets at the moment.

## Changelog

### 0.1.0
- Monsters are now prevented from spawning near a blocker instead of being spawned and deleted.
- Much cheaper checks: blockers register themselves instead of a 75 m physics scan per monster per frame.
- Wild boars, wolves, lox and other tameable species are blocked too; only tamed ones are left alone.
- Bosses are never removed.
- Radius is configurable and synced from the server.
- Proper piece name and description in the build menu.
- Everyone on a server must have the mod (version check through Jötunn).
- Updated for Valheim 1.0.17 and Jötunn 2.30.2; builds on Linux and Windows with the .NET SDK.

### 0.0.4
- First working version.

## License

[WTFPL](LICENSE)
