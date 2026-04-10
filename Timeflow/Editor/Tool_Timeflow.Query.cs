#if HAS_TIMEFLOW
#nullable enable
using System.Collections;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.Timeflow.Editor
{
    public partial class Tool_Timeflow
    {
        [McpPluginTool("timeflow-query", Title = "Timeflow / Query Timeline")]
        [Description(@"Reports the Timeflow timeline state on a GameObject.
Returns: current time, start/end time, playing state, loop, time scale,
auto-play settings, child Timeflow count, TimeflowObject count, and behavior list.
Use to inspect timeline setup before configuring or controlling playback.")]
        public string QueryTimeflow(
            [Description("Name of the GameObject with the Timeflow component.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                UnityEngine.Component tf = GetTimeflow(gameObjectName);
                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"=== Timeflow on '{gameObjectName}' ===");
                sb.AppendLine($"  CurrentTime:       {Get(tf, "CurrentTime")}");
                sb.AppendLine($"  StartTime:         {Get(tf, "StartTime")}");
                sb.AppendLine($"  EndTime:           {Get(tf, "EndTime")}");
                sb.AppendLine($"  IsPlaying:         {Get(tf, "IsPlaying")}");
                sb.AppendLine($"  IsPlayReverse:     {Get(tf, "IsPlayReverse")}");
                sb.AppendLine($"  LoopEnabled:       {Get(tf, "LoopEnabled")}");
                sb.AppendLine($"  GlobalTimeScale:   {Get(tf, "GlobalTimeScale")}");
                sb.AppendLine($"  AutoPlay:          {Get(tf, "AutoPlay")}");
                sb.AppendLine($"  ContinuousPlay:    {Get(tf, "ContinuousPlay")}");
                sb.AppendLine($"  PlayFromStart:     {Get(tf, "PlayFromStart")}");
                sb.AppendLine($"  AllowReplay:       {Get(tf, "AllowReplay")}");

                // Director sync
                object? director = Get(tf, "Director");
                sb.AppendLine($"  Director:          {(director != null ? "assigned" : "(none)")}");
                sb.AppendLine($"  DirectorSync:      {Get(tf, "DirectorSyncEnabled")}");

                // Children
                int childCount = GetCollectionCount(Get(tf, "TimeflowChildren"));
                sb.AppendLine($"  TimeflowChildren:  {childCount}");

                // List TimeflowObjects in hierarchy
                if (TimeflowObjectType != null)
                {
                    GameObject go = GameObject.Find(gameObjectName)!;
                    UnityEngine.Component[] objects = go.GetComponentsInChildren(TimeflowObjectType);
                    sb.AppendLine($"\n-- TimeflowObjects ({objects.Length}) --");

                    foreach (UnityEngine.Component obj in objects)
                    {
                        string objName = obj.gameObject.name;
                        bool updateAnim = (bool)(Get(obj, "UpdateAnimation") ?? true);
                        float animSpeed = (float)(Get(obj, "AnimationSpeed") ?? 1f);
                        int behaviorCount = GetCollectionCount(Get(obj, "Behaviors"));
                        sb.AppendLine($"  [{objName}] behaviors: {behaviorCount}, speed: {animSpeed}, active: {updateAnim}");

                        // List behaviors on this object
                        object? behaviors = Get(obj, "Behaviors");
                        if (behaviors is IList list)
                        {
                            foreach (object? b in list)
                            {
                                if (b == null) continue;
                                string bType = b.GetType().Name;
                                bool enabled = (bool)(Get(b, "enabled") ?? true);
                                sb.AppendLine($"    - {bType} (enabled: {enabled})");
                            }
                        }
                    }
                }

                return sb.ToString();
            });
        }
    }
}
#endif
