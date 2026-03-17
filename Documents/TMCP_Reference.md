# TecVooDoo MCP Tools -- API Reference

**Package:** `com.tecvoodoo.mcp-tools` v1.0.0
**Source:** `E:\Unity\SyntyAssets\Assets\MCPTools\`
**Last Updated:** March 14, 2026

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

**End of Document**
