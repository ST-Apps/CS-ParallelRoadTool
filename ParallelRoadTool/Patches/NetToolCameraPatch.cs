using System;
using ColossalFramework;
using ColossalFramework.HTTP.Paradox;
using ColossalFramework.Math;
using CSUtil.Commons;
using HarmonyLib;
using Mono.Cecil.Cil;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Models;
using ParallelRoadTool.Utils;
using ParallelRoadTool.Wrappers;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming

namespace ParallelRoadTool.Patches
{
    [HarmonyPatch(typeof(NetTool), nameof(NetTool.RenderOverlay), typeof(RenderManager.CameraInfo), typeof(NetInfo),
                  typeof(Color), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
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

                // Render only if we have a clear direction, otherwise results will look messy
                if (endPoint.m_direction == startPoint.m_direction)
                    return;

                // Get NetTool instance
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
                    if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() != ItemClass.CollisionType.Elevated)
                        selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).elevated ?? selectedNetInfo;

                    var currentStartPoint = new NetTool.ControlPoint
                    {
                        m_direction = startPoint.m_direction,
                        m_elevation = startPoint.m_elevation,
                        m_node = 0,
                        m_outside = startPoint.m_outside,

                        // startPoint may have a (0,0,0) direction, in that case we use the one from the middlePoint which is accurate enough to avoid overlapping starting nodes
                        m_position
                            = startPoint.m_position
                                        .Offset(startPoint.m_direction == Vector3.zero ? middlePoint.m_direction : startPoint.m_direction,
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

                    // TODO: TMP
                    // TODO: creare struttura in PRTManager con i dati degli ultimi segmenti paralleli creati (uno per rete selezionata).
                    // A questo aggiungiamo anche le info generate in questa intersezione.
                    // In questo modo sia questo che NetManagerPatch usano gli stessi dati e possiamo usare questa classe
                    // per pre-generare i punti da creare (senza fare quindi gli stessi calcoli più volte).
                    // Praticamente questa classe diventa l'unica responsabile per la creazione del buffer di punti da creare in NetManagerPatch.
                    // Verificare però se questo discorso vale anche per le curve o solo per le rette.

                    // Check if we need to look for an intersection point to move our previously created ending point.
                    // This is needed because certain angles will cause the segments to overlap.
                    // To fix this we create a parallel line from the original segment, we extend a line from the previous ending point and check if they intersect.
                    // IMPORTANT: this is meant for straight roads only!
                    var previousEndPointNullable = NetManagerPatch.PreviousNode(i, false, true);
                    var previousStartPointNullable = NetManagerPatch.PreviousNode(i, true, true);
                    if (netTool.m_mode == NetTool.Mode.Straight && previousEndPointNullable.HasValue && previousStartPointNullable.HasValue)
                    {
                        // Since ending point's direction will point to starting point ones we need to invert its direction
                        var currentEndPointOrientation = -endPoint.m_direction.normalized;

                        // We now turn the current ending direction by 90° to face the offset direction
                        var offsetOrientation = Quaternion.AngleAxis(-90, Vector3.up) * -endPoint.m_direction;

                        // Given the offset direction we can set two points on that will be used to draw the line.
                        // Those points are set by just moving the current ending point at the edge of the screen but still on the parallel lin.
                        var offsetSegmentEndPoint = endPoint.m_position + offsetOrientation.normalized * horizontalOffset + currentEndPointOrientation * 10000;
                        var offsetSegmentStartPoint = endPoint.m_position + offsetOrientation.normalized * horizontalOffset;

                        // We can now extract the previously created ending point
                        var previousEndPoint = previousEndPointNullable.Value;
                        var previousStartPoint = previousStartPointNullable.Value;

                        // Get the closest one between start and end

                        // If the offset start point is different from previous ending point it means we're not connecting to the previous segment.
                        // If we're not connecting to the previous segment we can't reuse its data so we must stop here
                        // IMPORTANT: curved segments have start and end nodes inverted for some reason
                        if (currentStartPoint.m_position == previousStartPoint.m_position ||
                            currentStartPoint.m_position == previousEndPoint.m_position)
                        {
                            // Just as we did for the node before, we invert the direction to create a line
                            var previousOrientation = -NetManagerPatch.PreviousEndDirection(i);

                            // These points are created by getting the previous ending point and stretching it to the edge of the map in both directions
                            var previousSegmentEndPoint   = previousEndPoint.m_position + previousOrientation * 5000;
                            var previousSegmentStartPoint = previousEndPoint.m_position - previousOrientation * 5000;

                            // We can finally compute the intersection by getting the two lines and checking if the intersect.
                            var offsetLine   = Line2.XZ(offsetSegmentStartPoint,   offsetSegmentEndPoint);
                            var previousLine = Line2.XZ(previousSegmentStartPoint, previousSegmentEndPoint);

                            // Intersect returns two vectors but they're not the coordinates of the intersection point.
                            // They're just the direction in which to find this intersection.
                            var intersection = offsetLine.Intersect(previousLine, out var ix, out var iy);
                            var intersectionPoint
                                = (offsetSegmentEndPoint - offsetSegmentStartPoint) * ix + offsetSegmentStartPoint;

                            // If we found an intersection we can draw an helper line showing how much we will have to move the node
                            if (intersection)
                            {
                                // Set our current point to the intersection point
                                currentStartPoint.m_position = intersectionPoint;

                                // Create a segment between the previous ending point and the intersection
                                var intersectionSegment = new Segment3(previousEndPoint.m_position, intersectionPoint);

                                // Render the helper line for the segment
                                RenderManager.instance.OverlayEffect.DrawSegment(RenderManager.instance.CurrentCameraInfo,
                                                                                 currentRoadInfos.Color, intersectionSegment,
                                                                                 currentRoadInfos.NetInfo.m_halfWidth * 2, 8f, 1,
                                                                                 1800, true, true);
                            }
                        }
                    }

                    // Render parallel segments by shifting the position of the 3 ControlPoint
                    NetToolReversePatch.RenderOverlay(netTool, cameraInfo, selectedNetInfo, currentRoadInfos.Color,
                                                      currentStartPoint, currentMidPoint, currentEndPoint);

                    // We draw arrows only for one-way networks, just as in game
                    if (!selectedNetInfo.IsOneWayOnly()) continue;

                    // Draw direction arrow by getting the tangent between starting and ending point
                    var bezier = new Bezier3
                    {
                        a = currentStartPoint.m_position,
                        d = currentEndPoint.m_position
                    };
                    NetSegment.CalculateMiddlePoints(bezier.a, currentMidPoint.m_direction, bezier.d,
                                                     -currentEndPoint.m_direction, true, true, out bezier.b, out bezier.c);

                    // we can now extract both position and direction from the tangent
                    var position = bezier.Position(0.5f);
                    var direction = bezier.Tangent(0.5f);

                    // Direction however will be oriented towards the middle point, so we need to rotate it by -90°
                    direction.y = 0;
                    direction = Quaternion.Euler(0, -90, 0) * direction.normalized;

                    // We can finally draw the arrow
                    NetToolReversePatch.RenderRoadAccessArrow(netTool, cameraInfo, Color.white, position, direction,
                                                              currentRoadInfos.IsReversed);
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
            [HarmonyPatch(typeof(NetTool), nameof(NetTool.RenderOverlay), typeof(RenderManager.CameraInfo), typeof(NetInfo),
                          typeof(Color), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint),
                          typeof(NetTool.ControlPoint))]
            public static void RenderOverlay(object instance,
                                             RenderManager.CameraInfo cameraInfo,
                                             NetInfo info,
                                             Color color,
                                             NetTool.ControlPoint startPoint,
                                             NetTool.ControlPoint middlePoint,
                                             NetTool.ControlPoint endPoint)
            {
                // No implementation is required as this will call the original method
                throw new
                    NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
            }

            [HarmonyReversePatch]
            [HarmonyPatch(typeof(NetTool), "RenderRoadAccessArrow", typeof(RenderManager.CameraInfo), typeof(Color),
                          typeof(Vector3), typeof(Vector3), typeof(bool))]
            public static void RenderRoadAccessArrow(object instance,
                                                     RenderManager.CameraInfo cameraInfo,
                                                     Color color,
                                                     Vector3 position,
                                                     Vector3 xDir,
                                                     bool flipped)
            {
                // No implementation is required as this will call the original method
                throw new
                    NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
            }
        }
    }
}
