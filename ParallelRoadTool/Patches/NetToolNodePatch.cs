﻿using System;
using System.Collections.Generic;
using ColossalFramework;
using CSUtil.Commons;
using HarmonyLib;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Models;
using ParallelRoadTool.Utils;
using ParallelRoadTool.Wrappers;
using UnityEngine;

namespace ParallelRoadTool.Patches;

[HarmonyPatch(typeof(NetTool), "CreateNodeImpl", typeof(NetInfo), typeof(bool), typeof(bool), typeof(NetTool.ControlPoint),
              typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
internal class NetToolNodePatch
{
    #region Fields

    private static readonly Dictionary<ushort, ushort> Nodes = new();

    #endregion

    internal static void Prefix(NetInfo              info,
                                bool                 needMoney,
                                bool                 switchDirection,
                                NetTool.ControlPoint startPoint,
                                NetTool.ControlPoint middlePoint,
                                NetTool.ControlPoint endPoint,
                                out ushort           __state)
    {
        __state = 0;

        // We only run if the mod is set as Active
        if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active)) return;

        try
        {
            // If start direction is not set we manually compute it
            if (startPoint.m_direction == Vector3.zero)
                startPoint.m_direction = (middlePoint.m_position - startPoint.m_position).normalized;

            //if (ControlPointsBuffer.m_size < Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count)
            //{
            //    ControlPointsBuffer.EnsureCapacity(Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count);
            //    NodesBuffer.EnsureCapacity(Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count);
            //}

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

                // Retrieve control points from buffer
                var currentStartPoint = NetToolCameraPatch.ControlPointsBuffer[i][0];
                var currentMiddlePoint = NetToolCameraPatch.ControlPointsBuffer[i][1];
                var currentEndPoint = NetToolCameraPatch.ControlPointsBuffer[i][2];

                //if (currentRoadInfos.IsReversed)
                //{
                //    // Swap control points
                //    // We also need to swap node references if any
                //    var tmpEndpoint = currentStartPoint;
                //    currentStartPoint = currentEndPoint;
                //    currentEndPoint   = tmpEndpoint;

                //    // Reverse direction too
                //    currentStartPoint.m_direction  *= -1;
                //    currentEndPoint.m_direction    *= -1;

                //    // Swap middle point too if it's in the same position as the end one
                //    if (middlePoint.m_position == endPoint.m_position)
                //        currentMiddlePoint = currentEndPoint;
                //}

                // Check if we already have a node in either start or end point
                //if (currentStartPoint.m_node == 0 || currentStartPoint.m_node == currentEndPoint.m_node)
                //{
                //    var nodeAt = NodeUtils.NodeIdAtPosition(currentStartPoint.m_position);
                //    if (nodeAt != 0)
                //    {
                //        currentStartPoint.m_node = nodeAt;

                //        Log._Debug($">>> Set current start point as {currentStartPoint.m_node} with segment {currentStartPoint.m_segment}");
                //    }

                //    nodeAt = NodeUtils.NodeIdAtPosition(currentEndPoint.m_position);
                //    if (nodeAt != 0)
                //    {
                //        currentEndPoint.m_node = nodeAt;

                //        Log._Debug($">>> Set current end point as {currentEndPoint.m_node} with segment {currentStartPoint.m_segment}");
                //    }
                //}

                //if (currentStartPoint.m_node == 0 && Nodes.TryGetValue(startPoint.m_node, out var newStartNodeId))
                //{
                //    currentStartPoint.m_node = newStartNodeId;

                //    Log._Debug($">>> Set current start point as {currentStartPoint.m_node} from object data");
                //}

                #region Angle Compensation

                // Check if we need to look for an intersection point to move our previously created ending point.
                // This is needed because certain angles will cause the segments to overlap.
                // To fix this we create a parallel line from the original segment, we extend a line from the previous ending point and check if they intersect.
                // IMPORTANT: this is meant for straight roads only!
                if (Singleton<ParallelRoadToolManager>.instance.IsAngleCompensationEnabled && netTool.m_mode == NetTool.Mode.Straight)
                {
                    var newStartNode = NetManager.instance.m_nodes.m_buffer[currentStartPoint.m_node];

                    var previousPoint = newStartNode;
                    var intersection = NodeUtils.FindIntersectionByOffset(newStartNode.m_position, endPoint.m_position, endPoint.m_direction,
                                                                          previousPoint.m_position, -currentStartPoint.m_direction, horizontalOffset,
                                                                          out var intersectionPoint);

                    // If we found an intersection we can draw an helper line showing how much we will have to move the node
                    if (intersection)
                    {
                        // Move the node to the newly found position but keep y from the offset
                        intersectionPoint.y
                            += startPoint.m_elevation; // startNetNode.m_position.Offset(startDirection, horizontalOffset, verticalOffset, invert).y;
                        NetManager.instance.MoveNode(currentStartPoint.m_node, intersectionPoint);
                    }
                }

                #endregion

                // TODO: TMP


                //Log._Debug($">>> Found nodes {currentStartPoint.m_node}, {currentMiddlePoint.m_node}, {currentEndPoint.m_node}");
                //Log._Debug($">>> Original nodes {startPoint.m_node}, {middlePoint.m_node}, {endPoint.m_node}");
                //Log._Debug($">>> Trying with points: [{currentStartPoint.m_position}, {currentMiddlePoint.m_position}, {currentEndPoint.m_position}] - original points were [{startPoint.m_position}, {middlePoint.m_position}, {endPoint.m_position}]");

                if (currentRoadInfos.IsReversed)
                {
                    Log._Debug($">>> Inverting: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");

                    Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic.Invert();

                    Log._Debug($">>> Inverted: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");
                }

                // Draw the offset segment for the current network
                if (!NetToolReversePatch.CreateNodeImpl(netTool, selectedNetInfo, needMoney, switchDirection, currentStartPoint, currentMiddlePoint,
                                                        currentEndPoint))
                {
                    //// Try to find nodes on given positions
                    //FindCloseNode(out currentStartPoint.m_node,  selectedNetInfo, currentStartPoint.m_position);
                    //FindCloseNode(out currentMiddlePoint.m_node, selectedNetInfo, currentMiddlePoint.m_position);
                    //FindCloseNode(out currentEndPoint.m_node,    selectedNetInfo, currentEndPoint.m_position);

                    //NodesBuffer[i]    ??= new ushort[3];
                    //NodesBuffer[i][0] =   currentStartPoint.m_node;
                    //NodesBuffer[i][1] =   currentMiddlePoint.m_node;
                    //NodesBuffer[i][2] =   currentEndPoint.m_node;

                    //ControlPointsBuffer[i]    ??= new NetTool.ControlPoint[3];
                    //ControlPointsBuffer[i][0] =   currentStartPoint;
                    //ControlPointsBuffer[i][1] =   currentMiddlePoint;
                    //ControlPointsBuffer[i][2] =   currentEndPoint;

                    //Log._Debug($">>> Got nodes {currentStartPoint.m_node}, {currentMiddlePoint.m_node}, {currentEndPoint.m_node}");

                    var toolErrors = NetTool.CreateNode(info, currentStartPoint, currentMiddlePoint, currentEndPoint,
                                                        NetTool.m_nodePositionsSimulation, 1000, true, false, true, needMoney, false, switchDirection,
                                                        0, out var node1, out var segment, out var cost, out var productionRate);

                    Log._Debug($">>> Segment creation failed because {toolErrors:g}");
                }
                else
                {
                    __state = NodeUtils.NodeIdAtPosition(currentEndPoint.m_position);

                    //if (!currentRoadInfos.IsReversed) continue;

                    //var startNodeId = NodeUtils.NodeIdAtPosition(currentStartPoint.m_position);
                    //var endNodeId = NodeUtils.NodeIdAtPosition(currentEndPoint.m_position);

                    //Log._Debug($">>> REVERSING FROM {startNodeId} to {endNodeId}");

                    //if (NetToolReversePatch.CreateNodeImpl(netTool, selectedNetInfo, needMoney, true, currentStartPoint with { m_node = startNodeId },
                    //                                       currentMiddlePoint with { m_node = 0 }, currentEndPoint with { m_node = endNodeId }))
                    //    continue;
                    //var toolErrors = NetTool.CreateNode(info, currentStartPoint with { m_node = startNodeId }, currentMiddlePoint with { m_node = 0 },
                    //                                    currentEndPoint with { m_node = endNodeId }, NetTool.m_nodePositionsSimulation, 1000, true,
                    //                                    false, true, needMoney, false, switchDirection, 0, out var node1, out var segment,
                    //                                    out var cost, out var productionRate);

                    //Log._Debug($">>> Segment reverse failed because {toolErrors:g}");
                }

                if (currentRoadInfos.IsReversed)
                {
                    Log._Debug($">>> Inverting: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");

                    Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic.Invert();

                    Log._Debug($">>> Inverted: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");
                }
            }
        }
        catch (Exception e)
        {
            // Log the exception
            Log._DebugOnlyError($"[{nameof(NetToolNodePatch)}.{nameof(Postfix)}] CreateNodeImpl failed.");
            Log.Exception(e);
        }
    }

    internal static void Postfix(NetInfo              info,
                                 bool                 needMoney,
                                 bool                 switchDirection,
                                 NetTool.ControlPoint startPoint,
                                 NetTool.ControlPoint middlePoint,
                                 NetTool.ControlPoint endPoint,
                                 ushort               __state)
    {
        var endNodeId = NodeUtils.NodeIdAtPosition(endPoint.m_position);

        Nodes[endNodeId] = __state;
    }

    [HarmonyPatch]
    private class NetToolReversePatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NetTool), "CreateNodeImpl", typeof(NetInfo), typeof(bool), typeof(bool), typeof(NetTool.ControlPoint),
                      typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
        public static bool CreateNodeImpl(object               instance,
                                          NetInfo              info,
                                          bool                 needMoney,
                                          bool                 switchDirection,
                                          NetTool.ControlPoint startPoint,
                                          NetTool.ControlPoint middlePoint,
                                          NetTool.ControlPoint endPoint)
        {
            // No implementation is required as this will call the original method
            throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
        }
    }

    //public static readonly FastList<NetTool.ControlPoint[]> ControlPointsBuffer = new();
    //public static readonly FastList<ushort[]>               NodesBuffer         = new();
}
