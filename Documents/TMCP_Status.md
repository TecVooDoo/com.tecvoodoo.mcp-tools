# TecVooDoo MCP Tools -- Status

**Package:** `com.tecvoodoo.mcp-tools` v1.5.0
**Source (edit here):** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\` (edit directly in package)
**Package (UPM):** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\`
**Unity Requirement:** 6000.0+
**Last Updated:** March 30, 2026 (SpaceSucks Session 1)

> **Install:** Add to manifest.json: `"com.tecvoodoo.mcp-tools": "file:../../DefaultUnityPackages/com.tecvoodoo.mcp-tools"`
> Requires `com.ivanmurzak.unity.mcp` (MCP base) already installed.

---

## Current State

**~177 tools** across 37 asset groups. All compiling.

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
| **Behavior Designer** | **5** | `HAS_BEHAVIOR_DESIGNER` | None (`#if` only) | **Updated SS1** |
| **DOTween Pro** | **4** | `HAS_DOTWEEN` | None (`#if` only) | **New S4** |
| **Unity Entities** | **5** | `HAS_UNITY_ENTITIES` | `MCPTools.UnityEntities.Editor` | **New S4b** |
| **Unity Physics** | **4** | `HAS_UNITY_PHYSICS` | `MCPTools.UnityPhysics.Editor` | **New S4b** |
| **Bro Audio** | **4** | `HAS_BROAUDIO` | `MCPTools.BroAudio.Editor` | **New S5** |
| **Koreographer** | **2** | `HAS_KOREOGRAPHER` | `MCPTools.Koreographer.Editor` | **New S5** |
| **PMG** | **4** | `HAS_PMG` | None (`#if` only) | **New S5** |
| **Maestro MIDI** | **4** | `HAS_MAESTRO` | `MCPTools.Maestro.Editor` | **New S5** |
| **DryWetMIDI** | **1** | `HAS_DRYWETMIDI` | `MCPTools.DryWetMIDI.Editor` | **New S5** |
| **FMOD Studio** | **5** | `HAS_FMOD` | `MCPTools.FMOD.Editor` | **New S5** |
| **Chunity** | **4** | `HAS_CHUNITY` | None (`#if` only) | **New S5** |
| **Naninovel** | **5** | `HAS_NANINOVEL` | `MCPTools.Naninovel.Editor` | **New VNPC** |
| **Adventure Creator** | **5** | `HAS_ADVENTURE_CREATOR` | `MCPTools.AdventureCreator.Editor` | **New VNPC** |
| **Text Animator** | **4** | `HAS_TEXT_ANIMATOR` | `MCPTools.TextAnimator.Editor` | **New VNPC** |
| **Ink Integration** | **3** | `HAS_INK` | `MCPTools.InkIntegration.Editor` | **New VNPC** |

**Auto-detection:** `MCPToolsDefineManager.cs` (Editor folder) scans for installed assets on domain reload and adds/removes `HAS_*` defines automatically. No manual setup needed. When an asset is removed from a project, its tools silently deactivate.

---

## Package vs Source Sync

All 33 groups built directly in the package folder. No separate source location.

**Edit process:** Edit directly in `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\`. Assets without asmdef use `#if HAS_*` guards (FinalIK, PWB, Quest Forge, Rope Toolkit, Feel, Master Audio, Dialogue System, DOTween, PMG, Chunity, Behavior Designer). Assets with asmdef use `defineConstraints` for the same effect.

---

## Known Gotchas

| Gotcha | Details |
|--------|---------|
| RayFire crash | `DemolishForced()` and `ApplyDamage()` crash Unity when called from MCP context. Tools exist but should only be used for setup, not runtime testing. |
| Flexalon RequireComponent | `FlexalonLayoutBase` auto-adds `FlexalonObject` via `[RequireComponent]`. Use `GetComponent<FlexalonObject>()`, never `AddComponent<FlexalonObject>()` (returns null duplicate). |
| MagicaCloth MeshCloth | MeshCloth locks all verts. Must use `SkinnedMeshRenderer`, not `MeshRenderer`. |
| Asmdef pattern | Tools with asmdefs use `overrideReferences: true` + `ReflectorNet.dll`. Tools without asmdefs live in Assembly-CSharp-Editor and use `#if` guards. |
| Domain reload | MCP disconnects during domain reload after adding defines. Wait for auto-reconnect. |
| BD Pro v3.x API | BD Pro 3 uses `StartBehavior`/`StopBehavior(bool pause)`/`RestartBehavior` (same as Pro 2). `SharedVariables` is an array. `GetVariable` takes `PropertyName`. New: `StopBehavior` takes optional `pause` param, `RestartBehavior` returns `bool`, `Status`/`IsActive()`/`IsRunning()`/`IsPaused()` state queries, `Tick()` for manual update, `UpdateMode`/`EvaluationType` enums. |
| BD asmdef crash on uninstall | **FIXED (SS1).** Was: hard assembly refs in asmdef caused compile errors when BD removed. Fix: deleted asmdef, converted to `#if HAS_BEHAVIOR_DESIGNER` pattern like FinalIK/Feel/DOTween. |
| BD Pro 3 upgrade | BD Pro 3 (DOTS-powered, v3.0.0+) keeps same assembly names as Pro 2. MonoBehaviour API is backward compatible. Tools updated SS1 with BD3 enhancements: pause/unpause, Status/IsActive/IsRunning/IsPaused, UpdateMode, manual Tick. ECS overloads (`World, Entity`) exist but not yet exposed as separate tools. |
| UCC reflection | UCC tools use 100% reflection-based API access for resilience across versions. |
| DOTween detect type | DefineManager detects `DOTweenAnimation, DOTweenPro` (Pro DLL), not core `DOTween.dll`. |
| Agent API verification | Build agents must verify actual API signatures from source before writing code. Never assume from documentation knowledge or eval summaries. |
| Assets/Plugins/ assembly | Assets installed under `Assets/Plugins/` compile to `Assembly-CSharp-firstpass`, not `Assembly-CSharp`. `FindType` now has a fallback that searches ALL assemblies if the specified one misses. Fixed HOK Session 22 -- DS was in firstpass, define manager couldn't find it. |
| Cross-version API drift | Shared package compiles in every project. Asset version differences between projects cause compile errors. MA v1.0.4 renamed `PauseGroup`->`PauseSoundGroup`, changed `PlaySoundAndForget` return from `PlaySoundResult`->`bool`. DS v2.2.68 changed `GetQuestDescription` second param from `string`->`QuestState` enum. Fixed HOK Session 22. |

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

### SpaceSucks Session 1 (Mar 30, 2026) -- BD Pro 3 upgrade + asmdef crash fix

**Asmdef crash fix (CLOSED BUG):** Deleted `MCPTools.BehaviorDesigner.Editor.asmdef`. BD tools now use `#if HAS_BEHAVIOR_DESIGNER` pattern (Assembly-CSharp-Editor) like FinalIK/Feel/DOTween. Removing BD from a project no longer causes compile errors.

**BD Pro 3 API update:** All 4 existing tools updated for BD Pro 3.0.0 (DOTS-powered). MonoBehaviour API is backward compatible with BD Pro 2.

**BD tool changes:**
- `bd-control` -- Added `pause` and `unpause`/`resume` actions (BD3 `StopBehavior(pause: true)` + `IsPaused()`)
- `bd-query` -- Added `UpdateMode`, `EvaluationType`, `MaxEvaluationCount` properties. Added runtime state section (play mode only): `Status` (TaskStatus enum), `IsActive`, `IsRunning`, `IsPaused`
- `bd-list-trees` -- Added `Status` to play mode listing
- `bd-set-variable` -- No API changes needed (PropertyName + SetVariableValue<T> unchanged)

**New tool:**
- `bd-tick` -- Manual tick for trees with `UpdateMode.Manual`. Takes optional `count` param. Play mode only.

**Tool count:** 4 -> 5 tools.

---

### HOK Session 22 (Mar 29, 2026) -- FindType fix + MA/DS API fixes

**No new tools built.** Infrastructure fix that unblocked existing tools in HOK.

**FindType assembly fallback:** `MCPToolsDefineManager.FindType()` now has a second pass that searches ALL loaded assemblies if the specified assembly doesn't contain the type. Root cause: Dialogue System installs to `Assets/Plugins/Pixel Crushers/` which compiles to `Assembly-CSharp-firstpass`, but the detection entry specified `Assembly-CSharp`. This is a permanent fix -- any asset that lands in an unexpected assembly (e.g. `firstpass` due to Plugins/ folder) will now be detected correctly.

**Master Audio API fixes (v1.0.4 compatibility):**
- `PauseGroup` -> `PauseSoundGroup`, `UnpauseGroup` -> `UnpauseSoundGroup`
- `PlaySoundAndForget` / `PlaySound3DAtVector3AndForget` return `bool` (not `PlaySoundResult`)
- `AddSoundGroupToDuckList` takes individual params (not `DuckGroupInfo` struct)
- `GetBusByName` removed -- simplified bus query to name-only listing
- `TriggerPreviousPlaylistClip` does not exist -- returns warning message

**Dialogue System API fix (v2.2.68 compatibility):**
- `QuestLog.GetQuestDescription(questName, "success")` -> `GetQuestDescription(questName, QuestState.Success)` (second param is `QuestState` enum, not string)

**Defines activated in HOK:** `HAS_DIALOGUE_SYSTEM`, `HAS_MASTERAUDIO`, `HAS_DOTWEEN` (all newly detected thanks to FindType fix). DS, MA, and DOTween tools now compile and surface as MCP skills in HOK.

---

### VNPC Session 6 (Mar 23, 2026) -- 4 narrative/VN tool groups (17 tools)

Narrative and visual novel assets from the VNPC project. All use asmdef with `defineConstraints`.

**Naninovel (5 tools):** `HAS_NANINOVEL`, `MCPTools.Naninovel.Editor`
- `nani-list-characters` -- List registered characters (ID, display name, colors, poses) via `Configuration.GetOrDefault<CharactersConfiguration>()`
- `nani-list-backgrounds` -- List registered backgrounds via `Configuration.GetOrDefault<BackgroundsConfiguration>()`
- `nani-list-scripts` -- Find all .nani files in project via AssetDatabase
- `nani-read-script` -- Read .nani script content by name (partial match, line-numbered output)
- `nani-list-commands` -- Discover all Command subclasses via reflection, show alias (`@char`, `@back`, etc.)

**Adventure Creator (5 tools):** `HAS_ADVENTURE_CREATOR`, `MCPTools.AdventureCreator.Editor`
- `ac-query-managers` -- Status of all 8 managers (assigned?, item/variable/action counts)
- `ac-list-inventory` -- Items, categories, recipes, documents from InventoryManager
- `ac-list-variables` -- Global variables (ID, label, type) from VariablesManager
- `ac-list-actions` -- Available Action types (title, filename) from ActionsManager, with filter
- `ac-find-scene-objects` -- Find Hotspots, NPCs, Players, Markers, ActionLists in current scene

**Text Animator (4 tools):** `HAS_TEXT_ANIMATOR`, `MCPTools.TextAnimator.Editor`
- `ta-list-effects` -- Discover all effects via `[EffectInfoAttribute]` reflection (behaviors/appearances, tag IDs)
- `ta-find-components` -- Find TextAnimatorComponentBase instances in scene (type, loop mode, text)
- `ta-list-databases` -- Find AnimationsDatabase assets via AssetDatabase
- `ta-get-settings` -- Find AnimatorSettingsScriptable assets via AssetDatabase

**Ink Integration (3 tools):** `HAS_INK`, `MCPTools.InkIntegration.Editor`
- `ink-list-files` -- List .ink files via InkLibrary (master/include, auto-compile, compiled status)
- `ink-compile` -- Compile specific file or all master files via InkCompiler
- `ink-get-story-info` -- Load compiled JSON, report variables, content lines, choice points, global tags

### Session 5 (Mar 22, 2026) -- 7 audio tool groups (24 tools)

Audio packages rated High in Sandbox AssetLog. All are optional -- tools auto-activate only when the target asset is installed.

**Bro Audio (4 tools):** `HAS_BROAUDIO`, `MCPTools.BroAudio.Editor`
- `bro-query` -- List all AudioEntity assets via `Resources.FindObjectsOfTypeAll<AudioEntity>()`; reports name, playing state
- `bro-play` -- `BroAudio.Play(new SoundID(entity), fadeIn)` with optional 3D position; lookup by entity name
- `bro-stop` -- `BroAudio.Stop(soundId, fadeOut)` or stop all entities by `BroAudioType` enum name; lookup by entity name
- `bro-volume` -- `BroAudio.SetVolume(soundId/type, volume, fadeTime)`; lookup by entity name

**Koreographer (2 tools):** `HAS_KOREOGRAPHER`, `MCPTools.Koreographer.Editor` (compiled DLL)
- `koreo-query` -- `Koreographer.Instance.GetAllLoadedKoreography()`, clip names, event track IDs, current beat time
- `koreo-beattime` -- `Koreographer.GetBeatTime(trackName, subdivision)` + `GetBeatTimeDelta()`

**PMG (4 tools):** `#if HAS_PMG`, Assembly-CSharp (MonoBehaviour, no static singleton)
- `pmg-query` -- `FindFirstObjectByType<MusicGenerator>()`, `GeneratorState`, `Tempo/KeySteps/Scale/Mode`, `AutoPlay`
- `pmg-play` -- `gen.SetState(GeneratorState.Playing)`
- `pmg-stop` -- `gen.SetState(GeneratorState.Stopped)`
- `pmg-configure` -- Sets `cfg.Tempo`, `cfg.KeySteps`, `cfg.Scale`, `cfg.Mode` (enum parse)

**Maestro MIDI (4 tools):** `HAS_MAESTRO`, `MCPTools.Maestro.Editor`
- `maestro-query` -- `FindObjectsByType<MidiFilePlayer/MidiStreamPlayer>()`, name, MPTK_MidiName, IsPlaying, Tempo, Speed, Loop
- `maestro-play` -- Finds by GO name, sets MPTK_MidiName/Loop/Speed, calls `MPTK_Play()`
- `maestro-stop` -- `MPTK_Stop()` on named player
- `maestro-send-note` -- `MPTKEvent { Command=NoteOn, Value, Channel, Velocity, Duration, Patch }` via `MPTK_PlayEvent()`

**DryWetMIDI (1 tool):** `HAS_DRYWETMIDI`, `MCPTools.DryWetMIDI.Editor` (compiled DLL)
- `midi-query-devices` -- `OutputDevice.GetAll()` + `InputDevice.GetAll()`, disposes each, wrapped in try/catch

**FMOD Studio (5 tools):** `HAS_FMOD`, `MCPTools.FMOD.Editor`
- `fmod-query` -- IsInitialized, HaveAllBanksLoaded, HaveMasterBanksLoaded, IsMuted, AnyBankLoading()
- `fmod-play` -- `RuntimeManager.PlayOneShot(eventPath, pos)` with optional 3D position parse
- `fmod-parameter` -- `RuntimeManager.StudioSystem.setParameterByName(name, value, ignoreSeekSpeed)`
- `fmod-vca` -- `RuntimeManager.GetVCA(path).setVolume(volume)` with before/after logging
- `fmod-bus` -- `RuntimeManager.GetBus(path).setVolume(volume)` with before/after logging

**Chunity (4 tools):** `#if HAS_CHUNITY`, Assembly-CSharp
- `chuck-query` -- `FindObjectsByType<ChuckMainInstance>()`, lists GO names and active state
- `chuck-run` -- `inst.RunCode(code)` on named ChuckMainInstance
- `chuck-set-float` -- `Chuck.Manager.SetFloat(instanceName, variableName, value)`
- `chuck-set-int` -- `Chuck.Manager.SetInt(instanceName, variableName, value)`

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

**Behavior Designer (4 tools, updated SS1 to 5):** `HAS_BEHAVIOR_DESIGNER`, `#if` only (asmdef removed SS1)
- `bd-query` -- Tree name, enabled, shared variables with values
- `bd-set-variable` -- Get/set SharedVariable by name (bool/float/int/string/vector3)
- `bd-control` -- Start/stop/pause/unpause/restart behavior tree
- `bd-list-trees` -- List all BehaviorTree components in scene
- `bd-tick` -- Manual tick for Manual update mode trees (added SS1)

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
