using System;
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

//[HarmonyPatch(typeof(NetTool), "CreateNodeImpl", typeof(NetInfo), typeof(bool), typeof(bool), typeof(NetTool.ControlPoint),
//              typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
internal class NetToolNodePatch
{
    protected static bool FindNode(out ushort newNodeId, NetInfo info, Vector3 position)
    {
        if (FindCloseNode(out newNodeId, info, position))
            return true;
        return false;
    }

    public static ref NetNode GetNode(ushort nodeId)
    {
        return ref NetManager.instance.m_nodes.m_buffer[nodeId];
    }

    private static bool FindCloseNode(out ushort nodeId, NetInfo info, Vector3 position)
    {
        var gridMinX = MinCell(position.x);
        var gridMinZ = MinCell(position.z);
        var gridMaxX = MaxCell(position.x);
        var gridMaxZ = MaxCell(position.z);
        for (var i = gridMinZ; i <= gridMaxZ; i++)
        {
            for (var j = gridMinX; j <= gridMaxX; j++)
            {
                nodeId = NetManager.instance.m_nodeGrid[i * 270 + j];
                ref var node = ref GetNode(nodeId);

                if (info.m_class == node.Info.m_class && (position - node.m_position).magnitude < 0.5f)
                    return true;
            }
        }

        nodeId = 0;
        return false;
    }

    private static int MinCell(float value)
    {
        return Mathf.Max((int)((value - 16f) / 64f + 135f) - 1, 0);
    }

    private static int MaxCell(float value)
    {
        return Mathf.Min((int)((value + 16f) / 64f + 135f) + 1, 269);
    }

    internal static void Prefix(NetInfo              info,
                                bool                 needMoney,
                                bool                 switchDirection,
                                NetTool.ControlPoint startPoint,
                                NetTool.ControlPoint middlePoint,
                                NetTool.ControlPoint endPoint)
    {
        // We only run if the mod is set as Active
        if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active))
            return;

        // If start direction is not set we manually compute it
        if (startPoint.m_direction == Vector3.zero)
            startPoint.m_direction = middlePoint.m_position - startPoint.m_position;

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

            // Check if we already have a node in either start or end point
            if (currentStartPoint.m_node == 0 && FindNode(out var newNodeId, selectedNetInfo, currentStartPoint.m_position))
                currentStartPoint.m_node = newNodeId;
            if (currentEndPoint.m_node == 0 && FindNode(out newNodeId, selectedNetInfo, currentEndPoint.m_position))
                currentEndPoint.m_node = newNodeId;

            Log._Debug($">>> Found nodes {currentStartPoint.m_node}, {currentMiddlePoint.m_node}, {currentEndPoint.m_node}");
            Log._Debug($">>> Original nodes {startPoint.m_node}, {middlePoint.m_node}, {endPoint.m_node}");

            // Draw the offset segment for the current network
            NetToolReversePatch.CreateNodeImpl(netTool, selectedNetInfo, needMoney, switchDirection, currentStartPoint, currentMiddlePoint,
                                               currentEndPoint);
            Log._Debug($">>> Got nodes {currentStartPoint.m_node}, {currentMiddlePoint.m_node}, {currentEndPoint.m_node}");
            Log._Debug($">>> Trying with points: [{currentStartPoint.m_position}, {currentMiddlePoint.m_position}, {currentEndPoint.m_position}] - original points were [{startPoint.m_position}, {middlePoint.m_position}, {endPoint.m_position}]");

            foreach (var VARIABLE in NetManager.instance.m_tempNodeBuffer)
            {
                Log._Debug($"BUFFER: {VARIABLE}");
            }

            #region Angle Compensation

            // Check if we need to look for an intersection point to move our previously created ending point.
            // This is needed because certain angles will cause the segments to overlap.
            // To fix this we create a parallel line from the original segment, we extend a line from the previous ending point and check if they intersect.
            // IMPORTANT: this is meant for straight roads only!
            if (ToolsModifierControl.GetTool<NetTool>().m_mode == NetTool.Mode.Straight)
            {
                var newStartNode = NetManager.instance.m_nodes.m_buffer[currentStartPoint.m_node];

                //// We can now extract the previously created ending point
                //var previousEndPoint = previousEndPointNullable.Value;
                //var previousStartPoint = previousStartPointNullable.Value;

                //// Get the closest one between start and end
                //var previousEndPointDistance
                //    = Vector3.Distance(previousEndPoint.m_position, newStartNode.m_position);
                //var previousStartPointDistance
                //    = Vector3.Distance(previousStartPoint.m_position, newStartNode.m_position);
                //var previousPoint = previousEndPoint;

                //if (previousStartPointDistance < previousEndPointDistance)
                //    previousPoint = previousStartPoint;

                var previousPoint = newStartNode;
                var intersection
                    = NodeUtils.FindIntersectionByOffset(newStartNode.m_position, endPoint.m_position,
                                                         endPoint.m_direction, previousPoint.m_position,
                                                         -currentStartPoint.m_direction, horizontalOffset,
                                                         out var intersectionPoint);

                // If we found an intersection we can draw an helper line showing how much we will have to move the node
                if (intersection)
                {
                    // Move the node to the newly found position but keep y from the offset
                    intersectionPoint.y = startPoint.m_elevation; // startNetNode.m_position.Offset(startDirection, horizontalOffset, verticalOffset, invert).y;
                    NetManager.instance.MoveNode(currentStartPoint.m_node, intersectionPoint);
                }
            }

            #endregion
        }
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
}
