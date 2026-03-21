# TecVooDoo MCP Tools -- Status

**Package:** `com.tecvoodoo.mcp-tools` v1.4.0
**Source (edit here):** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\` (edit directly in package)
**Package (UPM):** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\`
**Unity Requirement:** 6000.0+
**Last Updated:** March 20, 2026 (Session 4b)

> **Install:** Add to manifest.json: `"com.tecvoodoo.mcp-tools": "file:../../DefaultUnityPackages/com.tecvoodoo.mcp-tools"`
> Requires `com.ivanmurzak.unity.mcp` (MCP base) already installed.

---

## Current State

**~135 tools** across 26 asset groups. All compiling.

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
| Rope Toolkit | 5 | `HAS_ROPE_TOOLKIT` | None (`#if` only) | Stable |
| Heathen Physics | 5 | `HAS_HEATHEN_PHYSICS` | `MCPTools.HeathenPhysics.Editor` | Stable |
| Heathen Ballistics | 5 | `HAS_HEATHEN_BALLISTICS` | `MCPTools.HeathenBallistics.Editor` | Stable |
| Feel | 4 | `HAS_FEEL` | None (`#if` only) | Stable |
| Damage Numbers Pro | 4 | `HAS_DAMAGE_NUMBERS_PRO` | `MCPTools.DamageNumbersPro.Editor` | Stable |
| Cinemachine | 5 | `HAS_CINEMACHINE` | `MCPTools.Cinemachine.Editor` | Stable |
| Animation Rigging | 5 | `HAS_ANIMATION_RIGGING` | `MCPTools.AnimationRigging.Editor` | Stable |
| ALINE | 4 | `HAS_ALINE` | `MCPTools.ALINE.Editor` | Stable |
| **Master Audio** | **6** | `HAS_MASTERAUDIO` | None (`#if` only) | **New S4** |
| **A* Pathfinding** | **6** | `HAS_ASTAR` | `MCPTools.AStarPathfinding.Editor` | **New S4** |
| **Dialogue System** | **6** | `HAS_DIALOGUE_SYSTEM` | None (`#if` only) | **New S4** |
| **SensorToolkit 2** | **5** | `HAS_SENSORTOOLKIT` | `MCPTools.SensorToolkit.Editor` | **New S4** |
| **UCC (Opsive)** | **5** | `HAS_UCC` | `MCPTools.UCC.Editor` | **New S4** |
| **Behavior Designer** | **4** | `HAS_BEHAVIOR_DESIGNER` | `MCPTools.BehaviorDesigner.Editor` | **New S4** |
| **DOTween Pro** | **4** | `HAS_DOTWEEN` | None (`#if` only) | **New S4** |
| **Unity Entities** | **5** | `HAS_UNITY_ENTITIES` | `MCPTools.UnityEntities.Editor` | **New S4b** |
| **Unity Physics** | **4** | `HAS_UNITY_PHYSICS` | `MCPTools.UnityPhysics.Editor` | **New S4b** |

**Auto-detection:** `MCPToolsDefineManager.cs` (Editor folder) scans for installed assets on domain reload and adds/removes `HAS_*` defines automatically. No manual setup needed. When an asset is removed from a project, its tools silently deactivate.

---

## Package vs Source Sync

All 24 groups built directly in the package folder. No separate source location.

**Edit process:** Edit directly in `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\`. Assets without asmdef use `#if HAS_*` guards (FinalIK, PWB, Quest Forge, Rope Toolkit, Feel, Master Audio, Dialogue System, DOTween). Assets with asmdef use `defineConstraints` for the same effect.

---

## Known Gotchas

| Gotcha | Details |
|--------|---------|
| RayFire crash | `DemolishForced()` and `ApplyDamage()` crash Unity when called from MCP context. Tools exist but should only be used for setup, not runtime testing. |
| Flexalon RequireComponent | `FlexalonLayoutBase` auto-adds `FlexalonObject` via `[RequireComponent]`. Use `GetComponent<FlexalonObject>()`, never `AddComponent<FlexalonObject>()` (returns null duplicate). |
| MagicaCloth MeshCloth | MeshCloth locks all verts. Must use `SkinnedMeshRenderer`, not `MeshRenderer`. |
| Asmdef pattern | Tools with asmdefs use `overrideReferences: true` + `ReflectorNet.dll`. Tools without asmdefs live in Assembly-CSharp-Editor and use `#if` guards. |
| Domain reload | MCP disconnects during domain reload after adding defines. Wait for auto-reconnect. |
| BD Pro v2.x API | Behavior Designer Pro v2 uses `StartBehavior`/`StopBehavior`/`RestartBehavior` (not `EnableBehavior`/`DisableBehavior` from v1). `SharedVariables` is an array, not `GetAllVariables()`. `GetVariable` takes `PropertyName`, not string. |
| UCC reflection | UCC tools use 100% reflection-based API access for resilience across versions. |
| DOTween detect type | DefineManager detects `DOTweenAnimation, DOTweenPro` (Pro DLL), not core `DOTween.dll`. |
| Agent API verification | Build agents must verify actual API signatures from source before writing code. Never assume from documentation knowledge or eval summaries. |

---

## Adding New Tool Groups

1. Create folder: `{AssetName}/Editor/`
2. Create partial class: `Tool_{AssetName}.cs` with `[McpPluginToolType]`
3. Create tool files: `Tool_{AssetName}.{Feature}.cs` with `[McpPluginTool]` methods
4. Add `HAS_{ASSETNAME}` detection to `MCPToolsDefineManager.cs`
5. Create asmdef if the asset has its own assembly (use `MCPTools.Flexalon.Editor.asmdef` as template). If asset lives in Assembly-CSharp, use `#if HAS_*` guards instead.
6. **CRITICAL:** Read actual source files to verify every method/property name before writing code. Never assume APIs.
7. Test in Sandbox or target project
8. Update `TMCP_Reference.md` and this status doc
9. Update `Sandbox_AssetLog.md` MCP Candidates section

---

## Session Log

### Session 4 (Mar 20, 2026) -- 7 new tool groups (36 tools) + MCP evals

**MCP controllability evals completed (10 assets):**
- DOTween Pro (Medium-High, 4 tools), Behavior Designer Pro (Medium-High, 4 tools), SensorToolkit 2 (High, 5 tools), UCC (High, 5 tools), A* Pathfinding Pro (High, 6 tools), Master Audio 2024 (High, 6 tools), Dialogue System for Unity (High, 6 tools)
- Not candidates: BD Senses Pack (Low), Procedural Dialogue Addon (Low), Follow & Protect Agent (Low)
- Deferred: GOAP v3 (Medium), Breeze (Medium-High)

**Master Audio (6 tools):** `#if HAS_MASTERAUDIO`, Assembly-CSharp
- `ma-query` -- List groups, buses, playlists, master volume, mute state, playing variations
- `ma-play` -- Play sound by group name (2D or 3D with position)
- `ma-group-control` -- Mute/unmute/solo/pause/stop/fade sound groups
- `ma-bus-control` -- Mute/unmute/solo/pause/stop/fade/pitch buses
- `ma-playlist` -- Play/stop/next/prev/random/pause/mute/fade/change playlists
- `ma-configure-ducking` -- Add/remove groups from music duck list

**A* Pathfinding (6 tools):** `HAS_ASTAR`, `MCPTools.AStarPathfinding.Editor`
- `astar-query` -- Graph list (type, nodes, dimensions) + AI agent state
- `astar-configure-grid` -- GridGraph dimensions, nodeSize, center, slope, step, erosion
- `astar-configure-recast` -- RecastGraph characterRadius, walkableHeight, cellSize, tiling
- `astar-configure-agent` -- AIPath/AILerp/RichAI speed, rotation, reach, slowdown
- `astar-scan` -- Trigger Scan() or runtime UpdateGraphs with bounds/penalty/walkability
- `astar-configure-seeker` -- Seeker tag masks, graph masks, cost multipliers

**Dialogue System (6 tools):** `#if HAS_DIALOGUE_SYSTEM`, Assembly-CSharp
- `ds-query` -- List conversations, actors, variables, quests from database + active state
- `ds-conversation` -- Start/stop/stopall/check conversations by title
- `ds-quest` -- Get/set quest states and entry states
- `ds-variable` -- Get/set Lua dialogue variables
- `ds-bark` -- Trigger barks from conversation or raw text
- `ds-lua` -- Execute arbitrary Lua code

**SensorToolkit 2 (5 tools):** `HAS_SENSORTOOLKIT`, `MCPTools.SensorToolkit.Editor`
- `sensor-query` -- List all sensors on GO with config (shape, layers, pulse, detections)
- `sensor-add-range` -- Add/configure RangeSensor or RaySensor
- `sensor-add-los` -- Add/configure LOSSensor with view angle/distance limits
- `sensor-configure-steering` -- Configure SteeringSensor seek/interest/danger
- `sensor-query-detections` -- Runtime: query detections sorted by distance/strength

**UCC (5 tools):** `HAS_UCC`, `MCPTools.UCC.Editor` (100% reflection-based)
- `uc-query` -- Full character state (locomotion, abilities, items, attributes)
- `uc-configure-locomotion` -- Mass, gravity, skin width
- `uc-ability-control` -- List/enable/disable/start/stop abilities by type name
- `uc-configure-attribute` -- Set health/stamina/mana values
- `uc-item-control` -- List inventory, equip/unequip item sets

**Behavior Designer (4 tools):** `HAS_BEHAVIOR_DESIGNER`, `MCPTools.BehaviorDesigner.Editor`
- `bd-query` -- Tree name, enabled, shared variables with values
- `bd-set-variable` -- Get/set SharedVariable by name (bool/float/int/string/vector3)
- `bd-control` -- Start/stop/restart behavior tree
- `bd-list-trees` -- List all BehaviorTree components in scene

**DOTween Pro (4 tools):** `#if HAS_DOTWEEN`, Assembly-CSharp
- `dotween-query` -- List DOTweenAnimation components with full config
- `dotween-add-animation` -- Add/configure DOTweenAnimation (type, ease, duration, endValue)
- `dotween-play` -- Runtime: play/pause/rewind/restart/complete/kill by id
- `dotween-global` -- Global DOTween control (killall/pauseall/playall/completeall)

**Unity Entities (5 tools):** `HAS_UNITY_ENTITIES`, `MCPTools.UnityEntities.Editor`
- `ecs-query-worlds` -- List all active Worlds (name, entity count, system count, default flag)
- `ecs-query-entities` -- Query entities by component types, list up to 50 with component lists
- `ecs-inspect-entity` -- Inspect entity by index+version, read all component field values via reflection
- `ecs-modify-entity` -- Modify IComponentData field on entity via reflection (supports float3, quaternion, enums)
- `ecs-create-destroy` -- Create entity with component types or destroy by index+version

**Unity Physics (4 tools):** `HAS_UNITY_PHYSICS`, `MCPTools.UnityPhysics.Editor`
- `uphys-query` -- Read Rigidbody, Colliders, PhysicsStepAuthoring on a GO with full config
- `uphys-configure-body` -- Add/configure Rigidbody (mass, damping, gravity, kinematic, constraints, Unity 6 API)
- `uphys-configure-step` -- Add/configure PhysicsStepAuthoring (gravity, solver, substeps, broadphase, threading)
- `uphys-configure-shape` -- Add/configure Collider (Box/Sphere/Capsule/Mesh + PhysicsMaterial + trigger)

**Compile fixes:** BD Pro tools rewritten against actual v2.x API (StartBehavior/StopBehavior, SharedVariables array, PropertyName-based variable access). Missing `using com.IvanMurzak.ReflectorNet.Utils;` added to all BD files. Asmdef fixed: moved BD Runtime from precompiledReferences to references, added GraphDesigner.Runtime.dll to precompiledReferences.

### Session 3 (Mar 20, 2026) -- Cinemachine + Animation Rigging + ALINE + Compile fix

Added 3 new tool groups (14 tools) + fixed Session 2 compile errors.

### Session 2 (Mar 20, 2026) -- Rope Toolkit + Heathen + Feel + DNP

Added 5 new tool groups (23 tools).

### Session 1 (Mar 14, 2026) -- Malbers AC + Quest Forge + Retarget Pro

Added 3 new tool groups (13 tools).

### Session 0 (Pre-docs) -- Stable

All 35 tools built across Sandbox Sessions 52-55. Package created at v1.0.0.

---

## Reference

Full API reference: `TMCP_Reference.md` (this folder)
Sandbox eval: `Sandbox_AssetLog.md` ENTRY-267
MCP candidates tracking: `Sandbox_AssetLog.md` MCP Candidates section

---

**End of Document**
