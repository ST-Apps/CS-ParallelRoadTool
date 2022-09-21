using System;
using ColossalFramework;
using ColossalFramework.Math;
using CSUtil.Commons;
using HarmonyLib;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Models;
using ParallelRoadTool.Wrappers;
using UnityEngine;

// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local

namespace ParallelRoadTool.Patches
{
    /// <summary>
    ///     Detour used to hook into the RenderOverlay method for segments.
    /// </summary>
    [HarmonyPatch(typeof(NetTool),
                  nameof(NetTool.RenderOverlay), typeof(RenderManager.CameraInfo), typeof(NetInfo), typeof(Color), typeof(NetTool.ControlPoint),
                  typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
    internal class NetToolCameraPatch
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
        private static void Postfix(RenderManager.CameraInfo cameraInfo,
                                    NetInfo info,
                                    Color color,
                                    NetTool.ControlPoint startPoint,
                                    NetTool.ControlPoint middlePoint,
                                    NetTool.ControlPoint endPoint)
        {
            try
            {
                // We only run if the mod is set as Active
                if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active))
                    return;

                if (endPoint.m_direction == Vector3.zero)
                    return;

                var netTool = ToolsModifierControl.GetTool<NetTool>();

                for (var i = 0; i < Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count; i++)
                {
                    var currentRoadInfos = Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes[i];

                    // Horizontal offset must be negated to appear on the correct side of the original segment
                    var horizontalOffset = currentRoadInfos.HorizontalOffset *
                                           (Singleton<ParallelRoadToolManager>.instance.IsLeftHandTraffic ? 1 : -1);
                    var verticalOffset = currentRoadInfos.VerticalOffset;

                    // If the user didn't select a NetInfo we'll use the one he's using for the main road                
                    var selectedNetInfo = info.GetNetInfoWithElevation(currentRoadInfos.NetInfo ?? info, out _);

                    // If the user is using a vertical offset we try getting the relative elevated net info and use it
                    if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() !=
                        ItemClass.CollisionType.Elevated)
                        selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).elevated ?? selectedNetInfo;

                    var currentStartPoint = new NetTool.ControlPoint
                    {
                        m_direction = startPoint.m_direction,
                        m_elevation = startPoint.m_elevation,
                        m_node = 0,
                        m_outside = startPoint.m_outside,

                        // startPoint may have a (0,0,0) direction, in that case we use the one from the middlePoint which is accurate enough to avoid overlapping starting nodes
                        m_position =
                            startPoint.m_position.Offset(startPoint.m_direction == Vector3.zero
                                                             ? middlePoint.m_direction
                                                             : startPoint.m_direction,
                                                         horizontalOffset, verticalOffset),
                        m_segment = 0
                    };

                    var currentMidPoint = new NetTool.ControlPoint
                    {
                        m_direction = middlePoint.m_direction,
                        m_elevation = middlePoint.m_elevation,
                        m_node = 0,
                        m_outside = middlePoint.m_outside,
                        m_position = middlePoint.m_position.Offset(middlePoint.m_direction, horizontalOffset, verticalOffset),
                        m_segment = 0
                    };

                    var currentEndPoint = new NetTool.ControlPoint
                    {
                        m_direction = endPoint.m_direction,
                        m_elevation = endPoint.m_elevation,
                        m_node = 0,
                        m_outside = endPoint.m_outside,
                        m_position = endPoint.m_position.Offset(endPoint.m_direction, horizontalOffset, verticalOffset),
                        m_segment = 0
                    };

                    // Render parallel segments by shifting the position of the 3 ControlPoint
                    NetToolReversePatch.RenderOverlay(netTool,
                                                      cameraInfo,
                                                      selectedNetInfo,
                                                      currentRoadInfos.Color,
                                                      currentStartPoint,
                                                      currentMidPoint,
                                                      currentEndPoint
                                                     );

                    // We draw arrows only for one-way networks, just as in game
                    if (!selectedNetInfo.IsOneWayOnly()) continue;

                    // Draw direction arrow by getting the tangent between starting and ending point
                    var bezier = new Bezier3
                    {
                        a = currentStartPoint.m_position,
                        d = currentEndPoint.m_position
                    };
                    NetSegment.CalculateMiddlePoints(bezier.a, currentMidPoint.m_direction, bezier.d, -currentEndPoint.m_direction, true, true, out bezier.b, out bezier.c);

                    // we can now extract both position and direction from the tangent
                    var position = bezier.Position(0.5f);
                    var direction = bezier.Tangent(0.5f);

                    // Direction however will be oriented towards the middle point, so we need to rotate it by -90°
                    direction.y = 0;
                    direction = Quaternion.Euler(0, -90, 0) * direction.normalized;

                    // We can finally draw the arrow
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
        ///     The reverse patch is meant as an easy way to access the original <see cref="PlayerNetAI.GetConstructionCost" />
        ///     method.
        /// </summary>
        [HarmonyPatch]
        private class NetToolReversePatch
        {
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(NetTool),
                          nameof(NetTool.RenderOverlay), typeof(RenderManager.CameraInfo), typeof(NetInfo), typeof(Color),
                          typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
            public static void RenderOverlay(object instance,
                                             RenderManager.CameraInfo cameraInfo,
                                             NetInfo info,
                                             Color color,
                                             NetTool.ControlPoint startPoint,
                                             NetTool.ControlPoint middlePoint,
                                             NetTool.ControlPoint endPoint)
            {
                // No implementation is required as this will call the original method
                throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
            }

            [HarmonyReversePatch]
            [HarmonyPatch(typeof(NetTool),
                          "RenderRoadAccessArrow", typeof(RenderManager.CameraInfo), typeof(Color), typeof(Vector3),
                          typeof(Vector3), typeof(bool))]
            public static void RenderRoadAccessArrow(object instance,
                                                     RenderManager.CameraInfo cameraInfo,
                                                     Color color,
                                                     Vector3 position,
                                                     Vector3 xDir,
                                                     bool flipped)
            {
                // No implementation is required as this will call the original method
                throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
            }
        }
    }
}
