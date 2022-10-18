// <copyright file="NetToolTmpPatch.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Patches;

using System;
using System.Reflection;
using ColossalFramework;
using CSUtil.Commons;
using Extensions;
using HarmonyLib;
using Managers;
using Models;
using UnityEngine;
using Utils;
using Wrappers;
using static ToolBase;

[HarmonyPatch(typeof(NetTool),nameof(NetTool.CreateNode),
              new[]
              {
                  typeof(NetInfo), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint),
                  typeof(FastList<NetTool.NodePosition>), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool),
                  typeof(bool), typeof(ushort), typeof(ushort), typeof(ushort), typeof(int), typeof(int)
              },
              new[]
              {
                  ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
                  ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
                  ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out
              })]
internal class NetToolNodePatch
{

    public static void Prefix(
        NetInfo info,
        NetTool.ControlPoint startPoint,
        NetTool.ControlPoint middlePoint,
        NetTool.ControlPoint endPoint,
        FastList<NetTool.NodePosition> nodeBuffer,
        int maxSegments,
        bool test,
        bool visualize,
        bool autoFix,
        bool needMoney,
        bool invert,
        bool switchDir,
        ushort relocateBuildingID,
        ushort node,
        ushort segment,
        int cost,
        int productionRate)
    {
        // We only run if the mod is set as Active
        if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active))
        {
            return;
        }

        // Prevent mod from running if we're in either test, switch_dir or visualize modes
        if (test || switchDir || visualize)
        {
            return;
        }

        // Get NetTool instance
        var netTool = ToolsModifierControl.GetTool<NetTool>();

        try
        {
            for (var i = 0; i < Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count; i++)
            {
                // Retrieve control points from buffer
                Singleton<ParallelRoadToolManager>.instance.PullControlPoints(i, out var currentStartPoint, out _, out _);

                // The correct position with angle compensation on is computed during overlay phase, so we just move the starting node because it will contain the correct position.
                // IMPORTANT: this is meant for straight roads only!
                if (!Singleton<ParallelRoadToolManager>.instance.IsAngleCompensationEnabled || netTool.m_mode != NetTool.Mode.Straight ||
                    currentStartPoint.m_node == 0)
                {
                    continue;
                }

                // HACK - After moving to this patch from CreateNodeImpl, angle compensation started to change Y position for nodes and this is an unintended behavior.
                // HACK - For this reason we manually set the Y position back by getting it from the current position before moving the node.
                var currentNode = NodeUtils.FromId(currentStartPoint.m_node);
                currentStartPoint.m_position.y = currentNode.m_position.y;

                // We can now move the node to the correct place
                NetManager.instance.MoveNode(currentStartPoint.m_node, currentStartPoint.m_position);
            }
        }
        catch (Exception e)
        {
            // Log the exception
            Log._DebugOnlyError($"[{nameof(NetToolNodePatch)}.{nameof(Prefix)}] CreateNode failed.");
            Log.Exception(e);
        }
    }

    public static void Postfix(
        NetInfo info,
        NetTool.ControlPoint startPoint,
        NetTool.ControlPoint middlePoint,
        NetTool.ControlPoint endPoint,
        FastList<NetTool.NodePosition> nodeBuffer,
        int maxSegments,
        bool test,
        bool visualize,
        bool autoFix,
        bool needMoney,
        bool invert,
        bool switchDir,
        ushort relocateBuildingID,
        ushort node,
        ushort segment,
        int cost,
        int productionRate)
    {
        // We only run if the mod is set as Active
        if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active))
        {
            return;
        }

        // Prevent mod from running if we're in either test, switch_dir or visualize modes
        if (test || switchDir || visualize)
        {
            return;
        }

        try
        {
            // If start direction is not set we manually compute it
            if (startPoint.m_direction == Vector3.zero)
            {
                startPoint.m_direction = (middlePoint.m_position - startPoint.m_position).normalized;
            }

            // Temp arrays to store both starting and ending parallel nodes
            var currentStartPoints = new NetTool.ControlPoint[Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count];
            var currentEndPoints = new NetTool.ControlPoint[Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count];

            for (var i = 0; i < Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count; i++)
            {
                var currentRoadInfos = Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes[i];

                // If the user didn't select a NetInfo we'll use the one he's using for the main road
                var selectedNetInfo = info.GetNetInfoWithElevation(currentRoadInfos.NetInfo ?? info, out _);

                // If the user is using a vertical offset we try getting the relative elevated net info and use it
                if (currentRoadInfos.VerticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() != ItemClass.CollisionType.Elevated)
                {
                    selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).Elevated ?? selectedNetInfo;
                }

                // Retrieve control points from buffer
                Singleton<ParallelRoadToolManager>.instance.PullControlPoints(i, out var currentStartPoint, out var currentMiddlePoint, out var currentEndPoint);

                // Draw the offset segment for the current network
                var toolErrors = NetToolReversePatch.CreateNode(selectedNetInfo, currentStartPoint, currentMiddlePoint, currentEndPoint, nodeBuffer,
                                                                maxSegments, false, false, autoFix, false, currentRoadInfos.IsReversed, false,
                                                                relocateBuildingID, out _, out _, out _, out _);
                if (toolErrors != ToolErrors.None)
                {
                    Log.Error($"[{nameof(NetToolNodePatch)}.{nameof(Postfix)}] Segment creation failed because {toolErrors:g}");
                    return;
                }
                else
                {
                    // Creation completed, we store the new ids so that we can match everything later
                    // Before doing that we need to retrieve node ids for all the start/end nodes because, even after creation, we can receive 0
                    if (currentStartPoint.m_node == 0)
                    {
                        currentStartPoint.m_position.AtPosition(info, out currentStartPoint.m_node, out _);
                    }

                    if (currentEndPoint.m_node == 0)
                    {
                        currentEndPoint.m_position.AtPosition(info, out currentEndPoint.m_node, out _);
                    }

                    if (startPoint.m_node == 0)
                    {
                        startPoint.m_position.AtPosition(info, out startPoint.m_node, out _);
                    }

                    if (endPoint.m_node == 0)
                    {
                        endPoint.m_position.AtPosition(info, out endPoint.m_node, out _);
                    }

                    // Now that we have all the node ids we can store the newly created ids to push them out later
                    currentStartPoints[i] = currentStartPoint;
                    currentEndPoints[i] = currentEndPoint;
                }
            }

            // Push all the generated ControlPoints for both start and end nodes.
            // They will be used as a reference in order to match and connect nodes later.
            Singleton<ParallelRoadToolManager>.instance.PushGeneratedNodes(startPoint.m_node, currentStartPoints);
            Singleton<ParallelRoadToolManager>.instance.PushGeneratedNodes(endPoint.m_node, currentEndPoints);
        }
        catch (Exception e)
        {
            // Log the exception
            Log._DebugOnlyError($"[{nameof(NetToolNodePatch)}.{nameof(Postfix)}] CreateNode failed.");
            Log.Exception(e);
        }
        finally
        {
            // We can now clear control points' buffer to get ready for building again
            Singleton<ParallelRoadToolManager>.instance.ClearControlPoints();
        }
    }
}

[HarmonyPatch]
internal static class NetToolReversePatch
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(NetTool), nameof(NetTool.CreateNode),
                  new[]
                  {
                      typeof(NetInfo), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint),
                      typeof(FastList<NetTool.NodePosition>), typeof(int), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool),
                      typeof(bool), typeof(ushort), typeof(ushort), typeof(ushort), typeof(int), typeof(int)
                  },
                  new[]
                  {
                      ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
                      ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
                      ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out, ArgumentType.Out,
                      ArgumentType.Out
                  })]
    public static ToolErrors CreateNode(
        NetInfo info,
        NetTool.ControlPoint startPoint,
        NetTool.ControlPoint middlePoint,
        NetTool.ControlPoint endPoint,
        FastList<NetTool.NodePosition> nodeBuffer,
        int maxSegments,
        bool test,
        bool visualize,
        bool autoFix,
        bool needMoney,
        bool invert,
        bool switchDir,
        ushort relocateBuildingID,
        out ushort node,
        out ushort segment,
        out int cost,
        out int productionRate)
    {
        // No implementation is required as this will call the original method
        throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
    }
}
