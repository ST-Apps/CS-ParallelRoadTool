using System;
using System.Collections.Generic;
using System.Diagnostics;
using ColossalFramework;
using ColossalFramework.Math;
using CSUtil.Commons;
using HarmonyLib;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Models;
using ParallelRoadTool.Utils;
using ParallelRoadTool.Wrappers;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming

namespace ParallelRoadTool.Patches
{
    [HarmonyPatch(typeof(NetManager), nameof(NetManager.CreateSegment), new[]
    {
        typeof(ushort),
        typeof(Randomizer),
        typeof(NetInfo),
        typeof(TreeInfo),
        typeof(ushort),
        typeof(ushort),
        typeof(Vector3),
        typeof(Vector3),
        typeof(uint),
        typeof(uint),
        typeof(bool)
    }, new[]
    {
        ArgumentType.Out,
        ArgumentType.Ref,
        ArgumentType.Normal,
        ArgumentType.Normal,
        ArgumentType.Normal,
        ArgumentType.Normal,
        ArgumentType.Normal,
        ArgumentType.Normal,
        ArgumentType.Normal,
        ArgumentType.Normal,
        ArgumentType.Normal
    })]
    internal static class NetManagerPatch
    {
        #region Fields

        // We store nodes from previous iteration so that we know which node to connect to
        private static ushort?[] _endNodeId, _clonedEndNodeId, _startNodeId, _clonedStartNodeId;
        private static bool _isPreviousInvert;
        private static Vector3[] _endNodeDirection;

        #endregion

        #region Properties

        /// <summary>
        ///     Sets the number of enabled parallel networks
        /// </summary>
        public static int NetworksCount
        {
            get => _endNodeId.Length;

            set
            {
                // We don't reset nodes arrays if size didn't change, this allows us to snap to previous nodes even after changing offsets.
                if (_endNodeId != null && _endNodeId.Length == value) return;

                _endNodeId = new ushort?[value];
                _clonedEndNodeId = new ushort?[value];
                _startNodeId = new ushort?[value];
                _clonedStartNodeId = new ushort?[value];
                _endNodeDirection = new Vector3[value];
            }
        }

        #endregion

        /// <summary>
        ///     Our detour should execute ONLY if the caller is explicitly allowed.
        ///     This prevents unexpected behaviors, but could require some changes in this method to allow compatibility with other
        ///     mods.
        ///     HACK - [ISSUE-10] [ISSUE-18]
        ///     TODO: things changed with Harmony, is this still necessary?
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        private static bool IsAllowedCaller(StackTrace st)
        {
            return true;

            //// Extract both type and method
            //var callerType = st.GetFrame(2)?.GetMethod()?.DeclaringType;
            //var callerMethod = st.GetFrame(1)?.GetMethod();

            //// They should never be null because stack traces are usually longer than 3 lines, but let's add this check because you'll never know
            //if (callerType == null || callerMethod == null)
            //{
            //    Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(IsAllowedCaller)}] {nameof(callerType)} or {nameof(callerMethod)} is null.");
            //    Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(IsAllowedCaller)}] Stacktrace is\n{st}");

            //    return false;
            //}

            //Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(IsAllowedCaller)}] Caller is {callerType.Name}.{callerMethod.Name}");

            //// ReSharper disable once ConvertIfStatementToReturnStatement
            //if (callerType == typeof(NetTool))

            //    // We must allow only CreateNode (and eventually patched Harmony methods)
            //    // This is the compatibility patch for Network Skins 2
            //    return HarmonyUtils.IsNameMatching(callerMethod.Name, "CreateNode");

            //return false;
        }

        /// <summary>
        ///     Mod's core.
        ///     First, we create the segment using game's original code.
        ///     Then we offset the 2 nodes of the segment, based on both direction and curve, so that we can finally create a
        ///     segment between the 2 offset nodes.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="randomizer"></param>
        /// <param name="info"></param>
        /// <param name="treeInfo"></param>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <param name="startDirection"></param>
        /// <param name="endDirection"></param>
        /// <param name="buildIndex"></param>
        /// <param name="modifiedIndex"></param>
        /// <param name="invert"></param>
        /// <param name="__result"></param>
        /// <param name="__args"></param>
        /// <returns></returns>
        private static void Postfix(out ushort segment,
                                    ref Randomizer randomizer,
                                    NetInfo info,
                                    TreeInfo treeInfo,
                                    ushort startNode,
                                    ushort endNode,
                                    Vector3 startDirection,
                                    Vector3 endDirection,
                                    uint buildIndex,
                                    uint modifiedIndex,
                                    bool invert,
                                    ref bool __result,
                                    IList<object> __args)
        {
            try
            {
                // Dummy assignment in case we don't need to move further
                segment = (ushort)__args[0];

                if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active))
                {
                    Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] Skipping because mod is not currently active.");
                    return;
                }

                if (Singleton<ParallelRoadToolManager>.instance.IsMouseLongPress &&
                    ToolsModifierControl.GetTool<NetTool>().m_mode == NetTool.Mode.Upgrade && startNode == _startNodeId[0] &&
                    endNode == _endNodeId[0])
                {
                    // HACK - [ISSUE-84] Prevent executing multiple times when we have a long mouse press on the very same segment
                    Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] Skipping because mouse has not been released yet from the previous upgrade.");

                    return;
                }

                // HACK - [ISSUE-10] [ISSUE-18] Check if we've been called by an allowed caller, otherwise we can stop here
                if (!IsAllowedCaller(new StackTrace(false)))
                {
                    Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] Skipped because caller is not allowed.");

                    return;
                }

                Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] Adding {Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count} parallel segments");

                if (Singleton<ParallelRoadToolManager>.instance.IsLeftHandTraffic)
                    _isPreviousInvert = invert;

                // If we're in upgrade mode we must stop here
                // HACK - [ISSUE 25] Enabling tool during upgrade mode so that we can add to existing roads
                var isUpgradeActive = false;
                var upgradeInvert = false;
                if (ToolsModifierControl.GetTool<NetTool>().m_mode == NetTool.Mode.Upgrade)
                {
                    isUpgradeActive = true;
                    upgradeInvert = invert;

                    // ReSharper disable CompareOfFloatsByEqualityOperator

                    if (startDirection.x == endDirection.x && startDirection.y == endDirection.y)
                        ToolsModifierControl.GetTool<NetTool>().m_mode = NetTool.Mode.Straight;
                    else
                        ToolsModifierControl.GetTool<NetTool>().m_mode = NetTool.Mode.Curved;

                    // ReSharper restore CompareOfFloatsByEqualityOperator
                }

                // True if we have a slope that is going down from start to end node
                var isEnteringSlope = NetManager.instance.m_nodes.m_buffer[invert ? startNode : endNode].m_elevation >
                                      NetManager.instance.m_nodes.m_buffer[invert ? endNode : startNode].m_elevation;

                for (var i = 0; i < Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count; i++)
                {
                    var currentRoadInfos = Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes[i];

                    var horizontalOffset = currentRoadInfos.HorizontalOffset;
                    var verticalOffset = currentRoadInfos.VerticalOffset;

                    Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] Using offsets: h {horizontalOffset} | v {verticalOffset}");

                    // If the user didn't select a NetInfo we'll use the one he's using for the main road                
                    var selectedNetInfo = info.GetNetInfoWithElevation(currentRoadInfos.NetInfo ?? info, out var isSlope);

                    // If the user is using a vertical offset we try getting the relative elevated net info and use it
                    if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() != ItemClass.CollisionType.Elevated)
                        selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).elevated ?? selectedNetInfo;

                    var isReversed = currentRoadInfos.IsReversed;

                    // Left-hand drive means that any condition must be reversed
                    if (Singleton<ParallelRoadToolManager>.instance.IsLeftHandTraffic)
                    {
                        invert = !invert;
                        isReversed = !isReversed;
                        horizontalOffset = -horizontalOffset;
                    }

                    Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] Using netInfo {selectedNetInfo.name} | reversed={isReversed} | invert={invert}");

                    // Get original nodes to clone them
                    var startNetNode = NetManager.instance.m_nodes.m_buffer[startNode];
                    var endNetNode = NetManager.instance.m_nodes.m_buffer[endNode];

                    // Create two clone nodes by offsetting the original ones.
                    // If we're not in "invert" mode (aka final part of a curve) and we already have an ending node with the same id of our starting node, we need to use that so that the segments can be connected.
                    // If the previous segment was in "invert" mode and the current startNode is the same as the previous one, we need to connect them.
                    // If we don't have any previous node matching our starting one, we need to clone startNode as this may be a new segment.
                    ushort newStartNodeId;
                    switch (invert)
                    {
                        case false when _endNodeId[i].HasValue && _endNodeId[i].Value == startNode:
                            // We start exactly on the previous ending node
                            newStartNodeId = _clonedEndNodeId[i].Value;

                            Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [START] Using old node from previous iteration {_clonedEndNodeId[i].Value} instead of the given one {startNode}");
                            Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [START] Start node {startNetNode.m_position} becomes {NetManager.instance.m_nodes.m_buffer[newStartNodeId].m_position}");
                            break;
                        case false when _isPreviousInvert && _startNodeId[i].HasValue && _startNodeId[i].Value == startNode:
                            // We start exactly on the previous starting node
                            newStartNodeId = _clonedStartNodeId[i].Value;

                            Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [START] Using old node from previous iteration {_clonedStartNodeId[i].Value} instead of the given one {startNode}");
                            Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [START] Start node{startNetNode.m_position} becomes {NetManager.instance.m_nodes.m_buffer[newStartNodeId].m_position}");
                            break;
                        default:
                            {
                                // Not a special case, we offset our node and create a new one (or find one at that position)
                                var newStartPosition
                                    = startNetNode.m_position.Offset(startDirection, horizontalOffset, verticalOffset, invert);

                                Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [START] {startNetNode.m_position} --> {newStartPosition} | isLeftHand = {Singleton<ParallelRoadToolManager>.instance.IsLeftHandTraffic} | invert = {invert}  | isSlope = {isSlope}");

                                newStartNodeId = NodeUtils.NodeAtPositionOrNew(ref randomizer, info, newStartPosition, verticalOffset);
                                break;
                            }
                    }

                    // Same thing as startNode, but this time we don't clone if we're in "invert" mode as we may need to connect this ending node with the previous ending one.
                    ushort newEndNodeId;
                    if (invert && _endNodeId[i].HasValue && _endNodeId[i].Value == endNode)
                    {
                        newEndNodeId = _clonedEndNodeId[i].Value;

                        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [END] Using old node from previous iteration {_clonedEndNodeId[i].Value} instead of the given one {endNode}");
                        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [END] End node{endNetNode.m_position} becomes {NetManager.instance.m_nodes.m_buffer[newEndNodeId].m_position}");
                    }
                    else
                    {
                        var newEndPosition = endNetNode.m_position.Offset(endDirection, horizontalOffset, verticalOffset,
                                                                          !(invert && isSlope && isEnteringSlope));

                        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [END] {endNetNode.m_position} --> {newEndPosition} | isEnteringSlope = {isEnteringSlope} | invert = {invert} | isSlope = {isSlope}");

                        newEndNodeId = NodeUtils.NodeAtPositionOrNew(ref randomizer, info, newEndPosition, verticalOffset);
                    }

                    // Store current end nodes in case we may need to connect the following segment to them
                    _endNodeId[i]         = endNode;
                    _clonedEndNodeId[i]   = newEndNodeId;
                    _startNodeId[i]       = startNode;
                    _clonedStartNodeId[i] = newStartNodeId;
                    _endNodeDirection[i]  = endDirection;

                    if (isReversed)
                    {
                        Vector3 tempStartDirection;
                        Vector3 tempEndDirection;
                        if (startDirection == -endDirection)
                        {
                            // Straight segment, we invert both directions
                            tempStartDirection = -startDirection;
                            tempEndDirection = -endDirection;
                        }
                        else
                        {
                            // Curve, we need to swap start and end direction                        
                            tempStartDirection = endDirection;
                            tempEndDirection = startDirection;
                        }

                        // Create the segment between the two cloned nodes, inverting start and end node
                        __result = NetManagerReversePatch.CreateSegment(NetManager.instance, out segment, ref randomizer,
                                                                        selectedNetInfo, treeInfo, newEndNodeId, newStartNodeId,
                                                                        tempStartDirection, tempEndDirection,
                                                                        Singleton<SimulationManager>.instance
                                                                            .m_currentBuildIndex + 1,
                                                                        Singleton<SimulationManager>.instance.m_currentBuildIndex,
                                                                        invert);
                    }
                    else
                    {
                        // Create the segment between the two cloned nodes
                        __result = NetManagerReversePatch.CreateSegment(NetManager.instance, out segment, ref randomizer,
                                                                        selectedNetInfo, treeInfo, newStartNodeId, newEndNodeId,
                                                                        startDirection, endDirection,
                                                                        Singleton<SimulationManager>.instance
                                                                            .m_currentBuildIndex + 1,
                                                                        Singleton<SimulationManager>.instance.m_currentBuildIndex,
                                                                        invert);
                    }

                    // Left-hand drive revert conditions back
                    if (!Singleton<ParallelRoadToolManager>.instance.IsLeftHandTraffic) continue;
                    invert = !invert;

                    // isReversed = !isReversed;
                    _isPreviousInvert = invert;
                }

                _isPreviousInvert = invert;

                // HACK - [ISSUE 25] Enabling tool during upgrade mode so that we can add to existing roads
                if (!isUpgradeActive) return;
                ToolsModifierControl.GetTool<NetTool>().m_mode = NetTool.Mode.Upgrade;
                _isPreviousInvert = upgradeInvert;
            }
            catch (Exception e)
            {
                // Log the exception and return false as we can't recover from this
                Log._DebugOnlyError($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] CreateSegment failed.");
                Log.Exception(e);

                segment = 0;
                __result = false;
            }
        }

        [HarmonyPatch]
        private class NetManagerReversePatch
        {
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(NetManager), nameof(NetManager.CreateSegment), new[]
            {
                typeof(ushort),
                typeof(Randomizer),
                typeof(NetInfo),
                typeof(TreeInfo),
                typeof(ushort),
                typeof(ushort),
                typeof(Vector3),
                typeof(Vector3),
                typeof(uint),
                typeof(uint),
                typeof(bool)
            }, new[]
            {
                ArgumentType.Out,
                ArgumentType.Ref,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal,
                ArgumentType.Normal
            })]
            public static bool CreateSegment(object instance,
                                             out ushort segment,
                                             ref Randomizer randomizer,
                                             NetInfo info,
                                             TreeInfo treeInfo,
                                             ushort startNode,
                                             ushort endNode,
                                             Vector3 startDirection,
                                             Vector3 endDirection,
                                             uint buildIndex,
                                             uint modifiedIndex,
                                             bool invert)
            {
                // No implementation is required as this will call the original method
                throw new
                    NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
            }
        }

        #region Control

        #region Public API

        /// <summary>
        /// Retrieves the <see cref="NetNode"/> with the provided index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isStartingNode">True for starting nodes, false for ending ones.</param>
        /// <param name="isCloned">True for offset nodes, false for original ones.</param>
        /// <returns></returns>
        public static NetNode? PreviousNode(int index, bool isStartingNode = false, bool isCloned = false)
        {
            // Check on the index just in case
            if (index > NetworksCount) return null;

            // Id for the returned node
            var nodeId = isStartingNode ? (isCloned ? _clonedStartNodeId[index] : _startNodeId[index]).GetValueOrDefault(0) : (isCloned ? _clonedEndNodeId[index] : _endNodeId[index]).GetValueOrDefault(0);

            // We couldn't find the node
            if (nodeId == 0) return null;

            // Extract the node
            return NetManager.instance.m_nodes.m_buffer[nodeId];
        }

        /// <summary>
        /// Retrieves the direction for the previous ending node.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Vector3 PreviousEndDirection(int index)
        {
            return _endNodeDirection[index];
        }

        #endregion

        #endregion
    }
}
