# TecVooDoo MCP Tools -- Status

**Package:** `com.tecvoodoo.mcp-tools` v1.12.0
**Source (edit here):** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\` (edit directly in package)
**Package (UPM):** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\`
**Unity Requirement:** 6000.0+
**MCP Compatibility:** **Self-syncing across MCP versions.** As of 2026-05-10 (Session 7), [`Editor/MCPToolsAsmdefSync.cs`](../Editor/MCPToolsAsmdefSync.cs) auto-rewrites every TMCP tool-group asmdef's `precompiledReferences` on each domain reload to match whatever `McpPlugin*.dll` / `McpPlugin.Common*.dll` / `ReflectorNet*.dll` filenames exist under `Assets/Plugins/NuGet/`. So a fresh MCP version bump (whether the new release ships `McpPlugin.dll`, `McpPlugin.6.2.1.dll`, `McpPlugin.7.0.0.dll`, or anything else) self-heals on first compile. Manual fallback: **Tools > TecVooDoo > Sync MCP DLL References**. The 46 asmdefs ship with a static fallback list covering MCP 0.66.x / 0.69.x / 0.71.0 / 0.72.0 conventions so the very first compile after install also succeeds. **Projects on MCP 0.66.1 must still upgrade MCP first** before reinstalling TMCP — see [Sandbox/Documents/MCP_ConnectionBrief.md](../../../Sandbox/Documents/MCP_ConnectionBrief.md) for the per-project recipe.
**Last Updated:** June 25, 2026 (**DOTween asmdef REVERTED (Session 24 follow-up, SetDesign canary): DOTween is the one tool group that cannot be cleanly asmdef'd** — DOTween Pro's `DOTweenAnimation` compiles into the predefined `Assembly-CSharp` in projects where DOTween's "Create ASMDEF" option is off (SetDesign), and an asmdef cannot reference `Assembly-CSharp`, so `MCPTools.DOTween.Editor` failed there with CS0246. Reverted to `#if HAS_DOTWEEN` in `Assembly-CSharp-Editor` (which CAN see `Assembly-CSharp`); the hardened postprocessor keeps this lone leaky group safe. 10 of the 11 leaky folders remain asmdef-isolated. Proper future fix: reflection-based `DOTweenAnimation` access so DOTween can be isolated too. **Prior same-session: Postprocessor hardening: `MCPToolsAssetPostprocessor.OnPostprocessAllAssets` now treats folder deletions (extension-less paths) as relevant -- the common Asset-Store/UPM removal shape Unity reports as a bare folder path, which the old `.asmdef`/`.cs`/`.dll`-only gate skipped, so `RemoveStaleDefines` never ran and the `HAS_*` define stuck. Also added a `File.Exists` guard in the asmdef presence scan to defeat AssetDatabase staleness during the postprocessor window. Functionally verified: folder-path input runs clean, no valid define stripped. This closes the second half of the stale-`HAS_*`-define hardening; together with the asmdef-ification it ends the leaky-folder/stale-define friction class.** **Prior same-session: leaky-folder hardening -- all 11 tool groups that shipped without an asmdef now have one -- `MCPTools.{DOTween,BehaviorDesigner,BoingKit,BridgeBuilder25D,Feel,JuicyActions,Lumen,MudBun,Terrain25D,Timeflow,uLipSync}.Editor`. They previously compiled into the global `Assembly-CSharp-Editor` via global `HAS_*` defines, so a stale define while the asset was absent would have failed every editor script project-wide. Now each is define-constraint-isolated like the other 45 groups. DOTween was the only one with its asset installed in TVD (compile-verified clean; `Tool_DOTween` confirmed moved to `MCPTools.DOTween.Editor`, no `Tool_*` left in `Assembly-CSharp-Editor`); the other 10 are define-excluded in TVD's lean set and validate downstream. **Every tool group now carries an asmdef -- the "leaky folders" class is closed.** Prior: June 18 -- Retarget Pro V5 batch-bake API-break fix, TMCP commit `990c230`, driven by Sandbox eval ENTRY-376)

> **Install:** Add to manifest.json: `"com.tecvoodoo.mcp-tools": "file:../../DefaultUnityPackages/com.tecvoodoo.mcp-tools"`
> Requires `com.ivanmurzak.unity.mcp` (MCP base) already installed.

---

## Current State

**245 tools across 56 asset groups** — grep-verified ground truth as of 2026-06-04 (`[McpPluginTool(` attributes = 245; `[McpPluginToolType]` markers = 56, one per group folder). This supersedes the prior "~259" running tally, which had drifted ~14 high over many sessions. Recent retirements reflected here: AI Navigation retired Session 17 2026-06-04 (superseded by Ivan-Murzak's official `navigation-*` 10-tool Extension; 4 `nav-*` tools removed); Cinemachine retired Session 16 2026-06-04 (superseded by official `cinemachine-*` Extension; 5 `cm-*` tools removed). Asset Inventory was removed back in Session 6 but lingered as a stale table row + orphaned `HAS_ASSETINVENTORY` define until the 2026-06-04 audit dropped both. All compiling.

| Group | Tools | Define | Asmdef | Status |
|-------|-------|--------|--------|--------|
| Flexalon | 7 | `HAS_FLEXALON` | `MCPTools.Flexalon.Editor` | Stable |
| Prefab World Builder | 4 | `HAS_PWB` | `MCPTools.PWB.Editor` | **Updated TVD1** |
| RayFire | 8 | `HAS_RAYFIRE` | `MCPTools.RayFire.Editor` | Stable |
| MagicaCloth 2 | 7 | `HAS_MAGICACLOTH2` | `MCPTools.MagicaCloth2.Editor` | Stable |
| Final IK | 5 | `HAS_FINALIK` | `MCPTools.FinalIK.Editor` | **Updated TVD1** |
| Malbers AC | 8 | `HAS_MALBERS_AC` | `MCPTools.MalbersAC.Editor` | Stable |
| Quest Forge | 5 | `HAS_MALBERS_QUESTFORGE` | `MCPTools.QuestForge.Editor` | **Updated TVD1** |
| Retarget Pro | 4 | `HAS_RETARGETPRO` | `MCPTools.RetargetPro.Editor` | Stable |
| Rope Toolkit | 5 | `HAS_ROPE_TOOLKIT` | `MCPTools.RopeToolkit.Editor` | **Updated TVD1** |
| Heathen Physics | 5 | `HAS_HEATHEN_PHYSICS` | `MCPTools.HeathenPhysics.Editor` | Stable |
| Heathen Ballistics | 5 | `HAS_HEATHEN_BALLISTICS` | `MCPTools.HeathenBallistics.Editor` | Stable |
| Feel | 4 | `HAS_FEEL` | `MCPTools.Feel.Editor` | Stable, asmdef-ified S24 |
| Damage Numbers Pro | 4 | `HAS_DAMAGE_NUMBERS_PRO` | `MCPTools.DamageNumbersPro.Editor` | Stable |
| Animation Rigging | 5 | `HAS_ANIMATION_RIGGING` | `MCPTools.AnimationRigging.Editor` | Stable |
| ALINE | 4 | `HAS_ALINE` | `MCPTools.ALINE.Editor` | Stable |
| **Master Audio** | **6** | `HAS_MASTERAUDIO` | `MCPTools.MasterAudio.Editor` | **New S4, Updated TVD1** |
| **A* Pathfinding** | **6** | `HAS_ASTAR` | `MCPTools.AStarPathfinding.Editor` | **New S4** |
| **Dialogue System** | **6** | `HAS_DIALOGUE_SYSTEM` | `MCPTools.DialogueSystem.Editor` | **New S4, Updated TVD1** |
| **SensorToolkit 2** | **5** | `HAS_SENSORTOOLKIT` | `MCPTools.SensorToolkit.Editor` | **New S4** |
| **UCC (Opsive)** | **5** | `HAS_UCC` | `MCPTools.UCC.Editor` | **New S4** |
| **Behavior Designer** | **5** | `HAS_BEHAVIOR_DESIGNER` | `MCPTools.BehaviorDesigner.Editor` | Updated SS1, asmdef-ified S24 |
| **DOTween Pro** | **4** | `HAS_DOTWEEN` | None (`#if` only) | New S4; **asmdef reverted S24** — DOTween Pro's `DOTweenAnimation` can compile into the predefined `Assembly-CSharp` (when DOTween's "Create ASMDEF" is off), which an asmdef cannot reference. Stays `#if`-guarded in `Assembly-CSharp-Editor`; kept safe by the hardened postprocessor. Proper fix = reflection-based `DOTweenAnimation` access (deferred). |
| **Unity Entities** | **5** | `HAS_UNITY_ENTITIES` | `MCPTools.UnityEntities.Editor` | **New S4b** |
| **Unity Physics** | **4** | `HAS_UNITY_PHYSICS` | `MCPTools.UnityPhysics.Editor` | **New S4b** |
| **Bro Audio** | **4** | `HAS_BROAUDIO` | `MCPTools.BroAudio.Editor` | **New S5** |
| **Koreographer** | **2** | `HAS_KOREOGRAPHER` | `MCPTools.Koreographer.Editor` | **New S5** |
| **PMG** | **4** | `HAS_PMG` | `MCPTools.PMG.Editor` | **New S5, Updated TVD1** |
| **Maestro MIDI** | **4** | `HAS_MAESTRO` | `MCPTools.Maestro.Editor` | **New S5** |
| **DryWetMIDI** | **1** | `HAS_DRYWETMIDI` | `MCPTools.DryWetMIDI.Editor` | **New S5** |
| **FMOD Studio** | **5** | `HAS_FMOD` | `MCPTools.FMOD.Editor` | **New S5** |
| **Chunity** | **4** | `HAS_CHUNITY` | `MCPTools.Chunity.Editor` | **New S5, Updated TVD1** |
| **Naninovel** | **5** | `HAS_NANINOVEL` | `MCPTools.Naninovel.Editor` | **New VNPC** |
| **Adventure Creator** | **5** | `HAS_ADVENTURE_CREATOR` | `MCPTools.AdventureCreator.Editor` | **New VNPC** |
| **Text Animator** | **4** | `HAS_TEXT_ANIMATOR` | `MCPTools.TextAnimator.Editor` | **New VNPC** |
| **Ink Integration** | **3** | `HAS_INK` | `MCPTools.InkIntegration.Editor` | **New VNPC** |
| **2.5D Terrain** | **3** | `HAS_TERRAIN25D` | `MCPTools.Terrain25D.Editor` | New S63, asmdef-ified S24 |
| **2.5D Bridge Builder** | **4** | `HAS_BRIDGEBUILDER25D` | `MCPTools.BridgeBuilder25D.Editor` | New S63, asmdef-ified S24 |
| **Decal Collider** | **3** | `HAS_DECAL_COLLIDER` | `MCPTools.DecalCollider.Editor` | **New TVD1** |
| **Texture Studio** | **3** | `HAS_TEXTURE_STUDIO` | `MCPTools.TextureStudio.Editor` | **New TVD1** |
| **Animancer Pro** | **4** | `HAS_ANIMANCER` | `MCPTools.Animancer.Editor` | **New TVD1** |
| **Juicy Actions** | **2** | `HAS_JUICY_ACTIONS` | `MCPTools.JuicyActions.Editor` | New TVD2, asmdef-ified S24 |
| **Boing Kit** | **2** | `HAS_BOINGKIT` | `MCPTools.BoingKit.Editor` | New TVD2, asmdef-ified S24 |
| **MudBun** | **3** | `HAS_MUDBUN` | `MCPTools.MudBun.Editor` | New TVD2, asmdef-ified S24 |
| **Lumen** | **2** | `HAS_LUMEN` | `MCPTools.Lumen.Editor` | New TVD2, asmdef-ified S24 |
| **Timeflow** | **4** | `HAS_TIMEFLOW` | `MCPTools.Timeflow.Editor` | New TVD2, asmdef-ified S24 |
| **uLipSync** | **3** | `HAS_ULIPSYNC` | `MCPTools.uLipSync.Editor` | New M3 S2, asmdef-ified S24 |
| **Technie Collider Creator 2** | **6** | `HAS_TCC` | `MCPTools.TCC.Editor` | **New TVD3** |
| **MK Edge Detection** | **4** | `HAS_MK_EDGE` | `MCPTools.MKEdge.Editor` | **New TVD3** |
| **Real Time Weather Pro** | **3** | `HAS_RTW` | `MCPTools.RTW.Editor` (reflection) | **New TVD3** |
| **Ultimate Terrain** | **3** | `HAS_ULTIMATE_TERRAIN` | `MCPTools.UltimateTerrain.Editor` | **New TVD3** |
| **PressE PRO 2** | **4** | `HAS_PRESSE` | `MCPTools.PressE.Editor` (reflection) | **New TVD3** |
| **COZY 3 Stylized Weather** | **5** | `HAS_COZY` | `MCPTools.Cozy.Editor` | **New TVD4** |
| **Modular 3D Text** | **6** | `HAS_M3DT` | `MCPTools.M3DText.Editor` (reflection) | **New TVD5** |
| **ORK Framework + Makinom** | **7** | `HAS_ORK` | `MCPTools.ORK.Editor` (reflection) | **New TVD5** |
| **CityGen3D** | **6** | `HAS_CITYGEN3D` | `MCPTools.CityGen3D.Editor` (reflection) | **New TVD5** |
| **UMotion Pro** | **4** | `HAS_UMOTION_PRO` | `MCPTools.UMotionPro.Editor` (DLL refs `UMotionApplication.dll` + `UMotionEditor.dll`; editor-only static API in `UMotionEditor.API`) | **New TVD10** |

**Auto-detection:** `MCPToolsDefineManager.cs` (Editor folder) scans for installed assets on domain reload and adds/removes `HAS_*` defines automatically. No manual setup needed. When an asset is removed from a project, its tools silently deactivate.

---

## Package vs Source Sync

All 33 groups built directly in the package folder. No separate source location.

**Edit process:** Edit directly in `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\`. Three compilation patterns:
1. **Asmdef with direct refs** -- asset has its own assembly (e.g., AnimationRigging, Animancer, Feel, DOTween, Terrain25D, BridgeBuilder25D, BehaviorDesigner). Asmdef references asset assembly + MCP assemblies, uses `defineConstraints`.
2. **Asmdef with reflection** -- asset is in Assembly-CSharp (no asmdef). Asmdef references only MCP assemblies, all asset type access via reflection (e.g., DecalCollider, TextureStudio, MasterAudio, DialogueSystem, RopeToolkit, FinalIK, PWB, QuestForge, PMG, Chunity).
3. **`#if HAS_*` guards** -- legacy pattern, still used by Feel, DOTween, Terrain25D, BridgeBuilder25D, BehaviorDesigner as belt-and-suspenders alongside their asmdefs. Works in local UPM packages. `MainThread` is in `com.IvanMurzak.ReflectorNet.Utils` (ReflectorNet.dll), NOT `com.IvanMurzak.Unity.MCP.Editor.Utils`.

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
| Texture Studio Render | `CompositeMap.CreateMapTexture` takes 4 params `(int, int, bool mipmaps, bool linear)`, not 2. **Fixed SD Session 5** -- added missing `false, false` args. |
| Assets/Plugins/ assembly | Assets installed under `Assets/Plugins/` compile to `Assembly-CSharp-firstpass`, not `Assembly-CSharp`. `FindType` now has a fallback that searches ALL assemblies if the specified one misses. Fixed HOK Session 22 -- DS was in firstpass, define manager couldn't find it. |
| Cross-version API drift | Shared package compiles in every project. Asset version differences between projects cause compile errors. MA v1.0.4 renamed `PauseGroup`->`PauseSoundGroup`, changed `PlaySoundAndForget` return from `PlaySoundResult`->`bool`. DS v2.2.68 changed `GetQuestDescription` second param from `string`->`QuestState` enum. Fixed HOK Session 22. |
| Stale `HAS_*` defines after UPM-style asset removal | **Logged TVD11 (2026-05-23).** `MCPToolsAssetPostprocessor.OnPostprocessAllAssets` does NOT reliably catch UPM Package Manager removals — its `path.EndsWith(".asmdef"\|".cs"\|".dll")` heuristic misses the deleted paths and the stale `HAS_X` define stays in `PlayerSettings`. On next compile, the integration's `defineConstraints`-gated asmdef compiles against a now-missing namespace and the project breaks with `CS0246`. `MCPToolsDefineManager` `[InitializeOnLoad]` cannot recover because the broader compile-error cascade prevents `MCPTools.Editor` itself from loading (despite having no `defineConstraints` of its own). Menu **Tools > TecVooDoo > Rescan MCP Defines** is therefore unreachable from this state. **Recovery:** inline-mirror the `Entries` table + `FindType()` from [MCPToolsDefineManager.cs](../Editor/MCPToolsDefineManager.cs) inside a `script-execute` call — Roslyn dynamic compile runs outside the project's asmdef graph and can write `PlayerSettings.SetScriptingDefineSymbols` even when MCPTools.Editor is offline. Worst-case manifests for the 11 asmdef-less integrations (next row) because their `#if HAS_*`-guarded code leaks into `Assembly-CSharp-Editor`. |
| 11 integration folders without asmdefs leak into Assembly-CSharp-Editor | **Logged TVD11 (2026-05-23).** Folders: BehaviorDesigner, Feel, DOTween, Terrain25D, BridgeBuilder25D, JuicyActions, BoingKit, MudBun, Lumen, uLipSync, Timeflow. The tool-group table above flags these with `None (#if only)` in the Asmdef column. Their `#if HAS_*` source guard is the only protection — `defineConstraints` cannot intervene because there's no asmdef to set them on. When their `HAS_X` define lingers stale (see prior row), the `#if`-guarded code compiles inside `Assembly-CSharp-Editor` and the whole project's Editor assembly breaks, not just a tool group. **Follow-up:** give each its own asmdef matching the rest of TMCP (~33 folders already do). Pattern to copy depends on whether the third-party API ships as asmdef (`references`-based) or DLL (`precompiledReferences`-based) — see Session 8 for the DLL pattern with DryWetMIDI/Koreographer. |

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

### Retarget Pro V5 batch-bake fix (June 18, 2026) -- API-break recovery (Sandbox eval ENTRY-376)

Driven by a Sandbox-session re-eval of Retarget Pro (Kinemation) at **V5**, logged as `Sandbox_AssetLog.md` ENTRY-376 (re-eval of ENTRY-243's 4.2.1; 243 stays). V5 is API-breaking against the `retarget-batch-bake` tool, and the break took the **entire MCP plugin offline**, not just the Retarget Pro group: a `CS0246` in [Tool_RetargetPro.BatchBake.cs](../RetargetPro/Editor/Tool_RetargetPro.BatchBake.cs) failed the `MCPTools.RetargetPro.Editor` assembly, cascading to all tools.

**Three V5 API changes:**
- `RetargetAnimBaker` moved namespace `KINEMATION.RetargetPro.Editor` -> `KINEMATION.RetargetPro.Editor.Scripts.Bakers`.
- The static `_savePath` field was removed; bake output now reads from the serialized `RetargetProfile.saveFolderPath`.
- The `_keyframeAll` baker option was removed entirely.

**Fix (TMCP commit `990c230`):**
- Added `using KINEMATION.RetargetPro.Editor.Scripts.Bakers;`.
- Output folder now overrides `profile.saveFolderPath` for the run and restores the original afterward (no permanent profile mutation).
- Dropped the dead `_keyframeAll` reflection; the `keyframeAll` param is retained for signature stability but is now a documented no-op against V5.
- Removed the now-unused `SetPrivateStaticField` helper.

Tool count unchanged -- Retarget Pro stays 4 tools, table row stays Stable. **Live-verified V5 resident in TVD:** `RetargetAnimBaker` resolves in the V5 `...Editor.Scripts.Bakers` namespace (assembly `RetargetPro.Editor`), `RetargetProfile.saveFolderPath` present, old V4 namespace gone, `MCPTools.RetargetPro.Editor.Tool_RetargetPro` compiles. Batch-bake functional test PASSED per the Sandbox carryover. Cross-project ownership: the eval + AssetLog entry are Sandbox-owned; this source fix + status entry are TMCP-repo-owned.

### TecVooDoo Session 17 (June 4, 2026) -- AI Navigation tool group retired + MCPToolsAssetPostprocessor hardening

Same-day follow-on to Session 16's Cinemachine retirement. Ivan-Murzak shipped a wave of 6 NEW MCP Extensions in a release that landed shortly after the Cinemachine drop: `inputsystem-*` (13 tools), `navigation-*` (10 tools), `splines-*` (13 tools), `terrain-*` (15 tools), `tilemap-*` (13 tools), `timeline-*` (12 tools). MCP Extension count: 4 → **10** in one cycle.

**Mapping against TMCP for the retire-on-publish precedent:** only `navigation-*` overlaps an existing TMCP tool group (`MCPTools.AINavigation.Editor` / `HAS_AI_NAVIGATION` / 4 `nav-*` tools). The other 5 new Extensions cover Unity built-ins that TMCP never integrated (InputSystem / Splines / built-in Terrain / Tilemap / Timeline) — no TMCP-side action needed for those.

**Decision: retire TMCP's AI Navigation group**, same call as Cinemachine for the same reasons. Ivan's `navigation-*` is 10 tools (`navigation-agent-add/set-destination`, `-get`, `-link-add`, `-list`, `-modifier-add/-volume-add`, `-modify`, `-set-bake-settings`, `-surface-add/bake`) — broader and more granular than TMCP's 4-tool surface (`nav-query`, `nav-configure-surface`, `nav-bake`, `nav-configure-link`). Updates the precedent's first application: **two TMCP tool groups retired same day via the same procedure**, validating the 8-step template captured in memory [[project-tmcp-retire-when-ivan-publishes]].

**Retirement footprint:**
- Deleted `AINavigation/` folder + `AINavigation.meta` (4 `.cs` + 4 `.cs.meta` + 1 `.asmdef` + 1 `.asmdef.meta` + 1 folder `.meta` + 1 root `.meta` = 12 files).
- Removed `("HAS_AI_NAVIGATION", "Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation")` from [MCPToolsDefineManager.cs](../Editor/MCPToolsDefineManager.cs) `Entries[]`. (Was line 82 post-Cinemachine.)
- Removed AI Navigation row from this doc's tool-group table; tool count adjusted from ~263/58 to ~259/57.
- Historical Session TVD1 entry referencing AI Navigation's original addition (this doc § Session 4 area) intentionally LEFT INTACT — historical record stays.

**Per-project stale-define caveat** (same as Cinemachine): removing the `Entries[]` line doesn't auto-strip `HAS_AI_NAVIGATION` from `PlayerSettings`. Optional cleanup via `Tools > TecVooDoo > Rescan MCP Defines` or manual strip.

**MCPToolsAssetPostprocessor hardening also addressed this session — see Session 17 entry in TVD_Status.md for full context.** The hardening makes UPM-style asset removals reliably strip stale `HAS_*` defines, closing the recurring failure mode that triggered manual `script-execute` recoveries during Session 11 (Koreographer / UltimateTerrain / COZY / etc. bulk removal) and Session 11 follow-on (Animancer). Two changes:
1. **`deletedAssets[]`-driven extraction:** the postprocessor now extracts deleted asmdef names directly from the `deletedAssets[]` array passed to `OnPostprocessAllAssets`, bypassing the `AssetDatabase.FindAssets("t:AssemblyDefinitionAsset")` rescan that could be stale during the postprocessor window.
2. **DLL-named-detection fix:** entries whose detection-type targets a DLL filename (HAS_DOTWEEN → DOTweenPro.dll, HAS_MAGICACLOTH2 → MagicaClothV2.dll, HAS_RAYFIRE → RayFireAssembly.dll, HAS_BROAUDIO → BroAudio.dll) were being spuriously flipped to "should remove" by `RemoveStaleDefines` because `presentAssemblies` (built from asmdef enumeration) never includes DLL names. Fix: also extract deleted `.dll` paths from `deletedAssets[]` and use that set to confirm DLL-based detections genuinely went away.

**Strategic precedent now validated twice (Cinemachine + AI Navigation):** the retire-on-publish rule is settled doctrine, not a one-off experiment. Future Ivan Extensions covering TMCP targets → retirement. The 8-step procedure in the memory template applies cleanly.

**Open follow-ups not done this session:**
- Other TMCP tool groups currently have no Ivan-Extension overlap (post-Session 17). Long-term watch list: AnimationRigging, ProBuilder (Ivan already has a partial extension; TMCP doesn't have a competing group), Particle System (Ivan already has it). No active conflicts.

---

### TecVooDoo Session 16 (June 4, 2026) -- Cinemachine tool group retired

Ivan-Murzak published an official MCP Extension for Cinemachine — `com.ivanmurzak.unity.mcp.cinemachine` 1.0.0 — alongside MCP 0.78.0 (2026-06-03 Sandbox / 2026-06-04 TVD). The official extension ships 14 tools (`cinemachine-brain-ensure`, `cinemachine-camera-create/get/list`, `cinemachine-modify`, `cinemachine-set-aim/-body/-default-blend/-lens/-noise/-priority/-targets`, `cinemachine-add-extension`, `cinemachine-get`) — broader and more granular than TMCP's 5-tool group (`cm-configure-brain`, `cm-configure-camera`, `cm-configure-follow`, `cm-configure-noise`, `cm-query`). Decision: **retire TMCP's Cinemachine group and defer to the upstream extension.**

**Strategic precedent set this session:** TMCP's value going forward is third-party Asset Store integrations Ivan won't cover (Master Audio, Behavior Designer, FinalIK, Animancer, etc.). Anything Ivan publishes as an official MCP Extension (Cinemachine today; possibly Animation Rigging, additional ProBuilder/Particle/Animation feature surfaces, etc. later) — TMCP retires its corresponding tool group and defers to upstream. Keeps TMCP focused on its unique value (asset-store-asset MCP coverage) and lowers maintenance surface as Ivan's catalog grows.

**Retirement footprint:**
- Deleted `Cinemachine/` folder + `Cinemachine.meta` (4 `.cs` + 4 `.cs.meta` + 1 `.asmdef` + 1 `.asmdef.meta` + 1 folder `.meta` + 1 root `.meta` = 11 files).
- Removed `("HAS_CINEMACHINE", "Unity.Cinemachine.CinemachineCamera, Unity.Cinemachine")` from [MCPToolsDefineManager.cs](../Editor/MCPToolsDefineManager.cs) `Entries[]`. (Was line 50.)
- Removed Cinemachine row from this doc's tool-group table; tool count adjusted from ~268/59 to ~263/58.
- Removed Cinemachine from the "Asmdef with direct refs" example list in § Package vs Source Sync.

**Stale-define cleanup note:** removing the `Entries[]` line does NOT auto-strip `HAS_CINEMACHINE` from any project's `PlayerSettings` — `UpdateDefines` only iterates the current `Entries[]`, so already-set defines for symbols no longer in the table persist as orphaned. This is harmless (no asmdef exists to gate on the define) but cosmetic cruft. Per-project cleanup is optional — run `Tools > TecVooDoo > Rescan MCP Defines` after pulling TMCP, or strip manually from `PlayerSettings > Scripting Define Symbols`. **TVD did NOT clean up the stale `HAS_CINEMACHINE` define this session — left as orphaned cosmetic.**

**Migration confirmed safe for all current fleet projects:** no project's workflows depend on `cm-*` tool names specifically. The official `cinemachine-*` tools are immediately usable. Both naming conventions coexist trivially because they don't collide — but the `cm-*` set is now gone.

**Open follow-ups not done this session:**
- Consider similar audits for other TMCP tool groups whenever Ivan publishes an official Extension. Currently no other overlaps.
- Long-term: when MCP publishes an `animation-rigging` or `animation-rigging-helpers` Extension (hypothetical), retire TMCP's AnimationRigging group similarly.

### TecVooDoo Session 11 (May 23, 2026) -- Stale `HAS_*` define recovery after MCP 0.75.1 bump + asmdef-less folder fragility finding

User upgraded `com.ivanmurzak.unity.mcp` 0.73.0 → 0.75.1 (sub-packages animation 1.2.3 / particlesystem 1.2.4 / probuilder 1.2.3) via Package Manager. As part of the standard MCP upgrade flow TMCP was removed and re-installed. Same session pruned several Phase-3 assets from the TecVooDoo project (confirmed by which `HAS_*` defines were stale afterward): **Koreographer, UltimateTerrain, COZY, Behavior Designer, Lumen, ORK, PressE, RTW, CityGen3D**, plus Cozy's package-set defines `COZY_3_AND_UP / COZY_URP / COZY_WEATHER / GRAPH_DESIGNER`.

**Project broke with `CS0246`** on `SonicBloom`, `PampelGames`, `DistantLands`, and `Opsive` — the four namespaces whose integration code was no longer guarded out at the asmdef level (because their `defineConstraints` were still satisfied by the stale defines).

**Why the auto-cleanup didn't run:** `MCPToolsAssetPostprocessor.OnPostprocessAllAssets`'s heuristic for detecting "relevant deletion" (`path.EndsWith(".asmdef"|".cs"|".dll")`) is unreliable for UPM-style removals — Package Manager surfaces deletions through paths that the heuristic missed. The `[InitializeOnLoad]` static ctor on `MCPToolsDefineManager` should have caught it on next domain reload, but the compile-error cascade prevented the `MCPTools.Editor` assembly itself from loading. **Note:** `MCPTools.Editor.asmdef` has no `defineConstraints` and no references, so on its face it should always compile — but Unity's broader compile-failure cascade still kept the assembly out of the loaded set. **Menu fallback unreachable** from this state because the menu item lives in the same assembly.

**Recovery procedure (worth keeping for future):** inline-mirror the `Entries[]` table + `FindType()` logic from [MCPToolsDefineManager.cs](../Editor/MCPToolsDefineManager.cs) inside an `mcp__ai-game-developer__script-execute` call. The Roslyn dynamic compile runs in a separate context that doesn't depend on the project's asmdef graph — `script-execute` can call `PlayerSettings.GetScriptingDefineSymbols / SetScriptingDefineSymbols` against `BuildTargetGroup.Standalone` even when MCPTools.Editor failed to load. Strip the stale defines in one pass; Unity domain-reloads automatically; the still-valid integration asmdefs compile clean. Verified this session — 14 MCPTools.* asmdefs loaded post-reload, console empty.

**Asmdef-less integration folders finding (the deeper diagnosis):** 11 TMCP integration folders ship without their own asmdef and leak into `Assembly-CSharp-Editor` — `BehaviorDesigner`, `Feel`, `DOTween`, `Terrain25D`, `BridgeBuilder25D`, `JuicyActions`, `BoingKit`, `MudBun`, `Lumen`, `uLipSync`, `Timeflow`. The tool-group table flags these with `None (#if only)` in the Asmdef column. Their `#if HAS_X` source guard is the *only* protection — `defineConstraints` cannot intervene because there is no asmdef. When `HAS_X` is stale (because of the postprocessor gap above), the `#if`-guarded code compiles inside `Assembly-CSharp-Editor` and the *entire project's Editor assembly* breaks, not just a tool group. The 33 integrations that DO have asmdefs degrade more gracefully: their asmdef simply doesn't compile when refs are missing, the rest of the project is unaffected.

**Suggested follow-up (next TMCP-touching session):** give each of the 11 folders its own asmdef matching the rest of TMCP. Pattern depends on whether the third-party API ships as asmdef (`references`-based — e.g. `MCPTools.Cozy.Editor.asmdef`) or DLL (`precompiledReferences`-based — e.g. `MCPTools.DryWetMIDI.Editor.asmdef` from Session 8). For BehaviorDesigner specifically, the existing pattern is `#if HAS_BEHAVIOR_DESIGNER` only — converting to `defineConstraints: ["HAS_BEHAVIOR_DESIGNER"]` on a new asmdef requires confirming the Opsive.BehaviorDesigner.Runtime asmdef name (or DLL filename if shipped that way) to add to `references` / `precompiledReferences`.

**No TMCP source changes shipped this session** — diagnostic + recovery only, no integration code or asmdef touched. The follow-up to add the 11 missing asmdefs is captured in the new "11 integration folders without asmdefs" Known Gotcha row.

**Known Gotchas table** gained two new rows for this class of failure: "Stale `HAS_*` defines after UPM-style asset removal" and "11 integration folders without asmdefs leak into Assembly-CSharp-Editor."

**Memory:** `feedback_tmcp_asmdef_rules.md` extended with the asmdef-less-folder list, the inline-mirror recovery diagnostic, and the "MCPTools.Editor can fail to load during cascade" gotcha.

### TecVooDoo Session 10 (May 15, 2026) -- UMotion Pro tool group

BloodMiner is about to use UMotion Pro v1.29p04 (Soxware Interactive) for character animation authoring. The asset was loaded into TecVooDoo this session; a fresh asset re-eval landed in [Sandbox_AssetLog.md](../../../Sandbox/Documents/Sandbox_AssetLog.md) as ENTRY-364 (re-eval of the pre-system summary-only ENTRY-091). Verdict for MCP candidacy was downgraded from "Deferred — script-execute adequate" to **Built** once BloodMiner adoption triggered the build.

**Asset shape:** Editor-only. Both shipped asmdefs (`UMotionSourceApplication`, `UMotionSourceEditor`) are `includePlatforms: ["Editor"]`. Main code lives in 2 obfuscated managed DLLs (`UMotionApplication.dll`, `UMotionEditor.dll`) + 2 native FBX SDK bundles. Zero user-facing runtime MonoBehaviour components — `component-add/get/modify` is N/A. Public API is the static `UMotionEditor.API.ClipEditor` (23 methods + 2 properties + 2 nested types) and `UMotionEditor.API.PoseEditor` (23 methods + 3 properties + 3 nested types). Documented at `Assets/UMotionEditor/Manual/UMotionAPI.html` and verified live via reflection — every advertised method resolves cleanly. Heavy DLL obfuscation (~756 types in `UMotionEditor.dll`), but the API namespace + `UMotionProjectFileV01_*` SO file-format types have stable names; everything else internal is unsafe to touch.

**4 tools built ([UMotionPro/Editor/](../UMotionPro/Editor/)):**

| Tool | Surface | Purpose |
|------|---------|---------|
| `umotion-query` | Reads ClipEditor + PoseEditor state (window-open flags, loaded project path, all clip names + selected clip, layer names with mute/weight, frame cursor + last keyframe, pose-editor assigned GameObject + pivot mode, optional bone hierarchy + mirror table) | Discovery — always call first to confirm `IsWindowOpened` + `IsProjectLoaded` before chaining. |
| `umotion-project` | Multi-op control: `open-windows`, `load`, `close`, `select-clip`, `rename-clip`, `delete-clip`, `set-frame`, `set-layer-blend`, `assign-pose-go`, `clear-pose-go` | Project + clip + cursor + layer-blend + pose-GO lifecycle. Single tool with `operation` param for compactness. |
| `umotion-import` | Wraps `ClipEditor.ImportClips(IEnumerable<AnimationClip>, ImportClipSettings)`. Accepts `clipPaths[]`, `fbxPaths[]` (pulls every AnimationClip sub-asset from each FBX), `clipNames[]` (AssetDatabase.FindAssets). All 6 `ImportClipSettings` flags are optional overrides on `ImportClipSettings.Default`. | Batch FBX → UMotion project clip import. Blocks until done. |
| `umotion-export` | `mode='current'` → `ExportCurrentClip()`. `mode='all'` → `ExportAllClips()`. `mode='variants'` → snapshots layer state, applies each `variantName:layerA=mute,layerB=unmute` preset, temporarily renames the clip to `<original>_<variantName>`, exports, restores everything. | Export to AnimationClip assets, with optional per-layer-mute variant generation in one call. Useful for "no-bow / with-bow" style animation set exports. |

**Architecture choices:**

- **Asmdef + DLL refs** rather than `#if HAS_*`-only. [MCPTools.UMotionPro.Editor.asmdef](../UMotionPro/Editor/MCPTools.UMotionPro.Editor.asmdef) explicitly references `"UMotionApplication.dll"` + `"UMotionEditor.dll"` in `precompiledReferences`, plus the source asmdefs `"UMotionSourceApplication"` + `"UMotionSourceEditor"` in `references`. Pattern matches DryWetMIDI (which has the same "third-party DLL filename in precompiledReferences" shape).
- **Direct API calls** with `using UMotionEditor.API;` — no reflection wrapper. The two public API classes are documented and stable; reflection would just add overhead and a future-rename hazard.
- **Window-state guards** in `Tool_UMotionPro.cs` root partial: `RequireClipEditorWindow()` throws with an actionable error pointing the caller at `umotion-project operation='open-windows'`. `RequireProjectLoaded()` chains that check + `ClipEditor.IsProjectLoaded`.
- **`umotion-export variants` preserves state.** Snapshots all layer mute/weight before the variant loop, restores in a `try/finally`. Also snapshots and restores the original clip name (the variants pattern temporarily renames the clip pre-export to control the exported AnimationClip's name).

**Define manager:** `("HAS_UMOTION_PRO", "UMotionEditor.API.ClipEditor, UMotionEditor")` added to [MCPToolsDefineManager.cs](../Editor/MCPToolsDefineManager.cs) (TecVooDoo Session 10 block). Confirmed firing — define present in PlayerSettings post-install.

**Gotcha hit this session — `MonoScript.GetClass()` returns null when files were added to a package source folder before the asmdef.** Symptom: file on disk with `.meta` and the right partial-class declaration, Unity recognizes it as a `MonoScript` via `assets-find`, but `CompilationPipeline.GetAssemblies(...).First(a.name=="MCPTools.UMotionPro.Editor").sourceFiles` only lists *some* of the files in the folder (4 of 5 in my case — `Tool_UMotionPro.Export.cs` was missing while `Tool_UMotionPro.Project.cs` was present). `AssetDatabase.Refresh(ForceUpdate)` + `CompilationPipeline.RequestScriptCompilation(CleanBuildCache)` did NOT recover it. Deleting the `.meta` and re-refreshing did NOT recover it either (Unity restored the meta from cache). **Fix that worked:** rename the file (e.g. `Tool_UMotionPro.Export.cs` → `Tool_UMotionPro.ExportV2.cs`). Filename change forced Unity to treat it as a brand-new asset and add it to the asmdef's source list. Took ~4.5 minutes for the rebuild to flush through (then McpPlugin's `SkillFileGenerator` re-runs for another ~30s — the "running backend" lag the user sees after the compile-progress bar finishes is the skill regeneration, not the compile itself). **General rule:** if a fresh tool file refuses to compile into its asmdef even though the file/.meta/class shape look right, rename it.

**File layout shipped:** `Tool_UMotionPro.cs` (root partial + state-guard helpers), `Tool_UMotionPro.Query.cs`, `Tool_UMotionPro.Project.cs`, `Tool_UMotionPro.Import.cs`, `Tool_UMotionPro.ExportV2.cs` (the renamed-and-stuck filename — leave alone until next session has time to rename it back through `AssetDatabase.MoveAsset`).

**SKILL.md YAML cap (1024 chars) warnings:** trimmed `umotion-project` (1409 → ~700 chars) and `umotion-import` (1045 → ~700 chars) descriptions in this session. Pre-existing over-cap descriptions on `cozy-configure-module` (1147 chars) and the upstream McpPlugin `unity-skill-create` (6678 chars) remain to be addressed.

**Smoke test:** verified `umotion-query` and `umotion-project operation='open-windows'` both return success. `umotion-query` after the open-windows call shows `PoseEditor.IsWindowOpened: True` — the API is alive.

### TecVooDoo Session 8 (May 10, 2026) -- DryWetMIDI + Koreographer asmdef refs

Hot-fix follow-on to Session 7. AudioProject reported two `CS0246` errors after reinstalling DryWetMIDI and Koreographer:

```
DryWetMIDI/Editor/Tool_DryWetMIDI.cs(8,7): error CS0246: ... 'Melanchall' could not be found ...
Koreographer/Editor/Tool_Koreographer.cs(7,7): error CS0246: ... 'SonicBloom' could not be found ...
```

**Root cause:** Both tool asmdefs had `overrideReferences: true` but `precompiledReferences` listed only the McpPlugin / ReflectorNet DLLs — never the third-party assemblies the tool source actually `using`-imports. So as soon as `HAS_DRYWETMIDI` / `HAS_KOREOGRAPHER` fired (i.e. the asset was installed) the asmdef started compiling and the namespace lookups failed.

A prior session had "fixed" this by deleting both `DryWetMIDI/` and `Koreographer/` folders entirely from the working tree (they were uncommitted deletes against HEAD), rather than adding the missing references.

**Fix shipped this session:**

1. Restored both folders via `git restore -- DryWetMIDI Koreographer DryWetMIDI.meta Koreographer.meta`.
2. Added `"Melanchall.DryWetMidi.dll"` to [MCPTools.DryWetMIDI.Editor.asmdef](../DryWetMIDI/Editor/MCPTools.DryWetMIDI.Editor.asmdef) `precompiledReferences`.
3. Added `"SonicBloom.Koreo.dll"` to [MCPTools.Koreographer.Editor.asmdef](../Koreographer/Editor/MCPTools.Koreographer.Editor.asmdef) `precompiledReferences`.
4. Verified clean compile in TecVooDoo with both assets installed; asmdef-sync correctly preserves third-party DLL refs (it only rewrites `McpPlugin*` / `ReflectorNet*` entries).

**General rule (added to `feedback_tmcp_asmdef_rules.md`):** When a tool .cs uses a third-party namespace, the asmdef MUST also reference the third-party assembly — either as an asmdef NAME in `references` (e.g. `"BroAudio"`, `"MidiPlayer.Run"`) or a DLL FILENAME in `precompiledReferences` (e.g. `"Melanchall.DryWetMidi.dll"`, `"SonicBloom.Koreo.dll"`). When CS0246 hits, **add the missing reference — never delete the tool folder.**

### TecVooDoo Session 7 (May 10, 2026) -- Self-syncing asmdef refs

Follow-on the same day. MCP 0.72.0 dropped mid-recovery and reverted to **unversioned** DLL filenames (`McpPlugin.dll`, `McpPlugin.Common.dll`, `ReflectorNet.dll`) flat at the top of `Assets/Plugins/NuGet/` — opposite of 0.71.0's versioned-filename convention. Blood Miner hit `CS0246` errors after upgrading because the asmdefs from Session 6 only listed the 0.71.0 versioned filenames.

User feedback: "MCP releases drop almost every other day. I don't plan to go backwards in version, but it does need to handle upgraded future MCP versions."

**Fix shipped this session:**

1. **Static dual-filename fallback** in all 46 tool-group asmdefs: `precompiledReferences` now lists both unversioned and 0.71.0 versioned filenames for `McpPlugin`, `McpPlugin.Common`, `ReflectorNet`. Unity links whichever file exists and ignores the missing one. Covers MCP 0.66.x / 0.69.x / 0.71.0 / 0.72.0 cleanly.

2. **`Editor/MCPToolsAsmdefSync.cs`** -- new `[InitializeOnLoad]` editor script. On every domain reload it:
   - Scans `Assets/Plugins/NuGet/` for `McpPlugin*.dll` / `McpPlugin.Common*.dll` / `ReflectorNet*.dll` filenames.
   - Rewrites each tool-group asmdef's `precompiledReferences` to match the discovered filenames (preserving any non-managed entries).
   - No-ops if already in sync (no infinite recompile loop).
   - Manual fallback: **Tools > TecVooDoo > Sync MCP DLL References**.

   This makes the asmdefs a function of the project's current NuGet folder rather than a hard-coded list. Future MCP version bumps that change DLL filenames self-heal on first compile, no TMCP source edit required.

**Trade-off accepted:** the auto-sync writes to TMCP's own asmdefs at runtime. Fine for the file-ref'd dev environment (and read-only registry installs would just no-op since the script can't write to read-only files — Unity logs a warning, project still compiles via the static fallback list).

### TecVooDoo Session 6 (May 10, 2026) -- MCP cross-version compatibility cleanup

Catch-up commit covering uncommitted prior-session work + today's MCP 0.71.0 fix. No new tool groups.

**Today's work (MCP 0.71.0 / NuGet 6.2.1 / 5.1.1):**
- All **46 tool-group asmdefs** updated to reference versioned NuGet DLL filenames (`McpPlugin.6.2.1.dll`, `McpPlugin.Common.6.2.1.dll`, `ReflectorNet.5.1.1.dll`) matching MCP 0.69.0+ flat-file convention. Pre-fix, only `UnityEntities` and `UnityPhysics` errored visibly because their `HAS_*` defines fire by default; the other 44 were silently broken behind unmet defines.
- Verified clean compile + ~140 tools registered in TecVooDoo (MCP 0.71.0).
- 8 PlayMode test failures observed in `MagicPigGames.JuicyActions.*` — third-party defects, not caused by Unity bump or this fix; details in `Documents/TVD_Status.md` Known Issues.

**May 4 catch-up (MCP_HAS_AIGD versionDefine for namespace move at MCP 0.69.0):**
- 6 asmdefs (FinalIK, Flexalon, MagicaCloth2, MalbersAC, PrefabWorldBuilder, RayFire) declare `versionDefines: { "name": "com.ivanmurzak.unity.mcp", "expression": "0.69.0", "define": "MCP_HAS_AIGD" }`.
- 23 .cs files wrap the `Data` namespace import in `#if MCP_HAS_AIGD using AIGD; #else using com.IvanMurzak.Unity.MCP.Runtime.Data; #endif` to keep both 0.66.x and 0.69.0+ projects compiling.

**Other housekeeping:**
- AssetInventory tool group **removed** (10 files deleted): no longer carried in TMCP.
- ORK + CityGen3D `.meta` files that were untracked from Session 5's tool-group additions are now committed.

**Cross-version status:** TMCP source now expects MCP 0.69.0+. Projects on MCP 0.66.1 must run the per-project upgrade recipe before reinstalling TMCP — see [Sandbox/Documents/MCP_ConnectionBrief.md](../../../Sandbox/Documents/MCP_ConnectionBrief.md).

**Open hardening item:** migrate the 46 asmdef `precompiledReferences` from filename-based to GUID-based, so future MCP version bumps don't require touching TMCP source.

### TecVooDoo Session 5 (Apr 27, 2026) -- 3 new tool groups (19 tools)

**New tool groups:**

- **Modular 3D Text (6 tools):** `HAS_M3DT`, `MCPTools.M3DText.Editor` (asmdef + reflection — TGS scripts compile into `Assembly-CSharp-firstpass` because they live under `Assets/Plugins/Tiny Giant Studio/`).
  - `m3dt-query` -- list scene-level Modular3DText components or full config snapshot for one (text, font, material, FontSize, WordSpacing, Capitalize/LowerCase/AutoLetterSize, modules, characterObjectList count, autoFontSize, etc.)
  - `m3dt-set-text` -- assign `Text` (triggers end-of-frame mesh rebuild). `forceUpdate` clears `oldText` for full character recreate.
  - `m3dt-configure` -- font / material / FontSize (Vector3 components) / WordSpacing / Capitalize / LowerCase / AutoLetterSize / autoFontSize + min/max / module flags / combineMesh flags / hideLetters flags
  - `m3dt-add-module` -- list / list-attached / add / clear; adds a Module asset to the addingModules or deletingModules list (constructs a ModuleContainer and calls UpdateVariableHolders())
  - `m3dt-find-fonts` -- enumerate `TinyGiantStudio.Text.Font` assets in project (with optional name filter)
  - `m3dt-create-control` -- create new GameObject with Modular3DText (and optionally Button/Slider/InputField/Toggle/HorizontalSelector/List), attach under parent, set initial text + font + material

**Detection entry added:** `HAS_M3DT` -> `TinyGiantStudio.Text.Modular3DText, Assembly-CSharp-firstpass`.

- **ORK Framework + Makinom (7 tools):** `HAS_ORK`, `MCPTools.ORK.Editor` (asmdef + reflection — ORK ships as DLLs under `Assets/Gaming Is Love/Makinom 2/DLL/` with `.pdb` symbols).
  - `ork-database-query` -- list combatants / items / abilities / classes / quests / equipment / status from project DB with filter + limit
  - `ork-query-combatant` -- list active group, or report deep status for a named combatant (level, class, all StatusValue base/current/max, inventory + equipment count). Play mode required.
  - `ork-modify-combatant` -- set/add a StatusValue (HP/MP/etc.), set Level, heal-to-max, revive, fire-changed
  - `ork-inventory` -- list / add / remove / count items in a combatant or group inventory
  - `ork-quest` -- list / add / remove / has / status on quests via `QuestHandler.AddQuest/RemoveQuest/GetQuest`
  - `ork-battle` -- query / end / flee the current battle (Battle handler)
  - `ork-schematic-run` -- list / run / stop a `MakinomSchematicAsset` via `Maki.MachineHandler`

**Detection entry added:** `HAS_ORK` -> `GamingIsLove.ORKFramework.ORK, ORKFramework3`.

- **CityGen3D (6 tools):** `HAS_CITYGEN3D`, `MCPTools.CityGen3D.Editor` (asmdef + reflection — CityGen3D ships as `CityGen3D.dll` + `CityGen3D.EditorExtension.dll` with `.pdb` symbols under `Assets/CityGen3D/Plugins/`).
  - `cg-query-map` -- `Map.Instance` snapshot: roads / buildings / features / surfaces / trees / entities counts, origin coord, scene Generator presence
  - `cg-find-road-at` -- `mapRoads.GetMapRoadAtWorldPosition(x,z,radius)` (mode='at') or `mapRoads.GetNearestRoad(Vector2, ref Vector3)` (mode='nearest')
  - `cg-find-feature-at` -- enumerate buildings / surfaces / features / entities / trees within radius via best-effort position introspection
  - `cg-add-blueprint` -- list project Blueprint prefabs or attach a named Blueprint to the active Generator's `blueprints` list
  - `cg-generator-configure` -- list every public field on the Generator with current value, or apply `field=value` assignments (bool/int/float/string/enum)
  - `cg-generate` -- trigger Generate / Clear / Cancel on the active Generator (best-effort method-name match)

**Detection entry added:** `HAS_CITYGEN3D` -> `CityGen3D.Map, CityGen3D`.

**Tool count:** 55 -> 58 groups, ~245 -> ~264 tools.

**Notes for ORK + CityGen3D builds:**
- Both groups are reflection-heavy because their assets ship as DLLs. Tools degrade to clear error messages when method/field signatures differ rather than failing to compile.
- The Unity APIUpdater raised `Error = 131` on Makinom and CityGen3D DLLs during initial import; once that resolved the asset types loaded correctly. If recurrence: right-click each affected DLL → enable "Override the API Updater" → reimport.
- **ORK tools require Play mode.** ORK reads from `ORK.Instance.*` (combatants/items/quests/etc.), which is null until `ORK.Initialize(projectAsset)` runs. The TMCP tools call `RequireORKInitialized()` and return a clear message out of Play mode (instead of throwing a NullReferenceException). Open a scene with an `ORK Game Starter` component and enter Play mode before invoking ORK tools.
- **CityGen3D tools work in edit mode.** `Map.Instance` returns null when no map is loaded; `cg-query-map` reports that gracefully. Use `cg-generate` to populate the map (after a Generator is in scene + configured).

---

### TecVooDoo Session 4 (Apr 25, 2026) -- 1 new tool group (5 tools)

**New tool group:**

- **COZY 3 Stylized Weather (5 tools):** `HAS_COZY`, `MCPTools.Cozy.Editor` (asmdef + direct refs to `DistantLands.Cozy.Runtime`).
  - `cozy-query` -- runtime snapshot of CozyWeather sphere: cloud/sky/fog style, time, weather profile, attached modules, climate/wind values, biome/system count
  - `cozy-set-weather` -- swap the active WeatherProfile via `CozyEcosystem.SetWeather(prof, transitionTime)`. In edit mode falls back to direct assignment + `RaiseOnWeatherChange`. Includes `listProfiles` mode to enumerate available profiles.
  - `cozy-set-time` -- set hour/minute, dayPercentage (0..1), or relative skip (`SkipTime`). Smooth play-mode transition via `TransitionTime`. Also sets day/year and toggles `FreezeUpdateInEditMode`.
  - `cozy-configure-module` -- list / query / add / remove / reset / enable / disable / set fields on any `CozyModule` subclass (Climate, Wind, Time, Atmosphere, Ambience, Weather, Reflections, Satellite, Interactions, Event, SaveLoad, Debug, Microsplat, PureNature, TVE, Buto, Transit, SystemTime). Field assignment supports bool/int/float/string/enum/Color/Vector2/Vector3.
  - `cozy-set-biome` -- list / set / isolate / reset CozyBiome instances. Mode (Global/Local), TransitionMode (Distance/Time), maxWeight, transitionDistance/Time, trigger collider. `isolate` zeroes other biomes for testing one in isolation.

**Detection entry added:** `HAS_COZY` -> `DistantLands.Cozy.CozyWeather, DistantLands.Cozy.Runtime`.

**Project setup applied (TecVooDoo this session):**
- Added `com.unity.modules.wind: 1.0.0` to `Packages/manifest.json` -- resolves CozyWindModule's `UnityEngine.WindZone` reference (Unity 2022.3+ made Wind an optional module).
- Added `COZY_URP` to Standalone scripting defines so Cozy's pipeline-gated `#if COZY_URP || COZY_HDRP` paths take the URP branch (matches the project's URP install).

These two fixes mirror the Sandbox setup documented in ENTRY-337 of `Sandbox_AssetLog.md`.

**Tool count:** 54 -> 55 groups, ~240 -> ~245 tools.

---

### TecVooDoo Session 3 (Apr 21, 2026) -- 5 new tool groups (21 tools)

**New tool groups:**

- **Technie Collider Creator 2 (6 tools):** `HAS_TCC`, `MCPTools.TCC.Editor` (asmdef + direct refs to `TechniePhysicsCreator`/`TechniePhysicsCreatorEditor`).
  - `tcc-create` -- attach RigidColliderCreator + generate PaintingData/HullData assets from MeshFilter
  - `tcc-add-hull` -- add hull (Box/ConvexHull/Sphere/Face/FaceAsBox/Auto/Capsule), assign material, optional PaintAllFaces
  - `tcc-generate` -- trigger GenerateColliders coroutine (VHACD for Auto hulls)
  - `tcc-configure-vhacd` -- set autoHullPreset (Low/Medium/High/Placebo/Custom) + 12 VhacdParameters
  - `tcc-bulk` -- SetAllTypes/Materials/AsChild/AsTrigger + maxPlanes
  - `tcc-query` -- list hulls, generated children, VHACD config, IsGeneratingColliders status
  - `tcc-delete-generated` -- DeleteGenerated() while preserving PaintingData

- **MK Edge Detection (4 tools):** `HAS_MK_EDGE`, `MCPTools.MKEdge.Editor` (asmdef + direct refs to MK URP volume + renderer feature assemblies).
  - `mkedge-query` -- read all 36 parameters + variant detection (Volume vs RendererFeature)
  - `mkedge-configure` -- set any subset of 36 parameters; auto-detects target flavor; handles RangeProperty/MinMaxRangeProperty/ColorProperty/EnumProperty/BitmaskProperty wrappers
  - `mkedge-preset` -- apply named presets (subtle-outline, comic, blueprint, sketch, ink-wash, souls-like, toon, noir)
  - `mkedge-toggle` -- enable/disable Volume.active or RendererFeature.SetActive

- **Real Time Weather Pro (3 tools):** `HAS_RTW`, `MCPTools.RTW.Editor` (asmdef + reflection — RTW is in Assembly-CSharp).
  - `rtw-query` -- location, active weather/water systems, request modes, auto-update settings
  - `rtw-configure` -- set lat/lon, weather/water systems, request modes, ActivateXSimulation/DeactivateXSimulation/DeactivateAllWeather/DeactivateAllWater
  - `rtw-request` -- RequestWeatherByCityAndCountry / ByCityAndState / ByGeoCoordinates (coroutine kicked via StartCoroutine)

- **Ultimate Terrain (3 tools):** `HAS_ULTIMATE_TERRAIN`, `MCPTools.UltimateTerrain.Editor` (asmdef + direct refs to `PG.UltimateTerrain`).
  - `ut-query` -- list active instances or single, status, position/scale, module/layer counts, IsExecuting/IsPaused
  - `ut-configure` -- set position, scale, duration, enableAnimation, delaySync, multiTerrainActive
  - `ut-execute` -- trigger Execute/ExecuteInstant/ExecuteHeight/ExecuteTextures + Pause/Resume/Stop + Bake + Reset[Terrain|Height|Texture|Details|Trees|Prefabs]

- **PressE PRO 2 (4 tools):** `HAS_PRESSE`, `MCPTools.PressE.Editor` (asmdef + reflection — PressE in Assembly-CSharp).
  - `pe-query` -- list Interactable + Key components, InteractionManager singleton state
  - `pe-configure-interactable` -- set interactMode, HasSensor, SensorRadius, UseConditions, OverrideInteractionKey, CanInteract, GrabId, maxInteractions
  - `pe-configure-key` -- set KeyName, AddKeyWhenInteract, RemoveKeyWhenUsed, DisableObjectWhenInteract
  - `pe-trigger` -- programmatically call Interactable.Interact() (mostly play mode)

**Detection entries added (5):** `HAS_TCC`, `HAS_MK_EDGE`, `HAS_ULTIMATE_TERRAIN`, `HAS_PRESSE`, `HAS_RTW`.

**Tool count:** 49 -> 54 groups, ~219 -> ~240 tools.

**Pattern split this session:**
- Direct refs (3 groups): TCC, MK Edge, Ultimate Terrain — assets ship asmdefs.
- Reflection (2 groups): RTW, PressE — assets in Assembly-CSharp.

**Bugfix (AQS verification, same day):** `HAS_TCC` was never firing when TCC was installed, so `MCPTools.TCC.Editor.asmdef` never compiled and no `tcc-*` tools registered. Detect-type string in `MCPToolsDefineManager.cs` pointed at `Technie.PhysicsCreator.Rigid.RigidColliderCreator` (based on folder layout), but the class is declared in namespace `Technie.PhysicsCreator` — the `Rigid` sub-namespace exists (e.g. `Hull`, `HullType`) but `RigidColliderCreator` itself is not in it. Fixed by changing the entry to `Technie.PhysicsCreator.RigidColliderCreator, TechniePhysicsCreator`. Lesson: folder structure is not namespace structure — verify the actual `namespace` declaration on the detect-type class when adding a new `HAS_*` entry.

---

### TecVooDoo Session 1 (Apr 9, 2026) -- 4 new tool groups + infrastructure fixes

**New tool groups (14 tools):**
- **Decal Collider (3 tools):** `HAS_DECAL_COLLIDER`, `MCPTools.DecalCollider.Editor` (asmdef + reflection). `decal-query` (full config + rebuild stats + hit objects), `decal-configure` (mode, projection, size, subdivisions, LOD), `decal-rebuild` (trigger rebuild + set sprite/text/color/lookAt + save mesh).
- **Texture Studio (3 tools):** `HAS_TEXTURE_STUDIO`, `MCPTools.TextureStudio.Editor` (asmdef + reflection). `texstudio-query` (layers, hierarchy, blend modes, materials, states), `texstudio-set-param` (SetParam overloads + transform + sprite + state management), `texstudio-render` (UpdateTexture + ApplyMap + bake PNG).
- **AI Navigation (4 tools):** `HAS_AI_NAVIGATION`, `MCPTools.AINavigation.Editor` (asmdef + direct refs). `nav-query` (surfaces, links, modifiers, volumes, agents), `nav-configure-surface` (collect, geometry, layers, tiles, voxels), `nav-bake` (sync BuildNavMesh, single or all), `nav-configure-link` (start/end, width, cost, bidirectional, transforms).
- **Animancer Pro (4 tools):** `HAS_ANIMANCER`, `MCPTools.Animancer.Editor` (asmdef + direct refs). `animancer-query` (graph state, layers, current/active states, registered states), `animancer-play` (play/crossfade by clip name, layer, speed, startTime), `animancer-stop` (specific clip or all), `animancer-configure` (speed, weight, time, normalizedTime, isPlaying on registered state).

**Infrastructure:**
- **DefineManager fix:** `RemoveStaleDefines()` now uses `AssetDatabase.FindAssets("ClassName t:MonoScript")` for Assembly-CSharp entries. Previously 8 entries (PWB, FinalIK, QuestForge, RopeToolkit, MasterAudio, DialogueSystem, PMG, Chunity) could never be auto-stripped. Added **Tools > TecVooDoo > Rescan MCP Defines** menu item.
- **Package dependency:** Added `com.ivanmurzak.unity.mcp` as dependency in TMCP package.json.
- **8 Assembly-CSharp groups migrated to asmdef + reflection:** MasterAudio, DialogueSystem, RopeToolkit, FinalIK, PWB, QuestForge, PMG, Chunity. These now use `defineConstraints` instead of `#if` guards, with 100% reflection for asset type access. More robust on asset removal.
- **8 own-assembly groups keep `#if` pattern:** Feel, DOTween, BehaviorDesigner, Terrain25D, BridgeBuilder25D, JuicyActions, BoingKit, MudBun. These retain reflection-based API calls with `#if` guards. Asmdefs were tested but `defineConstraints` doesn't prevent missing assembly reference errors for uninstalled assets.
- **`MainThread` using clarification:** `MainThread.Instance.Run()` is in `com.IvanMurzak.ReflectorNet.Utils` (ReflectorNet.dll), NOT in `com.IvanMurzak.Unity.MCP.Editor.Utils`.

**Tool count:** 39 -> 43 groups, ~184 -> ~200 tools.

---

### TecVooDoo Session 2 (Apr 9, 2026) -- 3 new tool groups (Juicy Actions, Boing Kit, MudBun)

**New tool groups (7 tools):**
- **Juicy Actions (2 tools):** `HAS_JUICY_ACTIONS`, `#if` guard + reflection. `juicy-query` (list all ActionOnEvent-derived triggers on GO, report ActionExecutor items/timeMode/cooldown), `juicy-play` (trigger PlayActions() on a specific trigger by index, play mode only). Detection: `MagicPigGames.JuicyActions.ActionExecutor, MagicPigGames.JuicyActions.Runtime`.
- **Boing Kit (2 tools):** `HAS_BOINGKIT` (detection already existed from TVD1), `#if` guard + reflection. `boing-query` (report BoingEffector/BoingBehavior/BoingBones/BoingReactorField config on GO), `boing-configure` (set fields by componentType: Effector radius/impulse, Behavior effects/locks, ReactorField cells/falloff/propagation).
- **MudBun (3 tools):** `HAS_MUDBUN`, `#if` guard + reflection. `mudbun-query` (renderer settings + child brush hierarchy with type/operator/blend/material), `mudbun-configure-renderer` (RenderMode, MeshingMode, VoxelDensity, MasterColor/Metallic/Smoothness, SplatSize, shadows), `mudbun-configure-brush` (Operator, Blend, Radius, Round, Color/Emission/Metallic/Smoothness per brush). Detection: `MudBun.MudRenderer, MudBun`.

**Infrastructure:**
- Added `HAS_JUICY_ACTIONS` and `HAS_MUDBUN` to MCPToolsDefineManager.cs.

**Lumen (2 tools):** `HAS_LUMEN`, `#if` guard + reflection. `lumen-query` (report LumenEffectPlayer config: scale/brightness/color/range/profile/layers/update frequency/fading/init-deinit behaviors), `lumen-configure` (set all player properties + runtime fade methods: FadeBrightness/FadeScale/FadeColor). Detection: `DistantLands.Lumen.LumenEffectPlayer, DistantLands.Lumen.Runtime`. UPM package at `Packages/com.distantlands.lumen`.

**Timeflow (4 tools):** `HAS_TIMEFLOW`, `#if` guard + reflection. `timeflow-query` (timeline state, objects, behaviors), `timeflow-control` (Play/Stop/Pause/SetTime/Reverse with timeScale/loop), `timeflow-configure-tween` (interpolation, repeat, min/max vectors, amount, pingpong, triggers), `timeflow-configure-event` (trigger time, SendMessage target, function, parameter, limits). Detection: `AxonGenesis.Timeflow, Timeflow`. ENTRY-100 re-eval: Conditional -> Recommended.

**Tool count:** 43 -> 49 groups, ~200 -> ~219 tools.

---

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
