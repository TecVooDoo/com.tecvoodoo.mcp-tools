#if HAS_PEGASUS
#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using Component = UnityEngine.Component;

namespace MCPTools.Pegasus.Editor
{
    public partial class Tool_Pegasus
    {
        [AiTool("pegasus-query", Title = "Pegasus / Query", ReadOnlyHint = true)]
        [Description(@"Lists every Pegasus flythrough manager in the open scenes (read-only): name, target, POI count and positions, path length, type (Looped / SingleShot), state, minimum height above terrain, auto-start flag, speed.
Managers rename themselves 'Pegasus Manager - <target>' when their inspector is drawn; the other pegasus-* tools accept either name.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                List<Component> managers = AllManagers();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Pegasus managers: {managers.Count}");
                foreach (Component manager in managers)
                {
                    GameObject? target = GetField(manager, "m_target") as GameObject;
                    IList? pois = GetField(manager, "m_poiList") as IList;
                    sb.AppendLine($"  '{manager.gameObject.name}' target={(target != null ? "'" + target.name + "'" : "NONE (sampling returns 0,0,0)")}, " +
                                  $"POIs={pois?.Count ?? 0}, length={(float)GetField(manager, "m_totalDistance")!:F1} m, type={GetField(manager, "m_flythroughType")}, " +
                                  $"state={GetField(manager, "m_currentState")}, minHeight={(float)GetField(manager, "m_minHeightAboveTerrain")!:F2} m, " +
                                  $"autoStart={GetField(manager, "m_autoStartAtRuntime")}, speed={(float)GetField(manager, "m_managerSpeed")!:F1}");
                    if (pois == null)
                        continue;
                    for (int i = 0; i < pois.Count; i++)
                    {
                        if (pois[i] is Component poi)
                        {
                            Vector3 p = poi.transform.position;
                            sb.AppendLine($"    [{i}] '{poi.name}' ({p.x:F1}, {p.y:F1}, {p.z:F1})");
                        }
                    }
                }
                return sb.ToString();
            });
        }
    }
}
#endif
