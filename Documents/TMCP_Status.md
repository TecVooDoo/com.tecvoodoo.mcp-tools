# TecVooDoo MCP Tools -- Status

**Package:** `com.tecvoodoo.mcp-tools` v1.1.0
**Source (edit here):** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\` (edit directly in package)
**Package (UPM):** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\`
**Unity Requirement:** 6000.0+
**Last Updated:** March 20, 2026

> **Install:** Add to manifest.json: `"com.tecvoodoo.mcp-tools": "file:../../DefaultUnityPackages/com.tecvoodoo.mcp-tools"`
> Requires `com.ivanmurzak.unity.mcp` (MCP base) already installed.

---

## Current State

**75 tools** across 14 asset groups. All stable.

| Group | Tools | Define | Asmdef | Status |
|-------|-------|--------|--------|--------|
| Flexalon | 7 | `HAS_FLEXALON` | `MCPTools.Flexalon.Editor` | Stable |
| Prefab World Builder | 4 | `HAS_PWB` | None (`#if` only) | Stable |
| RayFire | 8 | `HAS_RAYFIRE` | `MCPTools.RayFire.Editor` | Stable |
| MagicaCloth 2 | 7 | `HAS_MAGICACLOTH2` | `MCPTools.MagicaCloth2.Editor` | Stable |
| Final IK | 5 | `HAS_FINALIK` | None (`#if` only) | Stable |
| Asset Inventory | 4 | `HAS_ASSETINVENTORY` | `MCPTools.AssetInventory.Editor` | Stable |
| Malbers AC | 8 | `HAS_MALBERS_AC` | `MCPTools.MalbersAC.Editor` | Stable |
| Quest Forge | 5 | `HAS_MALBERS_QUESTFORGE` | None (`#if` only) | Stable |
| Retarget Pro | 4 | `HAS_RETARGETPRO` | `MCPTools.RetargetPro.Editor` | Stable |
| **Rope Toolkit** | **5** | `HAS_ROPE_TOOLKIT` | None (`#if` only) | **New** |
| **Heathen Physics** | **5** | `HAS_HEATHEN_PHYSICS` | `MCPTools.HeathenPhysics.Editor` | **New** |
| **Heathen Ballistics** | **5** | `HAS_HEATHEN_BALLISTICS` | `MCPTools.HeathenBallistics.Editor` | **New** |
| **Feel** | **4** | `HAS_FEEL` | None (`#if` only) | **New** |
| **Damage Numbers Pro** | **4** | `HAS_DAMAGE_NUMBERS_PRO` | `MCPTools.DamageNumbersPro.Editor` | **New** |

**Auto-detection:** `MCPToolsDefineManager.cs` (Editor folder) scans for installed assets on domain reload and adds/removes `HAS_*` defines automatically. No manual setup needed.

---

## Package vs Source Sync

All 14 groups built directly in the package folder. No separate source location.

**Edit process:** Edit directly in `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\`. FinalIK, PWB, Quest Forge, Rope Toolkit, and Feel have no asmdef -- they use `#if HAS_*` guards and compile to nothing when the target asset isn't installed. SyntyAssets project was deleted Mar 12, 2026.

---

## Known Gotchas

| Gotcha | Details |
|--------|---------|
| RayFire crash | `DemolishForced()` and `ApplyDamage()` crash Unity when called from MCP context. Tools exist but should only be used for setup, not runtime testing. |
| Flexalon RequireComponent | `FlexalonLayoutBase` auto-adds `FlexalonObject` via `[RequireComponent]`. Use `GetComponent<FlexalonObject>()`, never `AddComponent<FlexalonObject>()` (returns null duplicate). |
| MagicaCloth MeshCloth | MeshCloth locks all verts. Must use `SkinnedMeshRenderer`, not `MeshRenderer`. |
| Asmdef pattern | Tools with asmdefs use `overrideReferences: true` + `ReflectorNet.dll`. Tools without asmdefs (PWB, FinalIK) live in Assembly-CSharp-Editor and use `#if` guards. |
| Domain reload | MCP disconnects during domain reload after adding defines. Wait for auto-reconnect. |

---

## Adding New Tool Groups

1. Create folder: `MCPTools/{AssetName}/Editor/`
2. Create partial class: `Tool_{AssetName}.cs` with `[McpPluginToolType]`
3. Create tool files: `Tool_{AssetName}.{Feature}.cs` with `[McpPluginTool]` methods
4. Add `HAS_{ASSETNAME}` detection to `MCPToolsDefineManager.cs`
5. Create asmdef if the asset has its own assembly (use `MCPTools.Flexalon.Editor.asmdef` as template). If asset lives in Assembly-CSharp, use `#if HAS_*` guards instead.
6. Test in Sandbox or target project (SyntyAssets deleted Mar 12, 2026)
7. Update `TMCP_Reference.md` and this status doc
8. Update `Sandbox_AssetLog.md` MCP Candidates section

---

## Session Log

### Session 2 (Mar 20, 2026) -- Rope Toolkit + Heathen + Feel + DNP

Added 5 new tool groups (23 tools):

**Rope Toolkit (5 tools):** `#if HAS_ROPE_TOOLKIT`, Assembly-CSharp (no asmdef)
- `rope-query` -- Read full rope config (simulation, collision, appearance, measurements, connections)
- `rope-configure-simulation` -- Set stiffness, energyLoss, gravityMultiplier, substeps, solverIterations
- `rope-configure-collision` -- Set collision enabled, influenceRigidbodies, friction, stride, margin
- `rope-configure-appearance` -- Set radius, radialVertices, isLoop
- `rope-add-connection` -- Add/configure RopeConnection (type, ropeLocation, rigidbody stiffness/damping)

**Heathen Unity Physics (5 tools):** `HAS_HEATHEN_PHYSICS`, `MCPTools.HeathenPhysics.Editor`
- `hphys-query` -- List all Heathen physics components (PhysicsData, BuoyantBody, ForceEffectField, ForceEffectReceiver)
- `hphys-configure-physics-data` -- Add/configure PhysicsData (volume, area, cross-sections, debug)
- `hphys-configure-buoyancy` -- Add/configure BuoyantBody (magnitude, calculationMode, activeSurface)
- `hphys-configure-force-field` -- Add/configure ForceEffectField (strength, radius, isGlobal)
- `hphys-configure-force-receiver` -- Add/configure ForceEffectReceiver (useLinear, useAngular, sensitivity)

**Heathen Ballistics (5 tools):** `HAS_HEATHEN_BALLISTICS`, `MCPTools.HeathenBallistics.Editor`
- `ballistic-query` -- List ballistic components (BallisticAim, TrickShot, BallisticPathLineRender, BallisticTargeting)
- `ballistic-configure-aim` -- Add/configure BallisticAim (speed, pivots, yaw/pitch limits, gravity)
- `ballistic-configure-trickshot` -- Add/configure TrickShot (speed, bounces, damping, distance, resolution, template)
- `ballistic-calculate-solution` -- Static aim solution: given from/to/speed, returns low+high arc Euler angles
- `ballistic-visualize` -- Add/configure BallisticPathLineRender (resolution, maxLength, bounces, gravityMode)

**Feel (4 tools):** `#if HAS_FEEL`, MoreMountains.Tools assembly
- `feel-query` -- Read MMF_Player settings and feedback list (type, label, active, duration, chance per feedback)
- `feel-configure-player` -- Add/configure MMF_Player (intensity, direction, cooldown, delay, durationMultiplier, channel)
- `feel-add-feedback` -- Add feedback type to FeedbacksList (CameraShake, Position, Scale, Rotation, Light, Particles, etc.)
- `feel-play` -- Runtime control: Play, Stop, Pause, Resume, Reset, Reverse

**Damage Numbers Pro (4 tools):** `HAS_DAMAGE_NUMBERS_PRO`, `MCPTools.DamageNumbersPro.Editor`
- `dnp-query` -- Read DamageNumber full config (display, fade, movement, spam control, performance)
- `dnp-configure-display` -- Set lifetime, permanent, number format, prefix/suffix texts, color, 3D/camera settings
- `dnp-configure-animation` -- Set fade in/out timing+offsets, movement mode (lerp/velocity/shaking), rotation
- `dnp-configure-performance` -- Set pooling (enable, poolSize), updateDelay, spamGroup, combination/push/destruction

### Session 1 (Mar 14, 2026) -- Malbers AC + Quest Forge

Added 2 new tool groups (13 tools total):

**Malbers AC (8 tools):**
- `ac-query-animal` -- Read full animal setup (states, modes, stances, speed sets)
- `ac-query-stats` -- Read all stats on a Stats component
- `ac-configure-state` -- Configure state properties (active, priority, input, strafe)
- `ac-configure-mode` -- Configure mode properties (active, cooldown, movement, rotation)
- `ac-configure-speed` -- Configure speed set entries (position, rotation, animator speeds)
- `ac-configure-stat` -- Configure individual stat values (value, max, regen, degen, immunity)
- `ac-configure-damageable` -- Configure MDamageable (multiplier, alignment)
- `ac-add-lock-axis` -- Add/configure LockAxis for 2.5D gameplay

**Quest Forge (5 tools):**
- `qf-create-quest` -- Create Quest ScriptableObject with ID, name, type
- `qf-query-quests` -- List all Quest SOs with objectives
- `qf-add-objective` -- Add objectives (Kill, Collect, TalkTo, GoToLocation, Interact)
- `qf-create-poi` -- Create PointOfInterest SO for minimap/compass
- `qf-query-pois` -- List all POI SOs with positions and settings

**Retarget Pro (4 tools):**
- `retarget-batch-bake` -- Batch-retarget folder of AnimationClips using a RetargetProfile
- `retarget-create-profile` -- Create RetargetProfile SO with source/target characters and rigs
- `retarget-query-profiles` -- List all RetargetProfile SOs with configuration details

### Session 0 (Pre-docs) -- Stable

All 35 tools built across Sandbox Sessions 52-55 in SyntyAssets project. Evaluated in Session 55. Documented in Sandbox ENTRY-267. Package created at `com.tecvoodoo.mcp-tools` v1.0.0. Installed in Sandbox and HOK.

---

## Reference

Full API reference: `TMCP_Reference.md` (this folder)
Sandbox eval: `Sandbox_AssetLog.md` ENTRY-267
MCP candidates tracking: `Sandbox_AssetLog.md` MCP Candidates section

---

**End of Document**
