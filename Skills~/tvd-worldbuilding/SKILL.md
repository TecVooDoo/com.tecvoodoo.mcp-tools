---
name: tvd-worldbuilding
description: "Use this skill whenever the user wants to build or dress an outdoor set or landscape in Unity with the Procedural Worlds tools through MCP, e.g. 'make a terrain', 'build me a forest valley', 'add a road over the hill', 'put street lamps along it', 'dress this set', 'give me a flythrough of the set', 'take beauty shots of the landscape', 'stamp a mountain'. Covers the gaia-*, gena-* and pegasus-* TMCP tools in the right ORDER, with the measured rules and starting values that keep scripted calls from silently doing nothing or doing damage. Do NOT use for a single tool's parameters (each tool's own skill has those) or for non-terrain interiors."
metadata:
  author: TecVooDoo (TMCP)
  tools: "gaia-query, gaia-create-world, gaia-sea-level, gaia-stamp, gaia-spawn, gena-query, gena-road, gena-spawn, pegasus-query, pegasus-create, pegasus-control, screenshot-camera, assets-create-folder, scene-create"
  verified: "SetDesign, Unity 6000.6.3f1, TMCP 1.18.3, 2026-10-10 (valley trial)"
---

# World building: Gaia -> GeNa -> Pegasus

## 0. Check the tools exist, or stop

Run `unity-tool-list` with regex `^(gaia|gena|pegasus)-`. Each group appears only when its asset is installed (`HAS_GAIA` / `HAS_GENA` / `HAS_PEGASUS`). **If a group is missing, tell the user which asset it needs and skip that stage. Don't fake it with `script-execute`.**

## Mental model -- read this before calling anything

- **Results arrive a call LATER.** Gaia world creation and tree/detail spawning finish on later editor ticks. Act in one call, then read with `gaia-query` in a SEPARATE call. Stamping, GeNa roads and spawns, and Pegasus are synchronous.
- **The MCP client re-sends slow calls** (up to 10x). Every create/build tool here is idempotent: a repeat answers "already exists / already built" with the first result. Treat that as success, not an error.
- **The terrain is an asset.** Gaia spawning, stamping and GeNa roads write INTO the TerrainData asset. A scene duplicated with Save-As SHARES it, so work in a scene whose terrain is its own. `gena-road` refuses a shared TerrainData.
- **Sea level gates spawning.** Ground below the session sea level gets nothing, and Gaia reports success anyway. The terrain report shows "N% of the area below sea level"; `gaia-spawn` refuses when all of it is below.

## The workflow

1. **Scene.** `assets-create-folder` FIRST, then `scene-create` with a path in it. `scene-create` fails if the folder is missing and still leaves an unsaved scene claiming that path; save explicitly at the end. `gaia-create-world` adds the scene to EditorBuildSettings, so remove that entry afterwards for a scratch scene.
2. **Terrain.** `gaia-create-world` (tileSize, tileHeight, seaLevel), then `gaia-query` next call: expect the terrain and your sea level. A new world is flat at 0 m, so it reads "entirely below sea level" until you stamp. That's expected.
3. **Landform.** `gaia-stamp` with a stamp texture (Gaia ships many under `Procedural Worlds/.../Stamps`). It reports max height before -> after, so stamp, read, adjust. **Starting values:** `2k Mountain 1` at tileHeight 600 gives about 12 m per `height` unit (5 -> 60 m, 8 -> 96 m). For a hill a road will cross, stay near 60 m (see step 6). **A single mountain stamp leaves the tile edges at 0 m**, so set the sea level to about 0-5 m with `gaia-sea-level`, or the edges go bare. Read the "% below" figure.
4. **Biome.** `gaia-query includeBiomePresets=true`, then `gaia-spawn action=create-biome name=<preset>`. It lists the spawners. **The listing order is NOT the run order.**
5. **Spawn ONE spawner per call.** A single call that LOOPS over spawners placed 0 trees. Separate tool calls are fine, even sent in parallel (measured once). **Run order:**
   - `<Biome>-Textures` first.
   - Then `-Trees`.
   - Then texture spawners that mask on trees (e.g. `-Tree Leaves Texture`).
   - Then `-Bushes` / `-Fern` / `-Grass` / `-Stone` / `-Mushrooms`.
   - Skip `GaiaPro-Village`, `-VFX`, `-Audio` unless asked.
   After the trees, `gaia-query` next call: `trees > 0` and a nonzero detail density. If a tree spawner shows 0 on a fresh world, spawn it once more.
6. **Roads.** `gena-road name=... points=['x,z', ...]` (snapped to terrain). It's DESTRUCTIVE with no undo: it carves, clears trees and paints. **Route and grade:** a straight road over a ~95 m summit carved a deep trench with 30-68 deg banks. Route diagonally or around the peak, keep the hill lower, or raise `yOffset` where it crosses. Read the reply's **Verges** line (slope 6 and 12 m out). In a big project the first call may say "scan incomplete"; call again. Then `gena-query`: `road BAKED`. Props already on the route are NOT moved; move them with `gameobject-modify` / re-spawn.
7. **Props.** `gena-spawn spawner=<GeNa spawner prefab> points=[...] searchRadius=12`. Beside a fresh road the verges are steep (16-25 deg), and GeNa silently rejects steep ground: a street lamp placed at up to 11.2 deg and was rejected from 16.8. `searchRadius` moves each point to the nearest BARE-TERRAIN spot under `maxSlope` (default 11) at least `clearance` (default 4 m) from roads and other props -- and says how far; "** no bare-terrain spot ... **" means nothing within the radius passed, so give a point on the open side, away from cut banks. Read EACH line: `PLACED` vs `NOTHING`; `landed N m ABOVE` means it stacked onto another prop. GeNa sample props render MAGENTA on URP until Unity's Render Pipeline Converter runs.
8. **Review.** `pegasus-create`. **Starting recipe** (worked first time):
   - an orbit of 4 points at radius about 0.35 x terrain size;
   - altitude about 1.2 x the peak height;
   - ONE `lookAts` entry at the summit;
   - `minHeightAboveTerrain=15`.
   Then `pegasus-control action=sample` (expect 0 below ground), then `action=scrub percent=0 / 0.25` + `screenshot-camera` on `<name> Camera` (it renders though disabled). **A single tile shows a flat void past its edge in high shots.** Keep shots lower and aimed inward, or build more tiles. Pegasus raises POIs itself to honour the minimum height.

## Cleanup and what gets left behind

Scene roots after a full run: `Gaia Tools`, `Gaia Runtime`, `Gaia Terrains`, `GeNa`, `BakedUnsplit_RoadMeshes`, `Spawner - <name>` (the GeNa spawner instance), `<name>` + `<name> Camera` (Pegasus). The baked GeNa spline is INACTIVE (search with inactive included). On disk: the Gaia session folder `Gaia User Data/Sessions/GS-<timestamp>/`, which holds the TerrainData. Remove the EditorBuildSettings entry for a scratch scene. Check `git status` and revert tracked settings you didn't mean to change; Gaia Manager's first run also rewrites the shared URP asset's shadow settings.

## Asking the user

Before step 6 (destructive) on a scene that isn't scratch, confirm the route. For "make it look good" requests, show a Pegasus still after steps 5 and 7 and ask before adding more.
