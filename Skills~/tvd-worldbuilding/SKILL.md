---
name: tvd-worldbuilding
description: "Use this skill whenever the user wants to build or dress an outdoor set or landscape in Unity with the Procedural Worlds tools through MCP, e.g. 'make a terrain', 'build me a forest valley', 'add a road over the hill', 'put street lamps along it', 'dress this set', 'give me a flythrough of the set', 'take beauty shots of the landscape', 'stamp a mountain'. Covers the gaia-*, gena-* and pegasus-* TMCP tools in the right ORDER, with the measured rules that keep scripted calls from silently doing nothing or doing damage. Do NOT use for a single tool's parameters (each tool's own skill has those) or for non-terrain interiors."
metadata:
  author: TecVooDoo (TMCP)
  tools: "gaia-query, gaia-create-world, gaia-sea-level, gaia-stamp, gaia-spawn, gena-query, gena-road, gena-spawn, pegasus-query, pegasus-create, pegasus-control, screenshot-camera"
  verified: "SetDesign, Unity 6000.6.3f1, TMCP 1.17.3, 2026-10-10"
---

# World building: Gaia -> GeNa -> Pegasus

## 0. Check the tools exist, or stop

Run `unity-tool-list` with regex `^(gaia|gena|pegasus)-`. Each group appears only when its asset is installed (`HAS_GAIA` / `HAS_GENA` / `HAS_PEGASUS`). **If a group is missing, tell the user which asset it needs and skip that stage. Don't fake it with `script-execute`.**

## Mental model -- read this before calling anything

- **Results arrive a call LATER.** Gaia world creation and tree/detail spawning finish on later editor ticks. Act in one call, then read with `gaia-query` in a SEPARATE call. Stamping, GeNa roads and spawns, and Pegasus are synchronous.
- **The MCP client re-sends slow calls** (up to 10x). Every create/build tool here is idempotent: a repeat answers "already exists / already built" with the first result. Treat that as success, not an error.
- **The terrain is an asset.** Gaia spawning, stamping and GeNa roads write INTO the TerrainData asset. A scene duplicated with Save-As SHARES it, so work in a scene whose terrain is its own. `gena-road` refuses a shared TerrainData.
- **Sea level gates spawning.** Terrain below the session sea level gets nothing and Gaia reports success anyway. `gaia-spawn` refuses that case, so fix it with `gaia-sea-level` or `gaia-stamp`.

## The workflow

1. **Scene.** Use a saved scene that's yours to modify. `gaia-create-world` adds it to EditorBuildSettings, so revert that afterwards if the scene is scratch.
2. **Terrain.** `gaia-create-world` (tileSize, tileHeight, seaLevel), then `gaia-query` next call: expect 1+ terrain and the requested sea level (no "requested ... but" warning).
3. **Landform.** `gaia-stamp` with a stamp texture (Gaia ships many under `Procedural Worlds/.../Stamps`). It reports max height before -> after. Then `gaia-sea-level` (read): if the terrain is mostly below sea level, lower the sea level.
4. **Biome.** `gaia-query includeBiomePresets=true` to list presets, then `gaia-spawn action=create-biome name=<preset>`. It lists the spawners.
5. **Spawn ONE spawner per call, TEXTURE spawners first**, then trees, then grass/details. Looping several in one call placed 0 trees. After the trees, `gaia-query` next call: `trees > 0` and a nonzero detail density. If a tree spawner shows 0 on a fresh world, spawn it once more.
6. **Roads.** `gena-road name=... points=['x,z', ...]` (snapped to terrain). It's DESTRUCTIVE with no undo: it carves, clears trees and paints. In a big project the first call may say "scan incomplete"; call again (progress is kept). Then `gena-query`: the road shows `road BAKED`. Props already standing on the route are NOT moved, so re-snap them.
7. **Props.** `gena-spawn spawner=<GeNa spawner prefab> points=[...]`. Read EACH line: `PLACED` vs `NOTHING ... slope N deg` (GeNa silently rejects steep points; ~17 deg rejected a street lamp). A `landed N m ABOVE` warning means it stacked onto another prop's collider. GeNa sample props render MAGENTA on URP until Unity's Render Pipeline Converter runs.
8. **Review.** `pegasus-create name=... points=['x,y,z', ...]`. It drives its OWN disabled camera, never the Main Camera. Then `pegasus-control action=sample` (clearance: expect 0 below ground, min near minHeightAboveTerrain), then `action=scrub percent=0.25` + `screenshot-camera` on `<name> Camera` for stills. For a high camera path, raise `minHeightAboveTerrain`; Pegasus moves the POIs up to enforce it.

## Cleanup and what gets left behind

- **Gaia:** a temporary "Session Stamper"; the session's TerrainData under `Gaia User Data/Sessions/`; an EditorBuildSettings entry.
- **GeNa:** a `GeNa` root and `Baked_RoadMeshes` scaffolding; the baked spline goes INACTIVE. Search with inactive included.
- **Pegasus:** the manager and `<name> Camera`.
- Check `git status` and revert tracked settings you didn't mean to change. Gaia's Manager first-run also rewrites the shared URP asset's shadow settings.

## Asking the user

Before step 6 (destructive) on a scene that isn't scratch, confirm the route. For "make it look good" requests, show a Pegasus still after steps 5 and 7 and ask before adding more.
