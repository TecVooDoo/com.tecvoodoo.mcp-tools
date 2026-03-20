#if HAS_FEEL
#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using MoreMountains.Feedbacks;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Feel.Editor
{
    public partial class Tool_Feel
    {
        [McpPluginTool("feel-add-feedback", Title = "Feel / Add Feedback to Player")]
        [Description(@"Adds a feedback to an MMF_Player's FeedbacksList and initializes the player.
feedbackType options (most useful):
  CameraShake, Position, Scale, Rotation, Light, Particles, Flash,
  Flicker, FreezeFrame, AudioSource, SetActive, Destroy, Enable,
  TimescaleModifier, InstantiateObject
label: display name shown in inspector. active: whether this feedback runs on play.
chance [0-100]: probability this feedback fires each play (100 = always).
targetObjectName: for transform-based feedbacks (Position, Scale, Rotation, Light, Particles, Flicker),
  sets the target automatically using GetComponent on the named GameObject.
After adding, use feel-configure-player to re-initialize if needed.")]
        public string AddFeedback(
            [Description("Name of the GameObject with MMF_Player.")] string gameObjectName,
            [Description("Feedback type to add. E.g.: CameraShake, Position, Scale, Rotation, Light, Particles, Flash, Flicker, FreezeFrame, AudioSource, TimescaleModifier.")] string feedbackType,
            [Description("Display label for this feedback in the inspector.")] string label = "",
            [Description("Whether this feedback is active.")] bool active = true,
            [Description("Probability this feedback fires [0-100]. 100 = always.")] float chance = 100f,
            [Description("Optional: name of a GameObject to auto-assign as this feedback's target (for transform/light/particle feedbacks).")] string? targetObjectName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var player = GetPlayer(gameObjectName);

                string typeName = $"MoreMountains.Feedbacks.MMF_{feedbackType}";
                var feedbackClass = System.Type.GetType($"{typeName}, MoreMountains.Tools");
                if (feedbackClass == null)
                    feedbackClass = System.Type.GetType($"{typeName}, Assembly-CSharp");
                if (feedbackClass == null)
                    throw new System.Exception($"Feedback type 'MMF_{feedbackType}' not found. Valid examples: CameraShake, Position, Scale, Rotation, Light, Particles, Flash, Flicker, FreezeFrame, AudioSource, TimescaleModifier.");
                if (!typeof(MMF_Feedback).IsAssignableFrom(feedbackClass))
                    throw new System.Exception($"MMF_{feedbackType} is not a valid MMF_Feedback subclass.");

                var feedback = (MMF_Feedback)System.Activator.CreateInstance(feedbackClass);
                feedback.Label = string.IsNullOrEmpty(label) ? feedbackType : label;
                feedback.Active = active;
                feedback.Chance = Mathf.Clamp(chance, 0f, 100f);

                // Auto-assign target for common feedback types
                if (targetObjectName != null)
                {
                    var targetGO = GameObject.Find(targetObjectName);
                    if (targetGO == null)
                        throw new System.Exception($"Target GameObject '{targetObjectName}' not found.");

                    var targetField = feedbackClass.GetField("AnimatePositionTarget") ??
                                     feedbackClass.GetField("AnimateScaleTarget") ??
                                     feedbackClass.GetField("AnimateRotationTarget") ??
                                     feedbackClass.GetField("BoundRenderer") ??
                                     feedbackClass.GetField("BoundLight") ??
                                     feedbackClass.GetField("BoundParticleSystem") ??
                                     feedbackClass.GetField("TargetAudioSource");

                    if (targetField != null)
                    {
                        var targetType = targetField.FieldType;
                        var component = targetGO.GetComponent(targetType);
                        if (component != null)
                            targetField.SetValue(feedback, component);
                    }
                }

                player.FeedbacksList.Add(feedback);
                player.Initialization();
                EditorUtility.SetDirty(player);

                int index = player.FeedbacksList.Count - 1;
                return $"OK: Added MMF_{feedbackType} (label='{feedback.Label}') to '{gameObjectName}' at index {index}. Total feedbacks: {player.FeedbacksList.Count}.";
            });
        }

        [McpPluginTool("feel-play", Title = "Feel / Play / Stop / Control Feedbacks")]
        [Description(@"Controls MMF_Player playback at runtime.
action options:
  Play -- play feedbacks at object position with full intensity.
  Stop -- stop all currently playing feedbacks.
  Pause -- pause playback mid-sequence.
  Resume -- resume a paused sequence.
  Reset -- reset all feedbacks to their initial state.
  Reverse -- play feedbacks in reverse direction.
intensity: optional playback intensity multiplier (default 1.0).
Note: Play/Stop/Pause/Resume only work at runtime (in Play mode). Use this tool for runtime testing.")]
        public string ControlPlayer(
            [Description("Name of the GameObject with MMF_Player.")] string gameObjectName,
            [Description("Action: Play, Stop, Pause, Resume, Reset, or Reverse.")] string action,
            [Description("Intensity multiplier for Play action (default 1.0).")] float intensity = 1.0f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var player = GetPlayer(gameObjectName);

                switch (action.ToLowerInvariant())
                {
                    case "play":
                        player.PlayFeedbacks(player.transform.position, intensity);
                        return $"OK: Playing feedbacks on '{gameObjectName}' at intensity {intensity:F2}.";
                    case "stop":
                        player.StopFeedbacks();
                        return $"OK: Stopped feedbacks on '{gameObjectName}'.";
                    case "pause":
                        player.PauseFeedbacks();
                        return $"OK: Paused feedbacks on '{gameObjectName}'.";
                    case "resume":
                        player.ResumeFeedbacks();
                        return $"OK: Resumed feedbacks on '{gameObjectName}'.";
                    case "reset":
                        player.ResetFeedbacks();
                        return $"OK: Reset feedbacks on '{gameObjectName}'.";
                    case "reverse":
                        player.PlayFeedbacksInReverse(player.transform.position, intensity);
                        return $"OK: Playing feedbacks in reverse on '{gameObjectName}'.";
                    default:
                        throw new System.Exception($"Unknown action '{action}'. Valid: Play, Stop, Pause, Resume, Reset, Reverse.");
                }
            });
        }
    }
}
#endif
