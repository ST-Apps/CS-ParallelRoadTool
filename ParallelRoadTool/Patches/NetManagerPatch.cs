using System;
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

// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local

namespace ParallelRoadTool.Patches
{
    /// <summary>
    ///     Mod's core class, it executes the detour to intercept segment's creation.
    /// </summary>
    [HarmonyPatch(typeof(NetManager),
                  nameof(NetManager.CreateSegment),
                  new[]
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
                  },
                  new[]
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
                  }
                 )]
    public class NetManagerPatch
    {
        // We store nodes from previous iteration so that we know which node to connect to
        private static ushort?[] _endNodeId, _clonedEndNodeId, _startNodeId, _clonedStartNodeId;
        private static bool _isPreviousInvert;

        /// <summary>
        ///     Sets the number of enabled parallel networks
        /// </summary>
        public static int NetworksCount
        {
            set
            {
                // We don't reset nodes arrays if size didn't change, this allows us to snap to previous nodes even after changing offsets.
                if (_endNodeId != null && _endNodeId.Length == value) return;
                _endNodeId = new ushort?[value];
                _clonedEndNodeId = new ushort?[value];
                _startNodeId = new ushort?[value];
                _clonedStartNodeId = new ushort?[value];
            }
        }

        /// <summary>
        ///     Our detour should execute ONLY if the caller is explicitly allowed.
        ///     This prevents unexpected behaviors, but could require some changes in this method to allow compatibility with other
        ///     mods.
        ///     HACK - [ISSUE-10] [ISSUE-18]
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        private static bool IsAllowedCaller(StackTrace st)
        {
            return true;

            // Extract both type and method
            var callerType = st.GetFrame(2)?.GetMethod()?.DeclaringType;
            var callerMethod = st.GetFrame(1)?.GetMethod();

            // They should never be null because stack traces are usually longer than 3 lines, but let's add this check because you'll never know
            if (callerType == null || callerMethod == null)
            {
                Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(IsAllowedCaller)}] {nameof(callerType)} or {nameof(callerMethod)} is null.");
                Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(IsAllowedCaller)}] Stacktrace is\n{st}");

                return false;
            }

            Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(IsAllowedCaller)}] Caller is {callerType.Name}.{callerMethod.Name}");

            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (callerType == typeof(NetTool))

                // We must allow only CreateNode (and eventually patched Harmony methods)
                // This is the compatibility patch for Network Skins 2
                return HarmonyUtils.IsNameMatching(callerMethod.Name, "CreateNode");

            return false;
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
                                    object[] __args)
        {
            try
            {
                segment = (ushort)__args[0];

                if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active))
                {
                    Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] Skipping because mod is not currently active.");
                    return;
                }

                if (Singleton<ParallelRoadToolManager>.instance.IsMouseLongPress
                    && ToolsModifierControl.GetTool<NetTool>().m_mode == NetTool.Mode.Upgrade
                    && startNode == _startNodeId[0]
                    && endNode == _endNodeId[0])
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
                    if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() !=
                        ItemClass.CollisionType.Elevated)
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
                    if (!invert && _endNodeId[i].HasValue && _endNodeId[i].Value == startNode)
                    {
                        newStartNodeId = _clonedEndNodeId[i].Value;

                        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [START] Using old node from previous iteration {_clonedEndNodeId[i].Value} instead of the given one {startNode}");
                        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [START] Start node {startNetNode.m_position} becomes {NetManager.instance.m_nodes.m_buffer[newStartNodeId].m_position}");
                    }
                    else if (!invert && _isPreviousInvert && _startNodeId[i].HasValue &&
                             _startNodeId[i].Value == startNode)
                    {
                        newStartNodeId = _clonedStartNodeId[i].Value;

                        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [START] Using old node from previous iteration {_clonedStartNodeId[i].Value} instead of the given one {startNode}");
                        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [START] Start node{startNetNode.m_position} becomes {NetManager.instance.m_nodes.m_buffer[newStartNodeId].m_position}");
                    }
                    else
                    {
                        var newStartPosition = startNetNode.m_position.Offset(startDirection, horizontalOffset,
                                                                              verticalOffset, invert);

                        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(Postfix)}] [START] {startNetNode.m_position} --> {newStartPosition} | isLeftHand = {Singleton<ParallelRoadToolManager>.instance.IsLeftHandTraffic} | invert = {invert}  | isSlope = {isSlope}");

                        newStartNodeId = NodeAtPositionOrNew(ref randomizer, info, newStartPosition, verticalOffset);
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

                        newEndNodeId = NodeAtPositionOrNew(ref randomizer, info, newEndPosition, verticalOffset);
                    }

                    // Store current end nodes in case we may need to connect the following segment to them
                    _endNodeId[i] = endNode;
                    _clonedEndNodeId[i] = newEndNodeId;
                    _startNodeId[i] = startNode;
                    _clonedStartNodeId[i] = newStartNodeId;

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
                        __result = NetManagerReversePatch.CreateSegment(NetManager.instance, out segment, ref randomizer, selectedNetInfo, treeInfo,
                                                                        newEndNodeId,
                                                                        newStartNodeId,
                                                                        tempStartDirection, tempEndDirection,
                                                                        Singleton<SimulationManager>.instance.m_currentBuildIndex + 1,
                                                                        Singleton<SimulationManager>.instance.m_currentBuildIndex, invert);
                    }
                    else
                    {
                        // Create the segment between the two cloned nodes
                        __result = NetManagerReversePatch.CreateSegment(NetManager.instance, out segment, ref randomizer, selectedNetInfo, treeInfo,
                                                                        newStartNodeId,
                                                                        newEndNodeId, startDirection, endDirection,
                                                                        Singleton<SimulationManager>.instance.m_currentBuildIndex + 1,
                                                                        Singleton<SimulationManager>.instance.m_currentBuildIndex, invert);
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
            [HarmonyPatch(typeof(NetManager),
                          nameof(NetManager.CreateSegment),
                          new[]
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
                          },
                          new[]
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
                          }
                         )]
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
                throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
            }
        }

        #region Utility

        /// <summary>
        ///     Creates a new node and returns it.
        /// </summary>
        /// <param name="newNodeId"></param>
        /// <param name="randomizer"></param>
        /// <param name="info"></param>
        /// <param name="newNodePosition"></param>
        /// <returns></returns>
        private static void CreateNode(out ushort newNodeId,
                                       ref Randomizer randomizer,
                                       NetInfo info,
                                       Vector3 newNodePosition)
        {
            NetManager.instance.CreateNode(out newNodeId, ref randomizer, info, newNodePosition,
                                           Singleton<SimulationManager>.instance.m_currentBuildIndex + 1);
        }

        /// <summary>
        ///     Tries to find an already existing node at the given position, if there aren't we create a new one.
        /// </summary>
        /// <param name="randomizer"></param>
        /// <param name="info"></param>
        /// <param name="newNodePosition"></param>
        /// <param name="verticalOffset"></param>
        /// <returns></returns>
        private static ushort NodeAtPositionOrNew(ref Randomizer randomizer,
                                                  NetInfo info,
                                                  Vector3 newNodePosition,
                                                  float verticalOffset)
        {
            var netManager = Singleton<NetManager>.instance;

            // This should be the best possible value for snapping
            var maxDistance = info.m_halfWidth;

            Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPositionOrNew)}] Trying to find an existing node at position {newNodePosition} (+- {verticalOffset}) with maxDistance = {maxDistance}");

            if (Singleton<ParallelRoadToolManager>.instance.IsSnappingEnabled &&
                (PathManager.FindPathPosition(newNodePosition, info.m_class.m_service,
                                              NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All, true, false,
                                              maxDistance, out var posA, out var posB, out _, out _) ||
                 PathManager.FindPathPosition(
                                              new Vector3(newNodePosition.x, newNodePosition.y - verticalOffset, newNodePosition.z),
                                              info.m_class.m_service, NetInfo.LaneType.All, VehicleInfo.VehicleType.All,
                                              VehicleInfo.VehicleCategory.All, true, false, maxDistance, out posA, out posB, out _,
                                              out _) ||
                 PathManager.FindPathPosition(
                                              new Vector3(newNodePosition.x, newNodePosition.y + verticalOffset, newNodePosition.z),
                                              info.m_class.m_service, NetInfo.LaneType.All, VehicleInfo.VehicleType.All,
                                              VehicleInfo.VehicleCategory.All, true, false, maxDistance, out posA, out posB, out _,
                                              out _)
                )
               )
            {
                Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPositionOrNew)}] FindPathPosition worked with posA.segment = {posA.m_segment} and posB.segment = {posB.m_segment}");

                if (posA.m_segment != 0)
                {
                    var startNodeId = netManager.m_segments.m_buffer[posA.m_segment].m_startNode;
                    var endNodeId = netManager.m_segments.m_buffer[posA.m_segment].m_endNode;

                    var startNode = netManager.m_nodes.m_buffer[startNodeId];
                    var endNode = netManager.m_nodes.m_buffer[endNodeId];

                    Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPositionOrNew)}] posA.segment is not 0, we got two nodes: {startNodeId} [{startNode.m_position}] and {endNodeId} [{endNode.m_position}]");

                    // Get node closer to current position
                    if (startNodeId != 0 && endNodeId != 0)
                        return (newNodePosition - startNode.m_position).sqrMagnitude <
                               (newNodePosition - endNode.m_position).sqrMagnitude
                                   ? startNodeId
                                   : endNodeId;

                    // endNode was not found, return startNode
                    if (startNodeId != 0) return startNodeId;

                    // startNode was not found, return endNode
                    if (endNodeId != 0) return endNodeId;
                }
            }

            Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPositionOrNew)}] No nodes has been found for position {newNodePosition}, creating a new one.");

            // Both startNode and endNode were not found, we need to create a new one
            CreateNode(out var newNodeId, ref randomizer, info, newNodePosition);
            return newNodeId;
        }

        #endregion
    }
}
