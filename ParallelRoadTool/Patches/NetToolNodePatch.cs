using System;
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
    internal static void Prefix(NetInfo              info,
                                bool                 needMoney,
                                bool                 switchDirection,
                                NetTool.ControlPoint startPoint,
                                NetTool.ControlPoint middlePoint,
                                NetTool.ControlPoint endPoint,

                                // State contains, for each parallel segment, the pair containing new start and end nodes ids
                                out Dictionary<int, NetTool.ControlPoint[]> __state)
    {
        __state = new Dictionary<int, NetTool.ControlPoint[]>();

        // We only run if the mod is set as Active
        if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active)) return;

        try
        {
            // If start direction is not set we manually compute it
            if (startPoint.m_direction == Vector3.zero)
                startPoint.m_direction = (middlePoint.m_position - startPoint.m_position).normalized;

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
                Singleton<ParallelRoadToolManager>.instance.PullControlPoints(i, out var currentStartPoint, out var currentMiddlePoint,
                                                                              out var currentEndPoint);


                #region Angle Compensation

                // Check if we need to look for an intersection point to move our previously created ending point.
                // This is needed because certain angles will cause the segments to overlap.
                // To fix this we create a parallel line from the original segment, we extend a line from the previous ending point and check if they intersect.
                // IMPORTANT: this is meant for straight roads only!
                if (Singleton<ParallelRoadToolManager>.instance.IsAngleCompensationEnabled && netTool.m_mode == NetTool.Mode.Straight)
                {
                    NetManager.instance.MoveNode(currentStartPoint.m_node, currentStartPoint.m_position);
                }

                #endregion

                // After lots of tries this is what looks like being the easiest option to deal with inverting a network's direction.
                // To invert the current network we temporarily invert traffic direction for the current game.
                // This will force the CreateSegment method to receive true for the invert parameter and thus to create every segment in the opposite direction.
                // This value will be restored once we will be done with all of the segments we need to create.
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
                    var toolErrors = NetTool.CreateNode(info, currentStartPoint, currentMiddlePoint, currentEndPoint,
                                                        NetTool.m_nodePositionsSimulation, 1000, true, false, true, needMoney, false, switchDirection,
                                                        0, out _, out _, out _, out _);

                    Log._Debug($">>> Segment creation failed because {toolErrors:g}");
                }
                else
                {
                    // Creation completed, we store the new ids so that we can match everything later
                    // If nodes are 0 we retrieve them back from their position
                    if (currentStartPoint.m_node == 0)
                        currentStartPoint.m_position.AtPosition(info, out currentStartPoint.m_node, out _);
                    if (currentEndPoint.m_node == 0)
                        currentEndPoint.m_position.AtPosition(info, out currentEndPoint.m_node, out _);

                    // We can now store them in the temporary state that is passed between prefix and postifx
                    __state[i] = new[] { currentStartPoint, currentEndPoint };
                }

                if (!currentRoadInfos.IsReversed) continue;
                Log._Debug($">>> Reverting: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");

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
    }

    internal static void Postfix(NetInfo                                 info,
                                 bool                                    needMoney,
                                 bool                                    switchDirection,
                                 NetTool.ControlPoint                    startPoint,
                                 NetTool.ControlPoint                    middlePoint,
                                 NetTool.ControlPoint                    endPoint,
                                 Dictionary<int, NetTool.ControlPoint[]> __state)
    {
        // We only run if the mod is set as Active
        if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active)) return;

        // Skip if state is not set (e.g. node creation failed in previous step)
        if (__state[0] == null)
            return;

        // If nodes are 0 we retrieve them back from their position
        if (startPoint.m_node == 0)
            startPoint.m_position.AtPosition(info, out startPoint.m_node, out _);
        if (endPoint.m_node == 0)
            endPoint.m_position.AtPosition(info, out endPoint.m_node, out _);

        // Here we will have the ids for the original start and nodes so that we can match them with what we have in our state
        ParallelRoadToolManager.NodesBuffer[startPoint.m_node] = __state[0][0];
        ParallelRoadToolManager.NodesBuffer[endPoint.m_node]   = __state[0][1];

        // We now set matching nodes for other eventual parallel segments we might have
        for (var i = 1; i < Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count; i++)
        {
            if (__state[i] == null) continue;
            ParallelRoadToolManager.NodesBuffer[__state[i - 1][0].m_node] = __state[i][0];
            ParallelRoadToolManager.NodesBuffer[__state[i - 1][1].m_node] = __state[i][1];
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

    //public static readonly FastList<NetTool.ControlPoint[]> ControlPointsBuffer = new();
    //public static readonly FastList<ushort[]>               NodesBuffer         = new();
}
