# TecVooDoo MCP Tools -- API Reference

**Package:** `com.tecvoodoo.mcp-tools` v1.5.0
**Source:** `E:\Unity\DefaultUnityPackages\com.tecvoodoo.mcp-tools\`
**Last Updated:** March 23, 2026

---

## Architecture

All tools follow the same pattern:
- `[McpPluginToolType]` on partial class per group
- `[McpPluginTool]` on each tool method
- `MainThread.Instance.Run()` for main-thread execution
- `GameObjectRef` for scene object lookup
- `Undo.RegisterCreatedObjectUndo()` on all creations
- `EditorUtility.SetDirty()` on all modifications
- Strongly-typed response objects with `[Description]` attributes

### Auto-Detection (MCPToolsDefineManager.cs)

| Define | Detection Target | Assembly |
|--------|-----------------|----------|
| `HAS_FLEXALON` | `Flexalon.FlexalonGridLayout` | `Flexalon` |
| `HAS_PWB` | `PluginMaster.PaletteManager` | `Assembly-CSharp-Editor` |
| `HAS_RAYFIRE` | `RayFire.RayfireRigid` | `RayFireAssembly` |
| `HAS_MAGICACLOTH2` | `MagicaCloth2.MagicaCloth` | `MagicaClothV2` |
| `HAS_FINALIK` | `RootMotion.FinalIK.FullBodyBipedIK` | `Assembly-CSharp-firstpass` |
| `HAS_ANIMANCER` | `Animancer.AnimancerComponent` | `Kybernetik.Animancer` |
| `HAS_PLAYMAKER` | `HutongGames.PlayMaker.PlayMakerFSM` | `PlayMaker` |
| `HAS_ASSETINVENTORY` | `AssetInventory.AssetSearch` | `AssetInventory.Editor` |
| `HAS_MALBERS_AC` | `MalbersAnimations.Controller.MAnimal` | `MalbersAnimations` |
| `HAS_MALBERS_QUESTFORGE` | `MalbersAnimations.QuestForge.QuestManager` | `Assembly-CSharp` |
| `HAS_RETARGETPRO` | `KINEMATION.RetargetProComponent` | `RetargetPro.Runtime` |
| `HAS_BROAUDIO` | `Ami.BroAudio.BroAudio` | `BroAudio` |
| `HAS_KOREOGRAPHER` | `SonicBloom.Koreo.Koreographer` | `SonicBloom.Koreo` |
| `HAS_PMG` | `ProcGenMusic.MusicGenerator` | `Assembly-CSharp` |
| `HAS_MAESTRO` | `MidiPlayerTK.MidiFilePlayer` | `MidiPlayer.Run` |
| `HAS_DRYWETMIDI` | `Melanchall.DryWetMidi.Core.MidiFile` | `Melanchall.DryWetMidi` |
| `HAS_FMOD` | `FMODUnity.RuntimeManager` | `FMODUnity` |
| `HAS_CHUNITY` | `ChuckMainInstance` | `Assembly-CSharp` |
| `HAS_NANINOVEL` | `Naninovel.Engine` | `Elringus.Naninovel.Runtime` |
| `HAS_ADVENTURE_CREATOR` | `AC.KickStarter` | `AC` |
| `HAS_TEXT_ANIMATOR` | `Febucci.TextAnimatorForUnity.TextAnimatorComponentBase` | `Febucci.TextAnimatorForUnity.Runtime` |
| `HAS_INK` | `Ink.Runtime.Story` | `Ink-Libraries` |

---

## 1. Flexalon (7 Tools)

**Files:** `MCPTools/Flexalon/Editor/Tool_Flexalon.*.cs`
**Asmdef:** `MCPTools.Flexalon.Editor` | **Define:** `HAS_FLEXALON`

### flexalon-create-flexible-layout

Creates a FlexalonFlexibleLayout (linear flexbox-like layout).

```csharp
FlexibleLayoutResponse CreateFlexibleLayout(
    string name = "FlexalonFlex",
    string direction = "PositiveX",      // PositiveX/NegativeX/PositiveY/NegativeY/PositiveZ/NegativeZ
    float gap = 0.1f,
    bool wrap = false,
    string wrapDirection = "NegativeY",
    float wrapGap = 0.1f,
    string horizontalAlign = "Center",   // Start/Center/End
    string verticalAlign = "Center",
    Vector3? position = null,
    GameObjectRef? parentGameObjectRef = null
)
```

**Returns:** gameObjectName, instanceId, direction, gap, wrap, position

### flexalon-create-grid-layout

Creates a FlexalonGridLayout (NxM grid, optional layers).

```csharp
GridLayoutResponse CreateGridLayout(
    string name = "FlexalonGrid",
    int columns = 3,
    int rows = 3,
    int layers = 1,
    float columnSize = 1f,
    float rowSize = 1f,
    float columnSpacing = 0.1f,
    float rowSpacing = 0.1f,
    Vector3? position = null,
    GameObjectRef? parentGameObjectRef = null
)
```

**Returns:** gameObjectName, instanceId, columns, rows, layers, position

### flexalon-create-circle-layout

Creates a FlexalonCircleLayout (circle or spiral).

```csharp
CircleLayoutResponse CreateCircleLayout(
    string name = "FlexalonCircle",
    float radius = 3f,
    string plane = "XZ",                 // XZ/XY/YZ
    string spacingType = "Evenly",       // Evenly/Degrees
    float spacingDegrees = 30f,
    float startAtDegrees = 0f,
    string rotate = "None",              // None/In/Out
    bool spiral = false,
    float spiralSpacing = 0.5f,
    Vector3? position = null,
    GameObjectRef? parentGameObjectRef = null
)
```

**Returns:** gameObjectName, instanceId, radius, plane, position

### flexalon-create-random-layout

Creates a FlexalonRandomLayout (scatter within bounds).

```csharp
RandomLayoutResponse CreateRandomLayout(
    string name = "FlexalonRandom",
    int randomSeed = 1,
    bool randomizeRotation = false,
    Vector3? size = null,
    Vector3? position = null,
    GameObjectRef? parentGameObjectRef = null
)
```

**Returns:** gameObjectName, instanceId, randomSeed, position
**Gotcha:** `FlexalonLayoutBase` `[RequireComponent]` auto-adds `FlexalonObject`. Use `GetComponent<FlexalonObject>()`, never `AddComponent`.

### flexalon-add-child

Adds a child to any Flexalon layout.

```csharp
AddChildResponse AddChild(
    GameObjectRef layoutRef,
    string childType = "cube",           // cube/sphere/cylinder/capsule/quad/empty/existing/prefab
    string? childName = null,
    GameObjectRef? existingChildRef = null,
    string? prefabPath = null,
    Vector3? scale = null
)
```

**Returns:** gameObjectName, instanceId, childType, layoutName, childCount

### flexalon-add-prefab-children

Batch adds multiple prefab instances to a layout.

```csharp
AddMultipleResponse AddPrefabChildren(
    GameObjectRef layoutRef,
    string prefabPath,
    int count = 1,
    Vector3? scale = null
)
```

**Returns:** layoutName, prefabName, addedCount, totalChildCount

### flexalon-set-object-size

Sets FlexalonObject size (controls layout measurement).

```csharp
SetSizeResponse SetObjectSize(
    GameObjectRef targetRef,
    float? width = null,
    float? height = null,
    float? depth = null
)
```

**Returns:** gameObjectName, instanceId, width, height, depth

### flexalon-list-layouts

Lists all Flexalon layouts in scene.

```csharp
ListLayoutsResponse ListLayouts()
```

**Returns:** count, layouts (type, childCount per layout)

---

## 2. Prefab World Builder (4 Tools)

**Files:** `MCPTools/PrefabWorldBuilder/Editor/Tool_PWB.*.cs`
**No asmdef** -- uses `#if HAS_PWB` guards | **Define:** `HAS_PWB`

### pwb-list-palettes

Lists all PWB palettes and brushes.

```csharp
ListPalettesResponse ListPalettes(
    bool includeBrushes = true
)
```

**Returns:** paletteCount, details (palette names, brush names, prefab refs)

### pwb-place-brush

Places a prefab from a PWB palette brush at a position.

```csharp
PlaceBrushResponse PlaceBrush(
    Vector3 position,
    int paletteIndex = 0,
    int brushIndex = 0,
    string? brushName = null,            // name search across all palettes
    Vector3? rotation = null,
    Vector3? scale = null,
    int itemIndex = 0,
    GameObjectRef? parentGameObjectRef = null
)
```

**Returns:** gameObjectName, instanceId, prefabName, brushName, paletteName, position

### pwb-place-line

Places multiple prefab instances in a line.

```csharp
PlaceLineResponse PlaceLine(
    Vector3 startPosition,
    Vector3 endPosition,
    int count = 5,
    int paletteIndex = 0,
    int brushIndex = 0,
    string? brushName = null,
    bool alignToLine = false,
    Vector3? scale = null,
    GameObjectRef? parentGameObjectRef = null
)
```

**Returns:** containerName, containerInstanceId, brushName, placedCount, startPosition, endPosition

### pwb-add-to-palette

Adds a prefab to a PWB palette as a new brush.

```csharp
AddToPaletteResponse AddToPalette(
    string prefabPath,
    int paletteIndex = 0
)
```

**Returns:** prefabName, paletteName, brushName, totalBrushes

---

## 3. RayFire (8 Tools)

**Files:** `MCPTools/RayFire/Editor/Tool_RayFire.*.cs`
**Asmdef:** `MCPTools.RayFire.Editor` | **Define:** `HAS_RAYFIRE`

**CRITICAL GOTCHA:** `rayfire-demolish` and `rayfire-apply-damage` crash Unity when called from MCP context. Use these tools for setup only, not runtime testing.

### rayfire-add-rigid

Adds RayfireRigid (makes object destructible).

```csharp
AddRigidResponse AddRigid(
    GameObjectRef targetRef,
    string simulationType = "Dynamic",    // Dynamic/Sleeping/Inactive/Kinematic
    string demolitionType = "Runtime",    // Runtime/None/AwakePrecache/AwakePrefragment
    string objectType = "Mesh",           // Mesh/ConnectedCluster/NestedCluster/MeshRoot
    string physicsMaterial = "Concrete",  // Concrete/Wood/Metal/Glass/Ice/Rubber/Rock
    float mass = 1f,
    bool enableDamage = true,
    float maxDamage = 100f,
    bool collisionDamage = true,
    bool useGravity = true,
    int maxDepth = 1,
    string fadeType = "None",             // None/SimExceed/LifeTime
    float fadeLifetime = 5f,
    bool initialize = true
)
```

**Returns:** gameObjectName, instanceId, simulationType, demolitionType, physicsMaterial, damageEnabled, maxDamage

### rayfire-configure-rigid

Modifies existing RayfireRigid. Null params leave unchanged.

```csharp
ConfigureRigidResponse ConfigureRigid(
    GameObjectRef targetRef,
    string? simulationType = null,
    string? demolitionType = null,
    string? physicsMaterial = null,
    float? mass = null,
    bool? enableDamage = null,
    float? maxDamage = null,
    bool? useGravity = null,
    int? maxDepth = null,
    string? fadeType = null,
    float? fadeLifetime = null
)
```

**Returns:** gameObjectName, instanceId, simulationType, demolitionType, physicsMaterial, mass, damageEnabled, maxDamage

### rayfire-add-shatter

Adds RayfireShatter for pre-fragmentation.

```csharp
AddShatterResponse AddShatter(
    GameObjectRef targetRef,
    string fragmentType = "Voronoi",     // Voronoi/Splinters/Slabs/Bricks/Radial/Hexagon/Voxels/Slices/Decompose
    int fragmentCount = 10,
    int brickColumns = 3,
    int brickRows = 3,
    int radialRings = 3,
    int radialRays = 6,
    bool colorPreview = false
)
```

**Returns:** gameObjectName, instanceId, fragmentType, configured

### rayfire-fragment

Executes fragmentation on a RayfireShatter.

```csharp
FragmentResponse Fragment(
    GameObjectRef targetRef
)
```

**Returns:** gameObjectName, instanceId, fragmentType, batchCount, hasFragments

### rayfire-list-rigid

Lists all RayfireRigid components in scene.

```csharp
ListRigidResponse ListRigid()
```

**Returns:** count, details (simulation type, demolition type, material, damage per object)

### rayfire-demolish

Forces immediate demolition. **CRASHES UNITY FROM MCP -- setup only.**

```csharp
DemolishResponse Demolish(
    GameObjectRef targetRef
)
```

**Returns:** gameObjectName, instanceId, fragmentCount, demolished

### rayfire-apply-damage

Applies damage, demolishes if over max. **CRASHES UNITY FROM MCP -- setup only.**

```csharp
ApplyDamageResponse ApplyDamage(
    GameObjectRef targetRef,
    float damageAmount,
    Vector3? damagePoint = null,
    float damageRadius = 0f
)
```

**Returns:** gameObjectName, instanceId, damageApplied, currentDamage, maxDamage, demolished

---

## 4. MagicaCloth 2 (7 Tools)

**Files:** `MCPTools/MagicaCloth2/Editor/Tool_MagicaCloth.*.cs`
**Asmdef:** `MCPTools.MagicaCloth2.Editor` | **Define:** `HAS_MAGICACLOTH2`

### magica-add-bone-cloth

Adds BoneCloth (simulates via bone transforms). Ideal for capes, hair, tails, skirts.

```csharp
AddClothResponse AddBoneCloth(
    GameObjectRef targetRef,
    string[] rootBoneNames,
    float gravity = 5f,
    float damping = 0.05f,
    float radius = 0.02f,
    float blendWeight = 1f
)
```

**Returns:** gameObjectName, instanceId, clothType, rootBonesFound, gravity

### magica-add-mesh-cloth

Adds MeshCloth (simulates via mesh vertices). Ideal for flags, curtains, tablecloths.

```csharp
AddClothResponse AddMeshCloth(
    GameObjectRef targetRef,
    float gravity = 5f,
    float damping = 0.05f,
    float radius = 0.02f,
    float blendWeight = 1f
)
```

**Returns:** gameObjectName, instanceId, clothType, rootBonesFound, gravity
**Gotcha:** MeshCloth locks all verts. Must use SkinnedMeshRenderer, not MeshRenderer.

### magica-list-cloth

Lists all MagicaCloth components in scene.

```csharp
ListClothResponse ListCloth()
```

**Returns:** count, details (type, gravity, blendWeight per component)

### magica-add-sphere-collider

Adds MagicaSphereCollider (prevents cloth penetration on body parts).

```csharp
AddColliderResponse AddSphereCollider(
    GameObjectRef targetRef,
    float radius = 0.1f,
    Vector3? center = null
)
```

**Returns:** gameObjectName, instanceId, colliderType, size

### magica-add-capsule-collider

Adds MagicaCapsuleCollider (best for limbs).

```csharp
AddColliderResponse AddCapsuleCollider(
    GameObjectRef targetRef,
    float startRadius = 0.05f,
    float endRadius = 0.05f,
    float length = 0.2f,
    string direction = "X",              // X/Y/Z
    Vector3? center = null
)
```

**Returns:** gameObjectName, instanceId, colliderType, size

### magica-add-wind

Creates MagicaWindZone affecting all cloth in scene.

```csharp
AddWindResponse AddWind(
    string name = "MagicaWind",
    string mode = "GlobalDirection",     // GlobalDirection/SphereDirection/BoxDirection/SphereRadial
    float strength = 5f,
    float turbulence = 1f,
    float directionAngleX = 0f,
    float directionAngleY = 0f,
    float radius = 10f,
    Vector3? boxSize = null,
    Vector3? position = null,
    GameObjectRef? parentGameObjectRef = null
)
```

**Returns:** gameObjectName, instanceId, windMode, strength, turbulence

---

## 5. Final IK (5 Tools)

**Files:** `MCPTools/FinalIK/Editor/Tool_FinalIK.*.cs`
**No asmdef** -- uses `#if HAS_FINALIK` guards | **Define:** `HAS_FINALIK`

### finalik-add-fbbik

Adds FullBodyBipedIK with auto-detection of biped bones.

```csharp
AddFBBIKResponse AddFullBodyBipedIK(
    GameObjectRef targetRef,
    bool autoDetect = true
)
```

**Returns:** gameObjectName, instanceId, referencesDetected, hasError, errorMessage, rootNode

### finalik-set-effector

Sets FBBIK effector target position/weight.

```csharp
SetEffectorResponse SetEffector(
    GameObjectRef targetRef,
    string effectorName,                 // Body/LeftHand/RightHand/LeftFoot/RightFoot/LeftShoulder/RightShoulder
    Vector3? position = null,
    float? positionWeight = null,
    float? rotationWeight = null
)
```

**Returns:** gameObjectName, instanceId, effectorName, position, positionWeight, rotationWeight

### finalik-add-lookat

Adds LookAtIK (head/spine rotation toward target).

```csharp
AddIKResponse AddLookAtIK(
    GameObjectRef targetRef,
    Vector3? targetPosition = null
)
```

**Returns:** gameObjectName, instanceId, ikType, targetPosition

### finalik-add-aim

Adds AimIK (bone chain rotation for aiming/pointing).

```csharp
AddIKResponse AddAimIK(
    GameObjectRef targetRef,
    Vector3? targetPosition = null
)
```

**Returns:** gameObjectName, instanceId, ikType, targetPosition

### finalik-list-ik

Lists all Final IK components in scene.

```csharp
ListIKResponse ListIK()
```

**Returns:** count, details (type, enabled per component)

---

## 6. Asset Inventory (4 Tools)

**Files:** `MCPTools/AssetInventory/Editor/Tool_AssetInventory.*.cs`
**Asmdef:** `MCPTools.AssetInventory.Editor` | **Define:** `HAS_ASSETINVENTORY`

### asset-inventory-search

Searches Asset Inventory 4 database by name, type, or tag.

```csharp
SearchResponse SearchAssets(
    string searchPhrase,
    string? fileType = null,
    string? packageFilter = null,
    int maxResults = 20
)
```

**Returns:** resultCount, returnedCount, results (paths, package names, metadata)

### asset-inventory-search-prefabs

Convenience wrapper for prefab-only search.

```csharp
SearchResponse SearchPrefabs(
    string searchPhrase,
    string? packageFilter = null,
    int maxResults = 20
)
```

**Returns:** resultCount, returnedCount, results

### asset-inventory-import

Imports asset from indexed package into project (even if package not installed).

```csharp
ImportResponse ImportAsset(
    string assetPath,
    bool withDependencies = true,
    bool addToScene = false,
    Vector3? position = null
)
```

**Returns:** assetPath, packageName, addToScene, message
**Note:** Uses `EditorApplication.delayCall` for async import. Watch Console for completion.

### asset-inventory-list-packages

Lists all indexed packages in Asset Inventory database.

```csharp
ListPackagesResponse ListPackages(
    string? nameFilter = null,
    int maxResults = 50
)
```

**Returns:** packageCount, details (names, file counts, versions)

---

## Malbers AC (8 Tools)

**Files:** `MalbersAC/Editor/Tool_MalbersAC.*.cs`
**Asmdef:** `MCPTools.MalbersAC.Editor` | **Define:** `HAS_MALBERS_AC`

### ac-query-animal

Reads full MAnimal setup: lists all states (name, ID, active, priority), modes (name, ID, active, abilities), stances (name, ID, enabled), and speed sets (name, speeds, indices).

```csharp
QueryAnimalResponse QueryAnimal(
    GameObjectRef targetRef
)
```

**Returns:** gameObjectName, activeState, stateCount, modeCount, stanceCount, speedSetCount, details (full breakdown)

### ac-query-stats

Reads all stats on a Stats component.

```csharp
QueryStatsResponse QueryStats(
    GameObjectRef targetRef
)
```

**Returns:** gameObjectName, statCount, details (name, ID, value, max, regen, degen per stat)

### ac-configure-state

Configures a state by name (partial match). Only provided params changed.

```csharp
ConfigureStateResponse ConfigureState(
    GameObjectRef targetRef,
    string stateName,              // e.g. "Locomotion", "Jump", "Swim"
    bool? active = null,
    int? priority = null,
    string? input = null,
    bool? canStrafe = null,
    bool? canTransitionToItself = null
)
```

**Returns:** gameObjectName, stateName, active, priority, input, canStrafe

### ac-configure-mode

Configures a mode by name (partial match). Only provided params changed.

```csharp
ConfigureModeResponse ConfigureMode(
    GameObjectRef targetRef,
    string modeName,               // e.g. "Attack1", "Action", "Damage"
    bool? active = null,
    float? coolDown = null,
    bool? allowMovement = null,
    bool? allowRotation = null,
    bool? ignoreLowerModes = null,
    string? input = null
)
```

**Returns:** gameObjectName, modeName, active, coolDown, allowMovement, allowRotation, abilityCount

### ac-configure-speed

Configures a speed entry within a speed set. Both names use partial match.

```csharp
ConfigureSpeedResponse ConfigureSpeed(
    GameObjectRef targetRef,
    string speedSetName,           // e.g. "Ground", "Swim", "Fly"
    string speedName,              // e.g. "Walk", "Trot", "Run"
    float? position = null,
    float? lerpPosition = null,
    float? rotation = null,
    float? animator = null,
    float? lerpAnimator = null
)
```

**Returns:** gameObjectName, speedSetName, speedName, position, lerpPosition, rotation, animator, lerpAnimator

### ac-configure-stat

Configures a specific stat by name (partial match). Only provided params changed.

```csharp
ConfigureStatResponse ConfigureStat(
    GameObjectRef targetRef,
    string statName,               // e.g. "Health", "Stamina", "Energy"
    bool? active = null,
    float? value = null,
    float? maxValue = null,
    float? minValue = null,
    float? regenRate = null,
    float? degenRate = null,
    float? regenWaitTime = null,
    float? immuneTime = null
)
```

**Returns:** gameObjectName, statName, active, value, maxValue, minValue, regenRate

### ac-configure-damageable

Configures MDamageable component. Only provided params changed.

```csharp
ConfigureDamageableResponse ConfigureDamageable(
    GameObjectRef targetRef,
    float? multiplier = null,
    bool? alignToDamage = null,
    float? alignTime = null
)
```

**Returns:** gameObjectName, multiplier, alignToDamage, alignTime

### ac-add-lock-axis

Adds or configures LockAxis component for 2.5D gameplay.

```csharp
LockAxisResponse AddLockAxis(
    GameObjectRef targetRef,
    bool lockX = false,
    bool lockY = false,
    bool lockZ = true,             // Standard 2.5D side-scroller
    float offsetX = 0f,
    float offsetY = 0f,
    float offsetZ = 0f
)
```

**Returns:** gameObjectName, lockX, lockY, lockZ, offset

---

## Quest Forge (5 Tools)

**Files:** `QuestForge/Editor/Tool_QuestForge.*.cs`
**Asmdef:** None (`#if HAS_MALBERS_QUESTFORGE` guards) | **Define:** `HAS_MALBERS_QUESTFORGE`

### qf-create-quest

Creates a Quest ScriptableObject asset.

```csharp
CreateQuestResponse CreateQuest(
    string assetPath,              // e.g. "Assets/_AQS/Data/Quests/Quest_FindJoey.asset"
    string questId,                // Unique ID: "find_joey_01"
    string questName,
    string description,
    string questType = "Side",     // Main, Side, Daily, Repeatable
    bool isRepeatable = false
)
```

**Returns:** assetPath, questId, questName, questType, isRepeatable

### qf-query-quests

Lists all Quest SOs in the project with objectives.

```csharp
QueryQuestsResponse QueryQuests(
    string? searchFolder = null    // e.g. "Assets/_AQS/Data/Quests"
)
```

**Returns:** questCount, details (ID, name, type, objectives per quest)

### qf-add-objective

Adds an objective to an existing Quest SO. Types: Kill, Collect, TalkTo, GoToLocation, Interact.

```csharp
AddObjectiveResponse AddObjective(
    string questAssetPath,
    string objectiveType,          // Kill, Collect, TalkTo, GoToLocation, Interact
    string targetId,               // Enemy tag / item ID / NPC ID / location ID / interactable ID
    int requiredCount = 1,
    string? specificId = null,     // Specific enemy/dialogue ID
    float posX = 0f, float posY = 0f, float posZ = 0f,  // GoToLocation only
    float requiredDistance = 5f    // GoToLocation only
)
```

**Returns:** questAssetPath, questId, objectiveType, targetId, requiredCount, totalObjectives

### qf-create-poi

Creates a PointOfInterest ScriptableObject for minimap/compass.

```csharp
CreatePOIResponse CreatePOI(
    string assetPath,
    string poiName,
    string category = "Waypoint",  // QuestObjective, QuestGiver, Waypoint, Location, Enemy, NPC, Item, Merchant, FastTravel, Custom
    string? locationId = null,
    float posX = 0f, float posY = 0f, float posZ = 0f,
    float radius = 5f,
    bool showOnMinimap = true,
    bool showOnCompass = true,
    bool enableFastTravel = false,
    bool hideUntilDiscovered = false,
    string? description = null,
    int priority = 0
)
```

**Returns:** assetPath, poiName, category, locationId, worldPosition, radius, showOnMinimap, showOnCompass

### qf-query-pois

Lists all PointOfInterest SOs in the project.

```csharp
QueryPOIsResponse QueryPOIs(
    string? searchFolder = null
)
```

**Returns:** poiCount, details (name, category, locationId, position, radius, display settings per POI)

---

## Retarget Pro (4 Tools)

**Files:** `RetargetPro/Editor/Tool_RetargetPro.*.cs`
**Asmdef:** `MCPTools.RetargetPro.Editor` | **Define:** `HAS_RETARGETPRO`

### retarget-batch-bake

Batch-retargets a folder of AnimationClips using a RetargetProfile. No batch UI exists in the asset -- this tool fills that gap.

```csharp
BatchBakeResponse BatchBake(
    string profilePath,            // Path to RetargetProfile SO
    string sourceFolder,           // Folder of source AnimationClips
    string outputFolder,           // Output folder (created if needed)
    float frameRate = 30f,         // 24-240
    bool copyClipSettings = true,  // Loop time, events
    bool useRootMotion = true,
    bool keyframeAll = true,       // false = optimize curves
    int maxClips = 0               // 0 = all, >0 = limit for testing
)
```

**Returns:** profilePath, sourceFolder, outputFolder, totalClipsFound, processed, failed, details (per-clip results)

**Note:** Uses reflection to set private fields on `RetargetAnimBaker` (_frameRate, _copyClipSettings, _useRootMotion, _keyframeAll, _savePath).

### retarget-create-profile

Creates a RetargetProfile ScriptableObject with source/target character references.

```csharp
CreateProfileResponse CreateProfile(
    string assetPath,
    string sourceCharacterPath,    // Prefab with KRigComponent
    string targetCharacterPath,    // Prefab with KRigComponent
    string? sourceRigPath = null,  // KRig asset
    string? targetRigPath = null,
    string? sourcePosePath = null, // A-pose or T-pose clip
    string? targetPosePath = null
)
```

**Returns:** assetPath, sourceCharacter, targetCharacter, hasSourceRig, hasTargetRig, hasSourcePose, hasTargetPose

**Note:** After creating the profile, retarget features (BasicRetarget, IKRetarget, etc.) must be added via the Inspector as they are nested ScriptableObject sub-assets.

### retarget-query-profiles

Lists all RetargetProfile SOs in the project.

```csharp
QueryProfilesResponse QueryProfiles(
    string? searchFolder = null
)
```

**Returns:** profileCount, details (name, source/target chars, rigs, feature count, poses per profile)

---

---

## 27. Bro Audio (4 Tools)

**Files:** `BroAudio/Editor/Tool_BroAudio.*.cs`
**Asmdef:** `MCPTools.BroAudio.Editor` | **Define:** `HAS_BROAUDIO`

**Note:** BroAudio v2 replaced integer IDs with direct `AudioEntity` ScriptableObject references. Tools now use entity names (strings) instead of int IDs. Entities found via `Resources.FindObjectsOfTypeAll<AudioEntity>()`.

### bro-query

Lists all AudioEntity assets, their names, and playing state.

```csharp
string Query()
```

**Returns:** Entity count, per entity: Name, IsPlaying.

### bro-play

Plays a sound entity by name with optional fade-in and 3D position.

```csharp
string Play(
    string entityName,
    float fadeIn = 0f,
    string? position = null    // "x,y,z" for 3D spatialized playback
)
```

**Returns:** OK with entity name, or ERROR if entity not found.

### bro-stop

Stops a sound by entity name (with fade) or stops all sounds of a named BroAudioType.

```csharp
string Stop(
    string? entityName = null,
    string? audioType = null,    // BroAudioType enum name: "All", "BGM", "SFX", "Ambience", "Generic", "UI"
    float fadeOut = 0f
)
```

**Returns:** OK with stopped target.

### bro-volume

Sets volume for a named entity or all sounds of a BroAudioType.

```csharp
string SetVolume(
    float volume,
    float fadeTime = 0f,
    string? entityName = null,
    string? audioType = null
)
```

**Returns:** OK with target and new volume.

---

## 28. Koreographer (2 Tools)

**Files:** `Koreographer/Editor/Tool_Koreographer.cs`
**Asmdef:** `MCPTools.Koreographer.Editor` | **Define:** `HAS_KOREOGRAPHER`
**Assembly:** Compiled DLL (`SonicBloom.Koreo.dll`)

### koreo-query

Lists all loaded Koreography assets and their beat track event IDs. Reports current beat time.

```csharp
string Query()
```

**Returns:** Koreographer initialized state, koreography count, per koreography: SourceClipName, event track IDs via `GetEventIDs()`, plus current beat time.

### koreo-beattime

Gets current beat time and beat time delta for a named track.

```csharp
string GetBeatTime(
    string trackName,
    int subdivision = 1    // Beats per measure subdivision
)
```

**Returns:** trackName, subdivision, beatTime, beatTimeDelta, or ERROR if Koreographer not initialized.

---

## 29. PMG -- Procedural Music Generator (4 Tools)

**Files:** `PMG/Editor/Tool_PMG.cs`
**No asmdef** -- uses `#if HAS_PMG` guard | **Define:** `HAS_PMG`
**Note:** PMG is a MonoBehaviour. No static singleton -- uses `FindFirstObjectByType<MusicGenerator>()`.

### pmg-query

Reads current PMG state: generator state, tempo, key, scale, mode, and auto-play setting.

```csharp
string Query()
```

**Returns:** GeneratorState, Tempo, KeySteps, Scale (enum name), Mode (enum name), AutoPlay.

### pmg-play

Starts the PMG music generator.

```csharp
string Play()
```

**Returns:** OK, or ERROR if no MusicGenerator found.

### pmg-stop

Stops the PMG music generator.

```csharp
string Stop()
```

**Returns:** OK, or ERROR if no MusicGenerator found.

### pmg-configure

Sets PMG configuration parameters. All params are optional -- only provided ones are changed.

```csharp
string Configure(
    float? tempo = null,
    int? keySteps = null,           // Semitones from C (0-11)
    string? scale = null,           // e.g. "Major", "Minor", "Dorian"
    string? mode = null             // e.g. "Ionian", "Dorian", "Phrygian"
)
```

**Returns:** Updated values for each changed param, or ERROR if MusicGenerator not found / enum parse fails.

---

## 30. Maestro MIDI (4 Tools)

**Files:** `Maestro/Editor/Tool_Maestro.cs`
**Asmdef:** `MCPTools.Maestro.Editor` | **Define:** `HAS_MAESTRO`

### maestro-query

Lists all MidiFilePlayer and MidiStreamPlayer components in the scene with their current state.

```csharp
string Query()
```

**Returns:** Per player: GO name, type (FilePlayer/StreamPlayer), MPTK_MidiName, IsPlaying, IsPaused, MPTK_Tempo, MPTK_Speed, MPTK_Loop.

### maestro-play

Starts playback on a named MidiFilePlayer.

```csharp
string Play(
    string playerName,         // GameObject name of the MidiFilePlayer
    string? midiName = null,   // MIDI file name (from MPTK library)
    bool loop = false,
    float speed = 1f
)
```

**Returns:** OK with player name and MIDI name, or ERROR if not found.

### maestro-stop

Stops a named MIDI player.

```csharp
string Stop(
    string playerName          // GameObject name of MidiFilePlayer or MidiStreamPlayer
)
```

**Returns:** OK with player name, or ERROR if not found.

### maestro-send-note

Sends an immediate MIDI note event to a named MidiStreamPlayer.

```csharp
string SendNote(
    string playerName,         // GameObject name of the MidiStreamPlayer
    int note,                  // MIDI note 0-127 (60 = middle C)
    int channel = 0,           // MIDI channel 0-15
    int velocity = 100,        // Note velocity 0-127
    int durationMs = 500,      // Duration in milliseconds
    int patch = 0              // GM instrument preset 0-127
)
```

**Returns:** OK with note, channel, velocity, duration, patch, or ERROR if player not found.

---

## 31. DryWetMIDI (1 Tool)

**Files:** `DryWetMIDI/Editor/Tool_DryWetMIDI.cs`
**Asmdef:** `MCPTools.DryWetMIDI.Editor` | **Define:** `HAS_DRYWETMIDI`
**Assembly:** Compiled DLL (`Melanchall.DryWetMidi.dll`)

### midi-query-devices

Lists all available MIDI input and output devices on the machine.

```csharp
string QueryDevices()
```

**Returns:** Output device count + names, input device count + names. Each device is disposed after listing. Errors per-device-type are caught and reported inline.

---

## 32. FMOD Studio (5 Tools)

**Files:** `FMOD/Editor/Tool_FMOD.cs`
**Asmdef:** `MCPTools.FMOD.Editor` | **Define:** `HAS_FMOD`

### fmod-query

Returns FMOD Studio runtime state: initialization, bank load status, mute state.

```csharp
string Query()
```

**Returns:** IsInitialized, HaveAllBanksLoaded, HaveMasterBanksLoaded, IsMuted, AnyBankLoading.

### fmod-play

Plays an FMOD Studio event as a one-shot (fire and forget).

```csharp
string PlayOneShot(
    string eventPath,           // e.g. "event:/Music/MainTheme"
    string? position = null     // "x,y,z" for 3D spatialized playback
)
```

**Returns:** OK with event path and position, or ERROR if not initialized.

### fmod-parameter

Sets a global FMOD Studio parameter by name.

```csharp
string SetGlobalParameter(
    string parameterName,
    float value,
    bool ignoreSeekSpeed = true
)
```

**Returns:** OK with name and value, or FMOD RESULT error code. Parameter must be declared as a global parameter in FMOD Studio (not local to an event).

### fmod-vca

Sets the volume of an FMOD Studio VCA.

```csharp
string SetVCAVolume(
    string vcaPath,             // e.g. "vca:/Master" or "vca:/Music"
    float volume                // 0.0 to 1.0 (clamped)
)
```

**Returns:** OK with path, old volume, and new volume, or FMOD RESULT error code.

### fmod-bus

Sets the fader level of an FMOD Studio Bus.

```csharp
string SetBusVolume(
    string busPath,             // e.g. "bus:/" or "bus:/Music"
    float volume                // 0.0 to 1.0 (clamped)
)
```

**Returns:** OK with path, old volume, and new volume, or FMOD RESULT error code.

---

## 33. Chunity (4 Tools)

**Files:** `Chunity/Editor/Tool_Chunity.cs`
**No asmdef** -- uses `#if HAS_CHUNITY` guard | **Define:** `HAS_CHUNITY`

### chuck-query

Lists all ChuckMainInstance components in the scene.

```csharp
string Query()
```

**Returns:** Instance count, per instance: index, GO name, activeInHierarchy. Use the GO name as `instanceName` in other chuck-* tools.

### chuck-run

Runs a ChucK code string on a named ChuckMainInstance.

```csharp
string RunCode(
    string instanceName,        // GameObject name of the ChuckMainInstance
    string code                 // Valid ChucK DSP code
)
```

**Example code:** `"SinOsc s => dac; 440 => s.freq; 1 => s.gain; 2::second => now;"`

**Returns:** OK if code submitted, ERROR if instance not found or RunCode returned false.

### chuck-set-float

Sets a global float variable in a ChuckMainInstance's VM.

```csharp
string SetFloat(
    string instanceName,
    string variableName,        // Must be declared as "global float myVar;" in ChucK code
    double value
)
```

**Returns:** OK with instance, variable, and value, or ERROR with hint to check global declaration.

### chuck-set-int

Sets a global int variable in a ChuckMainInstance's VM.

```csharp
string SetInt(
    string instanceName,
    string variableName,        // Must be declared as "global int myVar;" in ChucK code
    long value
)
```

**Returns:** OK with instance, variable, and value, or ERROR with hint to check global declaration.

---

## 34. Naninovel (5 Tools)

**Files:** `Naninovel/Editor/Tool_Naninovel.*.cs`
**Asmdef:** `MCPTools.Naninovel.Editor` | **Define:** `HAS_NANINOVEL`

### nani-list-characters

Lists all characters registered in Naninovel project configuration. Shows IDs, display names, colors, poses.

```csharp
string ListCharacters()
```

**Returns:** Character list with metadata from `CharactersConfiguration`.

### nani-list-backgrounds

Lists all backgrounds registered in Naninovel project configuration.

```csharp
string ListBackgrounds()
```

**Returns:** Background list with IDs and implementation types from `BackgroundsConfiguration`.

### nani-list-scripts

Lists all .nani scenario script files in the project.

```csharp
string ListScripts()
```

**Returns:** File names, paths, and sizes grouped by folder.

### nani-read-script

Reads the contents of a .nani script file by name.

```csharp
string ReadScript(
    string scriptName     // Name without extension, case-insensitive, partial match
)
```

**Returns:** Full script content with line numbers.

### nani-list-commands

Lists available Naninovel script commands discovered via reflection.

```csharp
string ListCommands(
    string? filter = null // Optional name/alias filter
)
```

**Returns:** Command names, aliases (e.g., `@char`, `@back`), and descriptions.

---

## 35. Adventure Creator (5 Tools)

**Files:** `AdventureCreator/Editor/Tool_AdventureCreator.*.cs`
**Asmdef:** `MCPTools.AdventureCreator.Editor` | **Define:** `HAS_ADVENTURE_CREATOR`

### ac-query-managers

Shows status of all 8 AC managers with basic statistics.

```csharp
string QueryManagers()
```

**Returns:** Manager assignment status, item/variable/action counts.

### ac-list-inventory

Lists inventory items, categories, recipes, and documents from InventoryManager.

```csharp
string ListInventory()
```

**Returns:** Items (ID, label, category), categories, recipes, documents. Limited to 100 items.

### ac-list-variables

Lists global variables from VariablesManager.

```csharp
string ListVariables()
```

**Returns:** Variables (ID, label, type). Limited to 100.

### ac-list-actions

Lists available Action types from ActionsManager.

```csharp
string ListActions(
    string? filter = null // Optional title/filename filter
)
```

**Returns:** Sorted action list (title, filename).

### ac-find-scene-objects

Finds AC objects in the current scene.

```csharp
string FindSceneObjects()
```

**Returns:** Hotspots, NPCs, Players, Markers, ActionLists with positions and action counts.

---

## 36. Text Animator (4 Tools)

**Files:** `TextAnimator/Editor/Tool_TextAnimator.*.cs`
**Asmdef:** `MCPTools.TextAnimator.Editor` | **Define:** `HAS_TEXT_ANIMATOR`

### ta-list-effects

Lists all Text Animator effects discovered via `[EffectInfoAttribute]` reflection.

```csharp
string ListEffects(
    string? filter = null // "behaviors", "appearances", or tag name search
)
```

**Returns:** Effects grouped by category (Behavior/Appearance) with tag IDs and class names.

### ta-find-components

Finds TextAnimator components in the current scene.

```csharp
string FindComponents()
```

**Returns:** Component list with GameObject name, type, animation loop mode, current text.

### ta-list-databases

Lists AnimationsDatabase assets in the project.

```csharp
string ListDatabases()
```

**Returns:** Database names and asset paths.

### ta-get-settings

Lists AnimatorSettingsScriptable assets in the project.

```csharp
string GetSettings()
```

**Returns:** Settings asset names and paths.

---

## 37. Ink Integration (3 Tools)

**Files:** `InkIntegration/Editor/Tool_InkIntegration.*.cs`
**Asmdef:** `MCPTools.InkIntegration.Editor` | **Define:** `HAS_INK`

### ink-list-files

Lists all .ink files via InkLibrary.

```csharp
string ListFiles()
```

**Returns:** File paths with master/include status, auto-compile flag, compiled status.

### ink-compile

Compiles .ink files to JSON.

```csharp
string CompileInk(
    string? fileName = null // File name without extension, or empty for all master files
)
```

**Returns:** Compilation status. Check Unity console for results.

### ink-get-story-info

Loads a compiled Ink story and reports its structure.

```csharp
string GetStoryInfo(
    string fileName     // .ink file name without extension
)
```

**Returns:** Variables (name + value), content line count, choice points, global tags.

---

## Behavior Designer Pro 3 (5 Tools)

**Define:** `HAS_BEHAVIOR_DESIGNER` | **Pattern:** `#if` guard (no asmdef) | **Namespace:** `MCPTools.BehaviorDesigner.Editor`

Behavior tree authoring, runtime control, and state inspection for Opsive Behavior Designer Pro 3 (DOTS-powered). All tools use the MonoBehaviour API which is backward compatible with Pro 2.

### bd-query

Queries a BehaviorTree component on a GameObject. Reports tree name, enabled state, configuration (UpdateMode, EvaluationType), shared variables with values. In play mode, also reports runtime state (Status, IsActive, IsRunning, IsPaused).

```csharp
string Query(
    string gameObjectName,   // GameObject with BehaviorTree
    int? treeIndex = 0       // Index if multiple trees
)
```

**Returns:** Tree name, enabled, StartWhenEnabled, PauseWhenDisabled, UpdateMode, EvaluationType, runtime state (play mode), shared variables list.

### bd-control

Controls behavior tree execution. Start, stop, pause, unpause (resume), or restart a tree.

```csharp
string Control(
    string gameObjectName,   // GameObject with BehaviorTree
    string action,           // "start", "stop", "pause", "unpause"/"resume", "restart"
    int? treeIndex = 0       // Index if multiple trees
)
```

**Actions:**
- `start` / `enable` -- `StartBehavior()`, also resumes paused trees
- `stop` / `disable` -- `StopBehavior(false)`, fully stops the tree
- `pause` -- `StopBehavior(true)`, pauses without clearing state
- `unpause` / `resume` -- resumes a paused tree via `StartBehavior()`
- `restart` -- `RestartBehavior()`, stops then starts

### bd-list-trees

Lists all BehaviorTree components in the current scene.

```csharp
string ListTrees()
```

**Returns:** Per tree: GameObject name, tree name, enabled state, variable count. In play mode, also shows TaskStatus (Inactive, Queued, Running, Success, Failure).

### bd-set-variable

Gets or sets a SharedVariable on a BehaviorTree by name.

```csharp
string SetVariable(
    string gameObjectName,   // GameObject with BehaviorTree
    string variableName,     // Shared variable name
    string? action = "get",  // "get" or "set"
    string? value = null,    // Value for set action
    string? valueType = "string", // "bool", "float", "int", "string", "vector3"
    int? treeIndex = 0       // Index if multiple trees
)
```

**Supported types:** bool, float, int, string, vector3 (parsed as `x,y,z`).

### bd-tick

Manually ticks a BehaviorTree. For trees with `UpdateMode.Manual` that are not auto-updated each frame. Play mode only.

```csharp
string Tick(
    string gameObjectName,   // GameObject with BehaviorTree
    int? count = 1,          // Number of ticks to perform
    int? treeIndex = 0       // Index if multiple trees
)
```

**Returns:** Tick count performed and current TaskStatus after ticking.

---

## Juicy Actions

**Files:** `JuicyActions/Editor/Tool_JuicyActions.cs`, `.Query.cs`, `.Play.cs`
**Define:** `HAS_JUICY_ACTIONS`
**Pattern:** `#if` guard + reflection (no asmdef)
**Assembly:** `MagicPigGames.JuicyActions.Runtime`

### juicy-query -- Query Triggers

Lists all Juicy Actions trigger components (ActionOnEvent-derived) on a GameObject. Reports each trigger's type, enabled state, and ActionExecutor details (item count, time mode, cooldown).

```csharp
string QueryTriggers(
    string gameObjectName    // GameObject with Juicy Actions triggers
)
```

**Returns:** Trigger list with type names, enabled state, and per-executor item counts.

### juicy-play -- Play Trigger

Triggers execution of a Juicy Actions trigger at runtime. Play mode only.

```csharp
string PlayTrigger(
    string gameObjectName,   // GameObject with Juicy Actions triggers
    int triggerIndex = 0     // Zero-based index of which trigger to fire
)
```

**Returns:** Confirmation of which trigger was fired.

---

## Boing Kit

**Files:** `BoingKit/Editor/Tool_BoingKit.cs`, `.Query.cs`, `.Configure.cs`
**Define:** `HAS_BOINGKIT`
**Pattern:** `#if` guard + reflection (no asmdef)
**Assembly:** `BoingKit`

### boing-query -- Query Components

Reports all Boing Kit components on a GameObject (BoingEffector, BoingBehavior, BoingBones, BoingReactorField) and their configuration.

```csharp
string QueryBoingComponents(
    string gameObjectName    // GameObject to inspect
)
```

**Returns:** Per-component field dump: effector radius/impulse, behavior effect toggles/locks, bones chain count/debug, reactor field cells/falloff/propagation.

### boing-configure -- Configure Component

Configures a specific Boing Kit component. Only provided parameters are changed.

```csharp
string ConfigureBoingComponent(
    string gameObjectName,           // GameObject with the component
    string componentType,            // "Effector", "Behavior", "Bones", or "ReactorField"
    // Effector params:
    float? radius = null,            // [0-20] Effect radius
    float? fullEffectRadiusRatio = null, // [0-1]
    float? maxImpulseSpeed = null,   // [0-100]
    bool? continuousMotion = null,
    float? moveDistance = null,       // [-10, 10]
    float? linearImpulse = null,     // [-200, 200]
    float? rotationAngle = null,     // [-180, 180]
    float? angularImpulse = null,    // [-2000, 2000]
    // Behavior/ReactorField params:
    bool? enablePositionEffect = null,
    bool? enableRotationEffect = null,
    bool? enableScaleEffect = null,  // Behavior only
    // ReactorField params:
    float? cellSize = null,          // [0.1-10]
    int? cellsX = null, int? cellsY = null, int? cellsZ = null,
    string? falloffMode = null,      // "None", "Circle", "Square"
    float? falloffRatio = null,      // [0-1]
    bool? enablePropagation = null
)
```

**Returns:** List of changes applied.

---

## MudBun

**Files:** `MudBun/Editor/Tool_MudBun.cs`, `.Query.cs`, `.ConfigureRenderer.cs`, `.ConfigureBrush.cs`
**Define:** `HAS_MUDBUN`
**Pattern:** `#if` guard + reflection (no asmdef)
**Assembly:** `MudBun`

### mudbun-query -- Query Renderer & Brushes

Reports MudBun renderer settings and child brush hierarchy.

```csharp
string QueryMudBun(
    string gameObjectName    // GameObject with MudRenderer
)
```

**Returns:** Renderer config (RenderMode, MeshingMode, VoxelDensity, MaxVoxelsK, MasterColor/Metallic/Smoothness) + per-brush list (type, Operator, Blend, type-specific params, material Color/Emission/Metallic/Smoothness).

### mudbun-configure-renderer -- Configure Renderer

Configures a MudRenderer component. All params optional except gameObjectName.

```csharp
string ConfigureRenderer(
    string gameObjectName,           // GameObject with MudRenderer
    string? renderMode = null,       // "FlatMesh", "SmoothMesh", "CircleSplats", "QuadSplats", "Decal"
    string? meshingMode = null,      // "MarchingCubes", "DualQuads", "SurfaceNets", "DualContouring"
    float? voxelDensity = null,      // [0.1-100]
    int? maxVoxelsK = null,          // [1-2048]
    string? masterColorHex = null,   // "#RRGGBB" or "#RRGGBBAA"
    float? masterMetallic = null,    // [0-1]
    float? masterSmoothness = null,  // [0-1]
    float? surfaceShift = null,      // [-1, 1]
    bool? enable2dMode = null,
    bool? castShadows = null,
    bool? receiveShadows = null,
    float? splatSize = null          // [0-5] for splat render modes
)
```

**Returns:** List of changes applied + MarkDirty confirmation.

### mudbun-configure-brush -- Configure Brush

Configures a MudBun brush (primitive) on a named child GameObject.

```csharp
string ConfigureBrush(
    string gameObjectName,           // The brush GameObject (child of MudRenderer)
    string? @operator = null,        // "Union", "Subtract", "Intersect", "Dye", "Pipe", "Engrave"
    float? blend = null,
    float? radius = null,            // Sphere, Cylinder
    float? round = null,             // Box, Cylinder
    string? colorHex = null,         // "#RRGGBB"
    string? emissionHex = null,      // "#RRGGBB"
    float? metallic = null,          // [0-1]
    float? smoothness = null         // [0-1]
)
```

**Returns:** Brush type detected + list of changes applied.

---

## Lumen (Distant Lands)

**Files:** `Lumen/Editor/Tool_Lumen.cs`, `.Query.cs`, `.Configure.cs`
**Define:** `HAS_LUMEN`
**Pattern:** `#if` guard + reflection (no asmdef)
**Assembly:** `DistantLands.Lumen.Runtime`

### lumen-query -- Query Effect Player

Reports LumenEffectPlayer configuration on a GameObject.

```csharp
string QueryLumen(
    string gameObjectName    // GameObject with LumenEffectPlayer
)
```

**Returns:** scale, brightness, color, range, updateFrequency, fadingTime, init/deinit behaviors, profile name, instantiated layer count.

### lumen-configure -- Configure Effect Player

Configures a LumenEffectPlayer component. All params optional except gameObjectName. Includes runtime fade methods for play mode.

```csharp
string ConfigureLumen(
    string gameObjectName,           // GameObject with LumenEffectPlayer
    float? scale = null,             // Uniform scale modifier
    float? brightness = null,        // Uniform brightness modifier
    string? colorHex = null,         // "#RRGGBB" color modifier
    float? range = null,             // Range multiplier for light layers
    float? fadingTime = null,        // Transition duration (seconds)
    string? updateFrequency = null,  // "Always", "OnChanges", "ViaScripting"
    string? initBehavior = null,     // "Immediate", "FadeToTargetBrightness", etc.
    string? deinitBehavior = null,   // "Immediate", "FadeBrightnessToZero", etc.
    bool? autoAssignSun = null,      // Auto-assign directional light as sun
    // Runtime fade (play mode only):
    float? fadeToBrightness = null,  // Smooth fade to brightness
    float? fadeToScale = null,       // Smooth fade to scale
    string? fadeToColorHex = null    // Smooth fade to color
)
```

**Returns:** List of changes applied. Runtime fades use fadingTime as duration.

---

## uLipSync (3 tools)

**Asset:** uLipSync 3.1.5 (hecomi) -- MFCC-based lip sync
**Define:** `HAS_ULIPSYNC`
**Detection:** `uLipSync.uLipSync, uLipSync.Runtime`
**Namespace:** `MCPTools.uLipSync.Editor`
**Compilation:** `#if HAS_ULIPSYNC` guards (no asmdef -- reflection-based)
**Source:** `uLipSync/Editor/Tool_uLipSync*.cs`
**Added:** M3AnimatedSeries Session 2, April 9, 2026

### lipsync-query

Reports all uLipSync components on a GameObject hierarchy and their configuration. Lists profile phonemes, blendshape mappings, baked data info. Can also list all Profile and BakedData assets in the project.

```csharp
string QueryLipSync(
    string? gameObjectName = null,  // GO to inspect (searches hierarchy)
    bool listAssets = false          // Also list Profile/BakedData assets
)
```

**Returns:** Component details (analyzer profile, blendshape mappings with resolved names, baked data player config, timeline events, texture mappings) and optionally all available Profile/BakedData assets with paths and phoneme counts.

---

### lipsync-configure

Configures uLipSync components on a GameObject. Sets Profile on the analyzer, configures blendshape mappings, and adjusts volume/smoothness parameters.

```csharp
string ConfigureLipSync(
    string gameObjectName,
    // Analyzer
    string? profileName = null,          // Profile asset name
    float? outputSoundGain = null,       // [0-1] Mute playback: 0
    // BlendShape
    string? phonemes = null,             // Comma-separated: "A,I,U,E,O,N,-"
    string? blendShapeNames = null,      // Comma-separated blendshape names (must match phoneme count)
    string? skinnedMeshRendererName = null, // SMR child name
    float? minVolume = null,             // Log10, default -2.5
    float? maxVolume = null,             // Log10, default -1.5
    float? smoothness = null,            // [0-0.3], default 0.05
    bool? usePhonemeBlend = null,        // Weighted blend vs winner-take-all
    // BakedDataPlayer
    float? playerVolume = null,          // [0-1]
    float? timeOffset = null             // [-0.3 to 0.3] Positive = mouth opens earlier
)
```

**Returns:** List of changes applied. Validates blendshape names against the SkinnedMeshRenderer mesh.

---

### lipsync-bake

Bakes AudioClip(s) into uLipSync BakedData ScriptableObject assets using a calibrated Profile. Equivalent to Window > uLipSync > Baked Data Generator. Supports single clip or batch directory baking.

```csharp
string BakeLipSyncData(
    string profileName,              // Calibrated Profile asset name
    string? audioClipName = null,    // Single clip to bake (mutually exclusive with inputDirectory)
    string? inputDirectory = null,   // Directory of AudioClips to batch-bake
    string? outputDirectory = null   // Output dir (created if needed, default: same as input)
)
```

**Returns:** Per-clip bake results with output paths and durations. Creates BakedData .asset files ready for uLipSyncBakedDataPlayer or Timeline tracks.

---

## Timeflow (Axon Genesis)

**Files:** `Timeflow/Editor/Tool_Timeflow.cs`, `.Query.cs`, `.Control.cs`, `.ConfigureTween.cs`, `.ConfigureEvent.cs`
**Define:** `HAS_TIMEFLOW`
**Pattern:** `#if` guard + reflection (no asmdef)
**Assembly:** `Timeflow`

### timeflow-query -- Query Timeline

Reports Timeflow state, TimeflowObjects, and behaviors in hierarchy.

```csharp
string QueryTimeflow(
    string gameObjectName    // GameObject with Timeflow component
)
```

**Returns:** CurrentTime, StartTime, EndTime, IsPlaying, loop, time scale, auto-play settings, Director sync state, child Timeflow count, per-object behavior list.

### timeflow-control -- Playback Control

Controls Timeflow playback. Works in edit mode and play mode.

```csharp
string ControlTimeflow(
    string gameObjectName,           // GameObject with Timeflow
    string action,                   // "Play", "PlayFromStart", "PlayReverse", "Stop", "Pause", "SetTime"
    float? time = null,              // Target time for SetTime
    float? timeScale = null,         // GlobalTimeScale multiplier
    bool? loop = null                // Enable/disable looping
)
```

**Returns:** Action performed + current state (time, playing).

### timeflow-configure-tween -- Configure Tween

Configures a Tween behavior on a GameObject. All params optional.

```csharp
string ConfigureTween(
    string gameObjectName,                   // GameObject with Tween
    string? interpolation = null,            // "EaseIn", "EaseOut", "EaseInOut", "Linear", etc.
    string? repeatMode = null,               // "Forever", "Every", "None"
    float? minValue = null, float? maxValue = null,  // Float range
    float? minX = null, float? minY = null, float? minZ = null,  // Vector min
    float? maxX = null, float? maxY = null, float? maxZ = null,  // Vector max
    float? amount = null,                    // Strength multiplier
    bool? pingPong = null,                   // Alternate direction
    bool? invertInterpolation = null,
    bool? allowTrigger = null,               // External trigger
    bool? triggerIsToggle = null,
    float? timeOffset = null,                // Time offset (seconds)
    float? timeScale = null                  // Time scale multiplier
)
```

**Returns:** List of changes applied.

### timeflow-configure-event -- Configure Event

Configures a TimeflowEvent trigger on a GameObject.

```csharp
string ConfigureEvent(
    string gameObjectName,           // GameObject with TimeflowEvent
    float? triggerTime = null,       // Time when event fires (seconds)
    string? targetName = null,       // Target GO for SendMessage
    string? function = null,         // Method name to call
    string? parameter = null,        // String parameter
    int? triggerLimit = null,        // Max triggers (0 = unlimited)
    bool? lockTime = null,           // Lock time in editor
    bool? logEnabled = null          // Console logging
)
```

**Returns:** List of changes applied.

---

**End of Document**
