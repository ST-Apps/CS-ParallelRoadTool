using System;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using FineRoadTool;
using ParallelRoadTool.Redirection;
using UnityEngine;

namespace ParallelRoadTool.Detours
{
    /// <summary>
    ///     Mod's core class, it executes the detour to intercept segment's creation.
    /// </summary>
    public struct NetManagerDetour
    {
        private static readonly MethodInfo From = typeof(NetManager).GetMethod("CreateSegment",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        private static readonly MethodInfo To =
            typeof(NetManagerDetour).GetMethod("CreateSegment", BindingFlags.NonPublic | BindingFlags.Instance);

        private static RedirectCallsState _state;
        private static bool _deployed;

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
                _endNodeId = new ushort?[value];
                _clonedEndNodeId = new ushort?[value];
                _startNodeId = new ushort?[value];
                _clonedStartNodeId = new ushort?[value];
            }
        }

        public static void Deploy()
        {
            if (_deployed) return;
            _state = RedirectionHelper.RedirectCalls(From, To);
            _deployed = true;

            // Initialize helper structures
            if (_endNodeId == null || _clonedEndNodeId == null || _startNodeId == null ||
                _clonedStartNodeId == null)
                NetworksCount = 1;

            // Initialize RayCast delegate                        
            //_rayCast = (RayCast)Delegate.CreateDelegate(typeof(RayCast), typeof(ToolBase), typeof(ToolBase).GetMethod("RayCast", BindingFlags.NonPublic | BindingFlags.Static));
        }

        public static void Revert()
        {
            if (!_deployed) return;
            RedirectionHelper.RevertRedirect(From, _state);
            _deployed = false;
        }

        #region Utility

        /// <summary>
        /// Creates a new node and returns it.
        /// </summary>
        /// <param name="newNodeId"></param>
        /// <param name="randomizer"></param>
        /// <param name="info"></param>
        /// <param name="newNodePosition"></param>
        /// <returns></returns>
        private static NetNode CreateNode(out ushort newNodeId, ref Randomizer randomizer, NetInfo info, Vector3 newNodePosition)
        {
            NetManager.instance.CreateNode(out newNodeId, ref randomizer, info, newNodePosition,
                Singleton<SimulationManager>.instance.m_currentBuildIndex + 1);

            return NetManager.instance.m_nodes.m_buffer[newNodeId];
        }

        /// <summary>
        /// Tries to find an already existing node at the given position, if there aren't we create a new one.
        /// </summary>
        /// <param name="newNodeId"></param>
        /// <param name="randomizer"></param>
        /// <param name="info"></param>
        /// <param name="newNodePosition"></param>        
        /// <returns></returns>
        private static ushort NodeAtPositionOrNew(ref Randomizer randomizer, NetInfo info, Vector3 newNodePosition)
        {
            var netManager = Singleton<NetManager>.instance;
            
            // This should be the best possible value for snapping
            var maxDistance = info.m_halfWidth;

            DebugUtils.Log($"Trying to find an existing node at position {newNodePosition} with maxDistance = {maxDistance}");            

            if (ParallelRoadTool.Instance.IsSnappingEnabled && 
                PathManager.FindPathPosition(newNodePosition, info.m_class.m_service, info.m_class.m_service, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleType.All, true, false, maxDistance, out var posA, out var posB, out var sqrDistA, out var sqrDistB))
            {

                DebugUtils.Log($"FindPathPosition worked with posA.segment = {posA.m_segment} and posB.segment = {posB.m_segment}");

                if (posA.m_segment != 0)
                {
                    var startNodeId = netManager.m_segments.m_buffer[posA.m_segment].m_startNode;
                    var endNodeId = netManager.m_segments.m_buffer[posA.m_segment].m_endNode;

                    var startNode = netManager.m_nodes.m_buffer[startNodeId];
                    var endNode = netManager.m_nodes.m_buffer[endNodeId];

                    DebugUtils.Log($"posA.segment is not 0, we got two nodes: {startNodeId} [{startNode.m_position}] and {endNodeId} [{endNode.m_position}]");

                    // Get node closer to current position
                    if (startNodeId != 0 && endNodeId != 0)
                    {
                        return (newNodePosition - startNode.m_position).sqrMagnitude < 
                               (newNodePosition - endNode.m_position).sqrMagnitude ? 
                            startNodeId : 
                            endNodeId;
                    }

                    // endNode was not found, return startNode
                    if (startNodeId != 0)
                    {
                        return startNodeId;
                    }

                    // startNode was not found, return endNode
                    if (endNodeId != 0)
                    {
                        return endNodeId;
                    }                    
                }
            }

            DebugUtils.Log($"No nodes has been found for position {newNodePosition}, creating a new one.");

            // Both startNode and endNode were not found, we need to create a new one
            CreateNode(out var newNodeId, ref randomizer, info, newNodePosition);
            return newNodeId;
        }

        /// <summary>
        ///     Given a point, a direction and a distance, we can get the coordinates for a point which is parallel to the given
        ///     one for the given direction.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="direction"></param>
        /// <param name="horizontalDistance"></param>
        /// <param name="verticalDistance"></param>
        /// <param name="isClockwise"></param>
        /// <returns>A <see cref="Vector3" /> with the coordinates generated by offsetting the given point.</returns>
        private static Vector3 Offset(Vector3 point, Vector3 direction, float horizontalDistance,
            float verticalDistance, bool isClockwise = true)
        {
            var offsetPoint = point + horizontalDistance * new Vector3((isClockwise ? 1 : -1) * direction.z,
                                  direction.y,
                                  (isClockwise ? -1 : 1) * direction.x);
            offsetPoint.y = point.y + verticalDistance;

            return offsetPoint;
        }

        /// <summary>
        ///     Returns a destination NetInfo with the same road type (elevated, tunnel etc.) as source one.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        private NetInfo GetNetInfoWithElevation(NetInfo source, NetInfo destination)
        {
            if (destination.m_netAI == null || source.m_netAI == null) return destination;
            var destinationWrapper = new RoadAIWrapper(destination.m_netAI);
            NetInfo result;
            switch (source.m_netAI.GetCollisionType())
            {
                case ItemClass.CollisionType.Undefined:
                case ItemClass.CollisionType.Zoned:
                case ItemClass.CollisionType.Terrain:
                    result = destinationWrapper.info;
                    break;
                case ItemClass.CollisionType.Underground:
                    result = destinationWrapper.tunnel;
                    break;
                case ItemClass.CollisionType.Elevated:
                    result = destinationWrapper.elevated;
                    break;
                default:
                    result = null;
                    break;
            }

            result = result ?? destination;

            DebugUtils.Log(
                $"Checking source.m_netAI.IsUnderground() && destination.m_netAI.SupportUnderground() == {source.m_netAI.IsUnderground()} && {destination.m_netAI.SupportUnderground()}");

            if (source.m_netAI.IsUnderground() && destination.m_netAI.SupportUnderground())
                result = destinationWrapper.tunnel;

            DebugUtils.Log($"Got a {source.m_netAI.GetCollisionType()}, new road is {result.name}");

            return result;
        }

        /// <summary>
        ///     This methods skips our detour by calling the original method from the game, allowing the creation of the needed
        ///     segment.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="randomizer"></param>
        /// <param name="info"></param>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <param name="startDirection"></param>
        /// <param name="endDirection"></param>
        /// <param name="buildIndex"></param>
        /// <param name="modifiedIndex"></param>
        /// <param name="invert"></param>
        /// <returns></returns>
        private static bool CreateSegmentOriginal(out ushort segment, ref Randomizer randomizer, NetInfo info,
            ushort startNode, ushort endNode, Vector3 startDirection, Vector3 endDirection, uint buildIndex,
            uint modifiedIndex, bool invert)
        {
            Revert();

            var result = NetManager.instance.CreateSegment(out segment, ref randomizer, info, startNode, endNode,
                startDirection, endDirection, buildIndex, modifiedIndex, invert);

            Deploy();

            return result;
        }

        #endregion

        /// <summary>
        ///     Mod's core.
        ///     First, we create the segment using game's original code.
        ///     Then we offset the 2 nodes of the segment, based on both direction and curve, so that we can finally create a
        ///     segment between the 2 offset nodes.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="randomizer"></param>
        /// <param name="info"></param>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <param name="startDirection"></param>
        /// <param name="endDirection"></param>
        /// <param name="buildIndex"></param>
        /// <param name="modifiedIndex"></param>
        /// <param name="invert"></param>
        /// <returns></returns>
        private bool CreateSegment(out ushort segment, ref Randomizer randomizer, NetInfo info, ushort startNode,
            ushort endNode, Vector3 startDirection, Vector3 endDirection, uint buildIndex, uint modifiedIndex,
            bool invert)
        {
            DebugUtils.Log($"Creating a segment and {ParallelRoadTool.SelectedRoadTypes.Count} parallel segments");

            // Let's create the segment that the user requested
            var result = CreateSegmentOriginal(out segment, ref randomizer, info, startNode, endNode, startDirection,
                endDirection, buildIndex, modifiedIndex, invert);

            // If we're in upgrade mode we must stop here
            if (ParallelRoadTool.NetTool.m_mode == NetTool.Mode.Upgrade) return result;

            for (var i = 0; i < ParallelRoadTool.SelectedRoadTypes.Count; i++)
            {
                var currentRoadInfos = ParallelRoadTool.SelectedRoadTypes[i];

                var horizontalOffset = currentRoadInfos.HorizontalOffset;
                var verticalOffset = currentRoadInfos.VerticalOffset;
                DebugUtils.Log($"Using offsets: h {horizontalOffset} | v {verticalOffset}");

                // If the user didn't select a NetInfo we'll use the one he's using for the main road                
                var selectedNetInfo = GetNetInfoWithElevation(info, currentRoadInfos.NetInfo ?? info);
                // If the user is using a vertical offset we try getting the relative elevated net info and use it
                if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() !=
                    ItemClass.CollisionType.Elevated)
                    selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).elevated ?? selectedNetInfo;

                var isReversed = currentRoadInfos.IsReversed;

                DebugUtils.Log($"Using netInfo {selectedNetInfo.name} | reversed={isReversed}");

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
                    DebugUtils.Log(
                        $"[START] Using old node from previous iteration {_clonedEndNodeId[i].Value} instead of the given one {startNode}");
                    newStartNodeId = _clonedEndNodeId[i].Value;
                    DebugUtils.Log(
                        $"[START] Start node{startNetNode.m_position} becomes {NetManager.instance.m_nodes.m_buffer[newStartNodeId].m_position}");
                }
                else if (!invert && _isPreviousInvert && _startNodeId[i].HasValue &&
                         _startNodeId[i].Value == startNode)
                {
                    DebugUtils.Log(
                        $"[START] Using old node from previous iteration {_clonedStartNodeId[i].Value} instead of the given one {startNode}");
                    newStartNodeId = _clonedStartNodeId[i].Value;
                    DebugUtils.Log(
                        $"[START] Start node{startNetNode.m_position} becomes {NetManager.instance.m_nodes.m_buffer[newStartNodeId].m_position}");
                }
                else
                {
                    var newStartPosition = Offset(startNetNode.m_position, startDirection, horizontalOffset,
                        verticalOffset, invert);
                    DebugUtils.Log($"[START] {startNetNode.m_position} --> {newStartPosition} | {invert}");
                    newStartNodeId = NodeAtPositionOrNew(ref randomizer, info, newStartPosition);
                }

                // Same thing as startNode, but this time we don't clone if we're in "invert" mode as we may need to connect this ending node with the previous ending one.
                ushort newEndNodeId;
                if (invert && _endNodeId[i].HasValue && _endNodeId[i].Value == endNode)
                {
                    DebugUtils.Log(
                        $"[END] Using old node from previous iteration {_clonedEndNodeId[i].Value} instead of the given one {endNode}");
                    newEndNodeId = _clonedEndNodeId[i].Value;
                    DebugUtils.Log(
                        $"[END] End node{endNetNode.m_position} becomes {NetManager.instance.m_nodes.m_buffer[newEndNodeId].m_position}");
                }
                else
                {
                    var newEndPosition = Offset(endNetNode.m_position, endDirection, horizontalOffset, verticalOffset);
                    DebugUtils.Log($"[END] {endNetNode.m_position} --> {newEndPosition} | {invert}");
                    newEndNodeId = NodeAtPositionOrNew(ref randomizer, info, newEndPosition);
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
                    result = CreateSegmentOriginal(out segment, ref randomizer, selectedNetInfo, newEndNodeId,
                        newStartNodeId,
                        tempStartDirection, tempEndDirection,
                        Singleton<SimulationManager>.instance.m_currentBuildIndex + 1,
                        Singleton<SimulationManager>.instance.m_currentBuildIndex, invert);
                }
                else
                {
                    // Create the segment between the two cloned nodes
                    result = CreateSegmentOriginal(out segment, ref randomizer, selectedNetInfo, newStartNodeId,
                        newEndNodeId, startDirection, endDirection,
                        Singleton<SimulationManager>.instance.m_currentBuildIndex + 1,
                        Singleton<SimulationManager>.instance.m_currentBuildIndex, invert);
                }
            }

            _isPreviousInvert = invert;
            return result;
        }
    }
}