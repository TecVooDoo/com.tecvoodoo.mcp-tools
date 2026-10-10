#if HAS_PEGASUS
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;
using Component = UnityEngine.Component;

namespace MCPTools.Pegasus.Editor
{
    public partial class Tool_Pegasus
    {
        [AiTool("pegasus-create", Title = "Pegasus / Create Flythrough")]
        [Description(@"Creates a Pegasus flythrough: manager 'name' with one POI per 'points' entry ('x,y,z' world positions), in order.
'lookAts' ('x,y,z'): one per point or one for all; omit to look along the path.
TARGET: by default a NEW disabled Camera '<name> Camera' (screenshot-camera still renders it) -- NOT the Main Camera, which Pegasus would move and a CinemachineBrain would fight. 'target' drives an existing GameObject instead.
The path stays >= minHeightAboveTerrain above ground (ENTRY-435: 0/300 samples below ground vs 176/300 for a Cinemachine dolly). Pegasus enforces it by MOVING the POIs up, so stored POIs can differ from 'points'.
IDEMPOTENT: an existing 'name' is not recreated (MCP re-send); replaceExisting=true rebuilds it.
Selecting a manager in the editor makes Pegasus add PEGASUS_PRESENT (a full recompile).
Then: pegasus-control scrub / sample.")]
        public string Create(
            [Description("Manager GameObject name.")] string name,
            [Description("POI world positions, in order, each 'x,y,z'. At least 2.")] string[] points,
            [Description("Look-at positions: one per point, or one for all. Omit to look along the path.")] string[]? lookAts = null,
            [Description("Existing GameObject to drive (name or hierarchy path). Omit to create '<name> Camera'.")] string? target = null,
            [Description("Looped (default) or SingleShot.")] string type = "Looped",
            [Description("Minimum camera height above terrain, metres. Pegasus default 1.8.")] float? minHeightAboveTerrain = null,
            [Description("Start automatically in play mode. Default false (Pegasus's own default is true).")] bool autoStartAtRuntime = false,
            [Description("Delete an existing manager with this name first. Default false.")] bool replaceExisting = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (Application.isPlaying)
                    throw new Exception("Create flythroughs in edit mode.");
                if (string.IsNullOrWhiteSpace(name))
                    throw new Exception("'name' is required.");
                if (points == null || points.Length < 2)
                    throw new Exception("'points' needs at least 2 entries ('x,y,z').");
                if (lookAts != null && lookAts.Length != 0 && lookAts.Length != 1 && lookAts.Length != points.Length)
                    throw new Exception($"'lookAts' must have 1 entry or {points.Length} (one per point).");

                string managerName = name.Trim();
                Vector3[] positions = new Vector3[points.Length];
                for (int i = 0; i < points.Length; i++)
                    positions[i] = ParseVector(points[i], $"points[{i}]");
                Vector3[]? looks = null;
                if (lookAts != null && lookAts.Length > 0)
                {
                    looks = new Vector3[lookAts.Length];
                    for (int i = 0; i < lookAts.Length; i++)
                        looks[i] = ParseVector(lookAts[i], $"lookAts[{i}]");
                }

                foreach (Component existing in AllManagers())
                {
                    string n = existing.gameObject.name;
                    if (n != managerName && n != "Pegasus Manager - " + managerName)
                        continue;
                    if (!replaceExisting)
                        return $"Pegasus manager '{n}' already exists -- NOT created again (a re-sent call would otherwise duplicate it). Pass replaceExisting=true to rebuild it. Inspect it with pegasus-query.";
                    Undo.DestroyObjectImmediate(existing.gameObject);
                }

                GameObject targetObject;
                bool createdCamera = false;
                if (!string.IsNullOrWhiteSpace(target))
                {
                    GameObject? found = GameObject.Find(target!.Trim());
                    if (found == null)
                        throw new Exception($"Target '{target}' not found (GameObject.Find: active objects, name or hierarchy path).");
                    targetObject = found;
                }
                else
                {
                    string cameraName = managerName + " Camera";
                    GameObject? existingCamera = GameObject.Find(cameraName);
                    if (existingCamera != null)
                    {
                        targetObject = existingCamera;
                    }
                    else
                    {
                        targetObject = new GameObject(cameraName);
                        Camera camera = targetObject.AddComponent<Camera>();
                        camera.enabled = false; // render on demand (screenshot-camera); never competes with the scene's cameras
                        Undo.RegisterCreatedObjectUndo(targetObject, "Create " + cameraName);
                        createdCamera = true;
                    }
                }

                GameObject managerObject = new GameObject(managerName);
                Undo.RegisterCreatedObjectUndo(managerObject, "Create " + managerName);
                Component manager = managerObject.AddComponent(ManagerType());

                // ENTRY-435: AddPOI throws (null m_defaults) until SetDefaults() runs; the menu item calls it, so must we.
                Call(manager, "SetDefaults");
                SetField(manager, "m_target", targetObject);
                SetEnumField(manager, "m_flythroughType", type);
                SetField(manager, "m_autoStartAtRuntime", autoStartAtRuntime);
                if (minHeightAboveTerrain.HasValue)
                    SetField(manager, "m_minHeightAboveTerrain", minHeightAboveTerrain.Value);

                for (int i = 0; i < positions.Length; i++)
                {
                    // AddPOI(pos, pos) = no look-at target, so the camera looks along the path.
                    Vector3 look = looks == null ? positions[i] : looks.Length == 1 ? looks[0] : looks[i];
                    Call(manager, "AddPOI", positions[i], look);
                }
                Call(manager, "InitialiseFlythrough");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Created Pegasus manager '{managerName}': {positions.Length} POIs, {GetField(manager, "m_flythroughType")}, " +
                              $"length {(float)GetField(manager, "m_totalDistance")!:F1} m, minHeight {(float)GetField(manager, "m_minHeightAboveTerrain")!:F2} m, autoStart {autoStartAtRuntime}.");
                sb.AppendLine($"Target: '{targetObject.name}'{(createdCamera ? " (new disabled Camera)" : "")}.");
                sb.AppendLine("Next: pegasus-control action='sample' to check terrain clearance, action='scrub' + screenshot-camera for stills.");
                return sb.ToString();
            });
        }
    }
}
#endif
