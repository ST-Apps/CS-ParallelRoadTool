﻿using System;
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

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming

namespace ParallelRoadTool.Patches;

[HarmonyPatch(typeof(NetTool), nameof(NetTool.RenderOverlay), typeof(RenderManager.CameraInfo), typeof(NetInfo), typeof(Color),
              typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
internal static class NetToolCameraPatch
{
    #region Fields

    public static readonly FastList<NetTool.ControlPoint[]> ControlPointsBuffer = new();

    #endregion

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
    private static void Postfix(RenderManager.CameraInfo cameraInfo,
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

            // Render only if we have a clear direction, otherwise results will look messy
            if (endPoint.m_direction == startPoint.m_direction)
                return;

            // If start direction is not set we manually compute it
            if (startPoint.m_direction == Vector3.zero)
                startPoint.m_direction = middlePoint.m_position - startPoint.m_position;

            if (ControlPointsBuffer.m_size < Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count)
                ControlPointsBuffer.EnsureCapacity(Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count);

            // Get NetTool instance
            var netTool = ToolsModifierControl.GetTool<NetTool>();

            for (var i = 0; i < Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count; i++)
            {
                var currentRoadInfos = Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes[i];

                // Horizontal offset must be negated to appear on the correct side of the original segment
                var horizontalOffset = currentRoadInfos.HorizontalOffset * (Singleton<ParallelRoadToolManager>.instance.IsLeftHandTraffic ? 1 : -1);
                var verticalOffset = currentRoadInfos.VerticalOffset;

                // If the user didn't select a NetInfo we'll use the one he's using for the main road                
                var selectedNetInfo = info.GetNetInfoWithElevation(currentRoadInfos.NetInfo ?? info, out _);

                // If the user is using a vertical offset we try getting the relative elevated net info and use it
                if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() != ItemClass.CollisionType.Elevated)
                    selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).elevated ?? selectedNetInfo;

                // Generate offset points for the current network
                ControlPointUtils.GenerateOffsetControlPoints(startPoint, middlePoint, endPoint, horizontalOffset, verticalOffset, out var currentStartPoint,
                                                              out var currentMiddlePoint, out var currentEndPoint);

#if DEBUG
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
                    RenderManager.instance.OverlayEffect.DrawSegment(cameraInfo, Color.red, middlePointSegment, currentMiddlePointSegment,
                                                                     info.m_halfWidth, 1, 1, 1800, true, true);
                }
#endif

                if (Singleton<ParallelRoadToolManager>.instance.IsAngleCompensationEnabled)
                {
                    // Check if we need to look for an intersection point to move our previously created ending point.
                    // This is needed because certain angles will cause the segments to overlap.
                    // To fix this we create a parallel line from the original segment, we extend a line from the previous ending point and check if they intersect.
                    // IMPORTANT: this is meant for straight roads only!
                    var previousEndPointNullable = NetManagerPatch.PreviousNode(i,   false, true);
                    var previousStartPointNullable = NetManagerPatch.PreviousNode(i, true,  true);
                    if (netTool.m_mode == NetTool.Mode.Straight && previousEndPointNullable.HasValue && previousStartPointNullable.HasValue)
                    {
                        // We can now extract the previously created ending point
                        var previousEndPoint = previousEndPointNullable.Value;
                        var previousStartPoint = previousStartPointNullable.Value;

                        // Get the closest one between start and end
                        var previousEndPointDistance = Vector3.Distance(previousEndPoint.m_position,     currentStartPoint.m_position);
                        var previousStartPointDistance = Vector3.Distance(previousStartPoint.m_position, currentStartPoint.m_position);
                        var previousPoint = previousEndPoint;

                        if (previousStartPointDistance < previousEndPointDistance)
                            previousPoint = previousStartPoint;
                        var intersection = NodeUtils.FindIntersectionByOffset(currentStartPoint.m_position, endPoint.m_position, endPoint.m_direction,
                                                                              previousPoint.m_position, -NetManagerPatch.PreviousEndDirection(i),
                                                                              horizontalOffset, out var intersectionPoint, cameraInfo);

                        // If we found an intersection we can draw an helper line showing how much we will have to move the node
                        if (intersection)
                        {
                            // Set our current point to the intersection point
                            currentStartPoint.m_position = intersectionPoint;

                            // Create a segment between the previous ending point and the intersection
                            var intersectionSegment = new Segment3(previousPoint.m_position, intersectionPoint);

                            // Render the helper line for the segment
                            RenderManager.instance.OverlayEffect.DrawSegment(RenderManager.instance.CurrentCameraInfo, currentRoadInfos.Color,
                                                                             intersectionSegment, currentRoadInfos.NetInfo.m_halfWidth * 2, 8f, 1,
                                                                             1800, true, true);
                        }
                    }
                }

                // Render the overlay for current offset segment
                NetToolReversePatch.RenderOverlay(netTool, cameraInfo, selectedNetInfo, currentRoadInfos.Color, currentStartPoint, currentMiddlePoint,
                                                  currentEndPoint);

                // Save to buffer
                ControlPointsBuffer[i]    ??= new NetTool.ControlPoint[3];
                ControlPointsBuffer[i][0] =   currentStartPoint;
                ControlPointsBuffer[i][1] =   currentMiddlePoint;
                ControlPointsBuffer[i][2] =   currentEndPoint;

                Log._Debug($">>> Rendering points: [{currentStartPoint.m_position}, {currentMiddlePoint.m_position}, {currentEndPoint.m_position}] - original points were [{startPoint.m_position}, {middlePoint.m_position}, {endPoint.m_position}]");

                // We draw arrows only for one-way networks, just as in game
                if (!selectedNetInfo.IsOneWayOnly()) continue;

                // Draw direction arrow by getting the tangent between starting and ending point
                var bezier = new Bezier3 { a = currentStartPoint.m_position, d = currentEndPoint.m_position };
                NetSegment.CalculateMiddlePoints(bezier.a, currentMiddlePoint.m_direction, bezier.d, -currentEndPoint.m_direction, true, true,
                                                 out bezier.b, out bezier.c);

                // we can now extract both position and direction from the tangent
                var position = bezier.Position(0.5f);
                var direction = bezier.Tangent(0.5f);

                // Direction however will be oriented towards the middle point, so we need to rotate it by -90°
                direction.y = 0;
                direction   = Quaternion.Euler(0, -90, 0) * direction.normalized;

                // We can finally draw the arrow
                // TODO: can we use middle point's position and direction?
                NetToolReversePatch.RenderRoadAccessArrow(netTool, cameraInfo, Color.white, position, direction, currentRoadInfos.IsReversed);
            }
        }
        catch (Exception e)
        {
            // Log the exception
            Log._DebugOnlyError($"[{nameof(NetToolCameraPatch)}.{nameof(Postfix)}] CreateSegment failed.");
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