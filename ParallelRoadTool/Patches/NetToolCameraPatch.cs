using System;
using ColossalFramework;
using ColossalFramework.Math;
using CSUtil.Commons;
using HarmonyLib;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Models;
using ParallelRoadTool.Settings;
using ParallelRoadTool.Utils;
using ParallelRoadTool.Wrappers;
using UnityEngine;
using VectorUtils = ParallelRoadTool.Utils.VectorUtils;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming

// TODO: angle compensation (pair the offset node to any original one and use it to found the angle target)
namespace ParallelRoadTool.Patches;

[HarmonyPatch(typeof(NetTool), nameof(NetTool.RenderOverlay), typeof(RenderManager.CameraInfo), typeof(NetInfo), typeof(Color),
              typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
internal static class NetToolCameraPatch
{
    /// <summary>
    ///     Overlay's core method.
    ///     First we render the base overlay, then we render an overlay for each of the selected roads, shifting them with the
    ///     correct offsets.
    /// </summary>
    /// <param name="cameraInfo"></param>
    /// <param name="info"></param>
    /// <param name="color"></param>
    /// <param name="startPoint"></param>
    /// <param name="middlePoint"></param>
    /// <param name="endPoint"></param>

    // ReSharper disable once UnusedMember.Local
    private static void Prefix(RenderManager.CameraInfo cameraInfo,
                               NetInfo                  info,
                               Color                    color,
                               NetTool.ControlPoint     startPoint,
                               NetTool.ControlPoint     middlePoint,
                               NetTool.ControlPoint     endPoint)
    {
        try
        {
            // We only run if the mod is set as Active
            if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active))
                return;

            // Render only if we have at least two distinct points
            if (startPoint.m_position == endPoint.m_position)
                return;

            // Reset start direction because it feels like it's never right
            startPoint.m_direction = (middlePoint.m_position - startPoint.m_position).normalized;

            // Get NetTool instance
            var netTool = ToolsModifierControl.GetTool<NetTool>();

            // Iterate over selected network and render the overlay for each of them
            for (var i = 0; i < Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count; i++)
            {
                var currentRoadInfos = Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes[i];

                // Horizontal offset must be negated to appear on the correct side of the original segment if we're on left-handed drive
                var horizontalOffset = currentRoadInfos.HorizontalOffset * (Singleton<ParallelRoadToolManager>.instance.IsLeftHandTraffic ? 1 : -1);
                var verticalOffset = currentRoadInfos.VerticalOffset;

                // If the user didn't select a NetInfo we'll use the one he's using for the main road                
                var selectedNetInfo = info.GetNetInfoWithElevation(currentRoadInfos.NetInfo ?? info, out _);

                // If the user is using a vertical offset we try getting the relative elevated net info and use it
                if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() != ItemClass.CollisionType.Elevated)
                    selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).elevated ?? selectedNetInfo;

                // Generate offset points for the current network
                ControlPointUtils.GenerateOffsetControlPoints(startPoint, middlePoint, endPoint, horizontalOffset, verticalOffset, selectedNetInfo,
                                                              netTool.m_mode, out var currentStartPoint, out var currentMiddlePoint,
                                                              out var currentEndPoint);

#if DEBUG

                // TODO: move this to a dedicated class maybe
                if (ModSettings.RenderDebugOverlay)
                {
                    // Middle points
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, middlePoint.m_position, info.m_halfWidth, 1, 1800, true,
                                                                    true);
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.blue, currentMiddlePoint.m_position, info.m_halfWidth, 1, 1800,
                                                                    true, true);

                    // Middle directions
                    var middlePointSegment = new Segment3(middlePoint.m_position,
                                                          middlePoint.m_position + middlePoint.m_direction.RotateXZ(-45).normalized * 100);
                    var currentMiddlePointSegment = new Segment3(currentMiddlePoint.m_position,
                                                                 currentMiddlePoint.m_position +
                                                                 currentMiddlePoint.m_direction.RotateXZ(-45).normalized * 100);
                    RenderManager.instance.OverlayEffect.DrawSegment(cameraInfo, Color.green, middlePointSegment, currentMiddlePointSegment,
                                                                     info.m_halfWidth, 1, 1, 1800, true, true);

                    // Middle intersections
                    var middlePointStartSegment = new Segment3(currentStartPoint.m_position,
                                                               currentStartPoint.m_position + currentStartPoint.m_direction.normalized * 1000);
                    var middlePointEndSegment = new Segment3(currentEndPoint.m_position,
                                                             currentEndPoint.m_position - currentEndPoint.m_direction.normalized * 1000);
                    RenderManager.instance.OverlayEffect.DrawSegment(cameraInfo, Color.white, middlePointStartSegment, info.m_halfWidth, 1, 1, 1800,
                                                                     true, true);
                    RenderManager.instance.OverlayEffect.DrawSegment(cameraInfo, Color.black, middlePointEndSegment, info.m_halfWidth, 1, 1, 1800,
                                                                     true, true);
                }
#endif

                #region Angle Compensation

                // TODO: move to controlpointutils
                // Check if we need to look for an intersection point to move our previously created ending point.
                // This is needed because certain angles will cause the segments to overlap.
                // To fix this we create a parallel line from the original segment, we extend a line from the previous ending point and check if they intersect.
                // IMPORTANT: this is meant for straight roads only!
                if (Singleton<ParallelRoadToolManager>.instance.IsAngleCompensationEnabled && netTool.m_mode == NetTool.Mode.Straight &&
                    ParallelRoadToolManager.NodesBuffer.TryGetValue(startPoint.m_node, out var previousEndPoint))
                {
                    // Skip for angle of 180° or 0
                    var angle = Vector3.Angle(-currentEndPoint.m_direction, previousEndPoint.m_direction);
                    if (Math.Abs(angle - 180f) > 0.1f && angle != 0f)
                    {
                        var intersectionPoint = VectorUtils.Intersection(previousEndPoint.m_position, previousEndPoint.m_direction,
                                                                         currentEndPoint.m_position, currentEndPoint.m_direction);

                        if (intersectionPoint != Vector3.up)
                        {
                            // Set our current point to the intersection point
                            currentStartPoint.m_position = intersectionPoint;

                            // Create a segment between the previous ending point and the intersection
                            var intersectionSegment = new Segment3(previousEndPoint.m_position, intersectionPoint);

                            // Render the helper line for the segment
                            RenderManager.instance.OverlayEffect.DrawSegment(RenderManager.instance.CurrentCameraInfo, currentRoadInfos.Color,
                                                                             intersectionSegment, currentRoadInfos.NetInfo.m_halfWidth * 2, 8f, 1,
                                                                             1800, true, true);
                        }
                    }
                }

                #endregion

                // Check if current node can be created. If not change color to red.
                var currentColor = currentRoadInfos.Color;
                if (!ControlPointUtils.CanCreate(info, currentStartPoint, currentMiddlePoint, currentEndPoint, currentRoadInfos.IsReversed))
                    currentColor = Color.red;

                // Render the overlay for current offset segment
                NetToolReversePatch.RenderOverlay(netTool, cameraInfo, selectedNetInfo, currentColor, currentStartPoint, currentMiddlePoint,
                                                  currentEndPoint);

                // Save to buffer
                // TODO: move to controlpointutils
                Singleton<ParallelRoadToolManager>.instance.PushControlPoints(i, currentStartPoint, currentMiddlePoint, currentEndPoint);

                // We draw arrows only for one-way networks, just as in game
                if (!selectedNetInfo.IsOneWayOnly()) continue;

                // Draw direction arrow by getting the tangent between starting and ending point
                var arrowControlPoint = ControlPointUtils.GenerateMiddlePoint(currentStartPoint, currentEndPoint);
                NetToolReversePatch.RenderRoadAccessArrow(netTool, cameraInfo, Color.white, arrowControlPoint.m_position,
                                                          arrowControlPoint.m_direction, currentRoadInfos.IsReversed);
            }
        }
        catch (Exception e)
        {
            // Log the exception
            Log._DebugOnlyError($"[{nameof(NetToolCameraPatch)}.{nameof(Prefix)}] RenderOverlay failed.");
            Log.Exception(e);
        }
    }

    /// <summary>
    ///     The reverse patch is meant as an easy way to access the original
    ///     <see cref="PlayerNetAI.GetConstructionCost(Vector3,Vector3,float,float)" />
    ///     method.
    /// </summary>
    [HarmonyPatch]
    private class NetToolReversePatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NetTool), nameof(NetTool.RenderOverlay), typeof(RenderManager.CameraInfo), typeof(NetInfo), typeof(Color),
                      typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
        public static void RenderOverlay(object                   instance,
                                         RenderManager.CameraInfo cameraInfo,
                                         NetInfo                  info,
                                         Color                    color,
                                         NetTool.ControlPoint     startPoint,
                                         NetTool.ControlPoint     middlePoint,
                                         NetTool.ControlPoint     endPoint)
        {
            // No implementation is required as this will call the original method
            throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NetTool), "RenderRoadAccessArrow", typeof(RenderManager.CameraInfo), typeof(Color), typeof(Vector3), typeof(Vector3),
                      typeof(bool))]
        public static void RenderRoadAccessArrow(object                   instance,
                                                 RenderManager.CameraInfo cameraInfo,
                                                 Color                    color,
                                                 Vector3                  position,
                                                 Vector3                  xDir,
                                                 bool                     flipped)
        {
            // No implementation is required as this will call the original method
            throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
        }
    }
}
