// <copyright file="NetToolNodePatch.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Patches;

using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using CSUtil.Commons;
using Extensions;
using HarmonyLib;
using Managers;
using Models;
using UnityEngine;
using Wrappers;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(NetTool), "CreateNodeImpl", typeof(NetInfo), typeof(bool), typeof(bool), typeof(NetTool.ControlPoint),
              typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
internal class NetToolNodePatch
{
    internal static void Prefix(NetInfo info,
                                bool needMoney,
                                bool switchDirection,
                                NetTool.ControlPoint startPoint,
                                NetTool.ControlPoint middlePoint,
                                NetTool.ControlPoint endPoint,

                                // State contains, for each parallel segment, the pair containing new start and end nodes ids
                                out Dictionary<int, NetTool.ControlPoint[]> __state)
    {
        __state = new Dictionary<int, NetTool.ControlPoint[]>();

        try
        {
            // We only run if the mod is set as Active
            if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active))
            {
                return;
            }

            // Prevent mod from running if user is just inverting a segment's direction
            if (switchDirection)
            {
                return;
            }

            // If start direction is not set we manually compute it
            if (startPoint.m_direction == Vector3.zero)
            {
                startPoint.m_direction = (middlePoint.m_position - startPoint.m_position).normalized;
            }

            // Get NetTool instance
            var netTool = ToolsModifierControl.GetTool<NetTool>();

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
                Singleton<ParallelRoadToolManager>.instance.PullControlPoints(i, out var currentStartPoint, out var currentMiddlePoint,
                                                                              out var currentEndPoint);

                // The correct position with angle compensation on is computed during overlay phase, so we just move the starting node because it will contain the correct position.
                // IMPORTANT: this is meant for straight roads only!
                if (Singleton<ParallelRoadToolManager>.instance.IsAngleCompensationEnabled && netTool.m_mode == NetTool.Mode.Straight)
                {
                    NetManager.instance.MoveNode(currentStartPoint.m_node, currentStartPoint.m_position);
                }

                // After lots of tries this is what looks like being the easiest option to deal with inverting a network's direction.
                // To invert the current network we temporarily invert traffic direction for the current game.
                // This will force the CreateSegment method to receive true for the invert parameter and thus to create every segment in the opposite direction.
                // This value will be restored once we will be done with all of the segments we need to create.
                if (currentRoadInfos.IsReversed)
                {
                    Log._Debug($"[{nameof(NetToolNodePatch)}.{nameof(Prefix)}] Inverting: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");

                    Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic.Invert();

                    Log._Debug($"[{nameof(NetToolNodePatch)}.{nameof(Prefix)}] Inverted: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");
                }

                // Draw the offset segment for the current network
                if (!NetToolReversePatch.CreateNodeImpl(netTool, selectedNetInfo, false, false, currentStartPoint, currentMiddlePoint,
                                                        currentEndPoint))
                {
                    var toolErrors = NetTool.CreateNode(info, currentStartPoint, currentMiddlePoint, currentEndPoint,
                                                        NetTool.m_nodePositionsSimulation, 1000, true, false, true, needMoney, false, switchDirection,
                                                        0, out _, out _, out _, out _);

                    Log.Error($"[{nameof(NetToolNodePatch)}.{nameof(Prefix)}] Segment creation failed because {toolErrors:g}");
                }
                else
                {
                    // Creation completed, we store the new ids so that we can match everything later
                    // If nodes are 0 we retrieve them back from their position
                    if (currentStartPoint.m_node == 0)
                    {
                        currentStartPoint.m_position.AtPosition(info, out currentStartPoint.m_node, out _);
                    }

                    if (currentEndPoint.m_node == 0)
                    {
                        currentEndPoint.m_position.AtPosition(info, out currentEndPoint.m_node, out _);
                    }

                    // We can now store them in the temporary state that is passed between prefix and postfix
                    __state[i] = new[] { currentStartPoint, currentEndPoint };
                }

                if (!currentRoadInfos.IsReversed)
                {
                    continue;
                }

                Log._Debug($"[{nameof(NetToolNodePatch)}.{nameof(Prefix)}] Reverting: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");

                Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic.Invert();

                Log._Debug($">>> Reverted: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");
            }
        }
        catch (Exception e)
        {
            // Log the exception
            Log._DebugOnlyError($"[{nameof(NetToolNodePatch)}.{nameof(Postfix)}] CreateNodeImpl failed.");
            Log.Exception(e);
        }
        finally
        {
            // We can now clear control points' buffer to get ready for building again
            Singleton<ParallelRoadToolManager>.instance.ClearControlPoints();
        }
    }

    internal static void Postfix(NetInfo info,
                                 bool needMoney,
                                 bool switchDirection,
                                 NetTool.ControlPoint startPoint,
                                 NetTool.ControlPoint middlePoint,
                                 NetTool.ControlPoint endPoint,
                                 Dictionary<int, NetTool.ControlPoint[]> __state)
    {
        // We only run if the mod is set as Active
        if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active))
        {
            return;
        }

        // Skip if state is not set (e.g. node creation failed in previous step)
        if (!__state.TryGetValue(0, out _))
        {
            return;
        }

        // If nodes are 0 we retrieve them back from their position
        if (startPoint.m_node == 0)
        {
            startPoint.m_position.AtPosition(info, out startPoint.m_node, out _);
        }

        if (endPoint.m_node == 0)
        {
            endPoint.m_position.AtPosition(info, out endPoint.m_node, out _);
        }

        // TODO: clear the buffer everytime we start building on Prefix? This should prevent ugly cases where people add new segments after a while
        // Push all the generated ControlPoints for both start and end nodes.
        // They will be used as a reference in order to match and connect nodes later.
        Singleton<ParallelRoadToolManager>.instance.PushGeneratedNodes(startPoint.m_node, __state.Select(s => s.Value[0]).ToArray());
        Singleton<ParallelRoadToolManager>.instance.PushGeneratedNodes(endPoint.m_node, __state.Select(s => s.Value[1]).ToArray());
    }

    [HarmonyPatch]
    private class NetToolReversePatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NetTool), "CreateNodeImpl", typeof(NetInfo), typeof(bool), typeof(bool), typeof(NetTool.ControlPoint),
                      typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
        public static bool CreateNodeImpl(object instance,
                                          NetInfo info,
                                          bool needMoney,
                                          bool switchDirection,
                                          NetTool.ControlPoint startPoint,
                                          NetTool.ControlPoint middlePoint,
                                          NetTool.ControlPoint endPoint)
        {
            // No implementation is required as this will call the original method
            throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
        }
    }
}
