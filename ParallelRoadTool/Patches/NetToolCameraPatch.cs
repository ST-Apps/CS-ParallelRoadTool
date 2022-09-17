using System;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using ColossalFramework;
using ColossalFramework.Math;
using CSUtil.Commons;
using HarmonyLib;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Models;
using ParallelRoadTool.Wrappers;
using UnityEngine;
using static NetInfo;

namespace ParallelRoadTool.Patches
{
    /// <summary>
    ///     Detour used to hook into the RenderOverlay method for segments.
    /// </summary>
    [HarmonyPatch(
        typeof(NetTool),
        nameof(NetTool.RenderOverlay),
        new[]
        {
            typeof(RenderManager.CameraInfo), typeof(NetInfo), typeof(Color),
            typeof(NetTool.ControlPoint),
            typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint)
        }
    )]
    internal class NetToolCameraPatch
    {
        /// <summary>
        ///     Overlay's core method.
        ///     First we render the base overlay, then we render an overlay for each of the selected roads, shifting them with the
        ///     correct offsets.
        ///     TODO: Probably RenderHelperLines is what we need to fix the look with curves, but detouring it makes Unity crash so
        ///     we have to live with this little issue.
        /// </summary>
        /// <param name="cameraInfo"></param>
        /// <param name="info"></param>
        /// <param name="color"></param>
        /// <param name="startPoint"></param>
        /// <param name="middlePoint"></param>
        /// <param name="endPoint"></param>

        // ReSharper disable once UnusedMember.Local
        static void Postfix(RenderManager.CameraInfo cameraInfo,
                                   NetInfo info,
                                   Color color,
                                   NetTool.ControlPoint startPoint,
                                   NetTool.ControlPoint middlePoint,
                                   NetTool.ControlPoint endPoint)
        {
            try
            {


                if (!ParallelRoadTool.ModStatuses.IsFlagSet(ModStatuses.Active))
                {
                    // We only run if the mod is set as Active
                    return;
                }
                if (endPoint.m_direction == Vector3.zero)
                    return;

                var netTool = ToolsModifierControl.GetTool<NetTool>();

                for (var i = 0; i < Singleton<ParallelRoadTool>.instance.SelectedNetworkTypes.Count; i++)
                {
                    var currentRoadInfos = Singleton<ParallelRoadTool>.instance.SelectedNetworkTypes[i];

                    // Horizontal offset must be negated to appear on the correct side of the original segment
                    var horizontalOffset = currentRoadInfos.HorizontalOffset *
                                           (Singleton<ParallelRoadTool>.instance.IsLeftHandTraffic ? 1 : -1);
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
                                startPoint.m_position.Offset(
                                                             startPoint.m_direction == Vector3.zero
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
                    NetToolReversePatch.RenderOverlay(
                        netTool,
                        cameraInfo, 
                        selectedNetInfo,
                        currentRoadInfos.Color,
                        currentStartPoint,
                        currentMidPoint,
                        currentEndPoint
                    );

                    // TODO: only on one-way roads

                    // Draw direction arrow
                    var bezier = new Bezier3()
                    {
                        a = currentStartPoint.m_position,
                        d = currentEndPoint.m_position
                    };
                    NetSegment.CalculateMiddlePoints(bezier.a, currentMidPoint.m_direction, bezier.d, -currentEndPoint.m_direction, true, true, out bezier.b, out bezier.c);
                    var position = bezier.Position(0.5f);
                    var direction = bezier.Tangent(0.5f);
                    direction.y = 0;
                    direction = Quaternion.Euler(0, -90, 0) * direction.normalized;
                    RenderRoadAccessArrow(cameraInfo, Color.white, position, direction, currentRoadInfos.IsReversed);
                }
            }
            catch (Exception e)
            {
                // Log the exception
                Log._DebugOnlyError($"[{nameof(NetToolCameraPatch)}.{nameof(Postfix)}] CreateSegment failed.");
                Log.Exception(e);
            }
        }

        protected static void RenderRoadAccessArrow(
                          RenderManager.CameraInfo cameraInfo,
                          Color color,
                          Vector3 position,
                          Vector3 xDir,
                          bool flipped)
        {
            GameAreaProperties properties = Singleton<GameAreaManager>.instance.m_properties;
            if (!((UnityEngine.Object)properties != (UnityEngine.Object)null))
                return;
            Vector3 vector3 = new Vector3(xDir.z, 0.0f, -xDir.x);
            float num = 3f;
            Quad3 quad;
            if (!flipped)
            {
                quad.a = position - 8f * xDir - (float)((double)num * 8.0 + 16.0) * vector3;
                quad.b = position - 8f * xDir - num * 8f * vector3;
                quad.c = position + 8f * xDir - num * 8f * vector3;
                quad.d = position + 8f * xDir - (float)((double)num * 8.0 + 16.0) * vector3;
            }
            else
            {
                quad.c = position - 8f * xDir - (float)((double)num * 8.0 + 16.0) * vector3;
                quad.d = position - 8f * xDir - num * 8f * vector3;
                quad.a = position + 8f * xDir - num * 8f * vector3;
                quad.b = position + 8f * xDir - (float)((double)num * 8.0 + 16.0) * vector3;
            }
            ++Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls;
            Singleton<RenderManager>.instance.OverlayEffect.DrawQuad(cameraInfo, properties.m_directionArrow, color, quad, -10f, 1280f, false, true);
        }

    }

    /// <summary>
    /// The reverse patch is meant as an easy way to access the original <see cref="PlayerNetAI.GetConstructionCost"/> method.
    /// </summary>
    [HarmonyPatch]
    internal class NetToolReversePatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(
            typeof(NetTool),
            nameof(NetTool.RenderOverlay),
            new[]
            {
                typeof(RenderManager.CameraInfo), typeof(NetInfo), typeof(Color),
                typeof(NetTool.ControlPoint),
                typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint)
            }
        )]
        public static void RenderOverlay(object instance,
            RenderManager.CameraInfo cameraInfo,
                                   NetInfo info,
                                   Color color,
                                   NetTool.ControlPoint startPoint,
                                   NetTool.ControlPoint middlePoint,
                                   NetTool.ControlPoint endPoint)
        {
            // No implementation is required as this will call the original method
            throw new NotImplementedException("It's a stub");
        }
    }
}
