#if HAS_BOINGKIT
#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.BoingKit.Editor
{
    public partial class Tool_BoingKit
    {
        [McpPluginTool("boing-query", Title = "Boing Kit / Query Components")]
        [Description(@"Reports all Boing Kit components on a GameObject and their configuration.
Checks for: BoingBehavior, BoingBones, BoingEffector, BoingReactorField.
Returns detailed settings for each found component.")]
        public string QueryBoingComponents(
            [Description("Name of the GameObject to inspect for Boing Kit components.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = GameObject.Find(gameObjectName);
                if (go == null)
                    throw new Exception($"GameObject '{gameObjectName}' not found.");

                var sb = new StringBuilder();
                sb.AppendLine($"=== Boing Kit Components on '{gameObjectName}' ===");
                bool found = false;

                // BoingEffector
                var effector = GetBoingComponent(go, BoingEffectorType, "BoingEffector");
                if (effector != null)
                {
                    found = true;
                    sb.AppendLine($"\n-- BoingEffector --");
                    sb.AppendLine($"  Radius:               {Get(effector, "Radius")}");
                    sb.AppendLine($"  FullEffectRadiusRatio: {Get(effector, "FullEffectRadiusRatio")}");
                    sb.AppendLine($"  MaxImpulseSpeed:      {Get(effector, "MaxImpulseSpeed")}");
                    sb.AppendLine($"  ContinuousMotion:     {Get(effector, "ContinuousMotion")}");
                    sb.AppendLine($"  MoveDistance:          {Get(effector, "MoveDistance")}");
                    sb.AppendLine($"  LinearImpulse:        {Get(effector, "LinearImpulse")}");
                    sb.AppendLine($"  RotationAngle:        {Get(effector, "RotationAngle")}");
                    sb.AppendLine($"  AngularImpulse:       {Get(effector, "AngularImpulse")}");
                }

                // BoingBehavior
                var behavior = GetBoingComponent(go, BoingBehaviorType, "BoingBehavior");
                if (behavior != null)
                {
                    found = true;
                    sb.AppendLine($"\n-- BoingBehavior --");
                    sb.AppendLine($"  UpdateMode:             {Get(behavior, "UpdateMode")}");
                    sb.AppendLine($"  EnablePositionEffect:   {Get(behavior, "EnablePositionEffect")}");
                    sb.AppendLine($"  EnableRotationEffect:   {Get(behavior, "EnableRotationEffect")}");
                    sb.AppendLine($"  EnableScaleEffect:      {Get(behavior, "EnableScaleEffect")}");
                    sb.AppendLine($"  LockTranslationX:       {Get(behavior, "LockTranslationX")}");
                    sb.AppendLine($"  LockTranslationY:       {Get(behavior, "LockTranslationY")}");
                    sb.AppendLine($"  LockTranslationZ:       {Get(behavior, "LockTranslationZ")}");
                    sb.AppendLine($"  LockRotationX:          {Get(behavior, "LockRotationX")}");
                    sb.AppendLine($"  LockRotationY:          {Get(behavior, "LockRotationY")}");
                    sb.AppendLine($"  LockRotationZ:          {Get(behavior, "LockRotationZ")}");
                }

                // BoingBones
                var bones = GetBoingComponent(go, BoingBonesType, "BoingBones");
                if (bones != null)
                {
                    found = true;
                    sb.AppendLine($"\n-- BoingBones --");
                    var boneChains = Get(bones, "BoneChains");
                    int chainCount = 0;
                    if (boneChains is ICollection col)
                        chainCount = col.Count;
                    else if (boneChains is Array arr)
                        chainCount = arr.Length;
                    sb.AppendLine($"  BoneChains (count):           {chainCount}");
                    sb.AppendLine($"  TwistPropagation:             {Get(bones, "TwistPropagation")}");
                    sb.AppendLine($"  MaxCollisionResolutionSpeed:  {Get(bones, "MaxCollisionResolutionSpeed")}");
                    sb.AppendLine($"  DebugDrawRawBones:            {Get(bones, "DebugDrawRawBones")}");
                    sb.AppendLine($"  DebugDrawTargetBones:         {Get(bones, "DebugDrawTargetBones")}");
                    sb.AppendLine($"  DebugDrawBoingBones:          {Get(bones, "DebugDrawBoingBones")}");
                    sb.AppendLine($"  DebugDrawFinalBones:          {Get(bones, "DebugDrawFinalBones")}");
                    sb.AppendLine($"  DebugDrawColliders:           {Get(bones, "DebugDrawColliders")}");
                }

                // BoingReactorField
                var reactorField = GetBoingComponent(go, BoingReactorFieldType, "BoingReactorField");
                if (reactorField != null)
                {
                    found = true;
                    sb.AppendLine($"\n-- BoingReactorField --");
                    sb.AppendLine($"  HardwareMode:          {Get(reactorField, "HardwareMode")}");
                    sb.AppendLine($"  CellMoveMode:          {Get(reactorField, "CellMoveMode")}");
                    sb.AppendLine($"  CellSize:              {Get(reactorField, "CellSize")}");
                    sb.AppendLine($"  CellsX:                {Get(reactorField, "CellsX")}");
                    sb.AppendLine($"  CellsY:                {Get(reactorField, "CellsY")}");
                    sb.AppendLine($"  CellsZ:                {Get(reactorField, "CellsZ")}");
                    sb.AppendLine($"  FalloffMode:           {Get(reactorField, "FalloffMode")}");
                    sb.AppendLine($"  FalloffRatio:          {Get(reactorField, "FalloffRatio")}");
                    sb.AppendLine($"  EnablePositionEffect:  {Get(reactorField, "EnablePositionEffect")}");
                    sb.AppendLine($"  EnableRotationEffect:  {Get(reactorField, "EnableRotationEffect")}");
                    sb.AppendLine($"  EnablePropagation:     {Get(reactorField, "EnablePropagation")}");
                }

                if (!found)
                    sb.AppendLine("\nNo Boing Kit components found on this GameObject.");

                return sb.ToString();
            });
        }
    }
}
#endif
