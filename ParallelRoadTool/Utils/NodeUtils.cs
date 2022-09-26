using ColossalFramework;
using ColossalFramework.Math;
using CSUtil.Commons;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Patches;
using UnityEngine;

namespace ParallelRoadTool.Utils
{
    internal class NodeUtils
    {
        /// <summary>
        ///     Creates a new node and returns it.
        /// </summary>
        /// <param name="newNodeId"></param>
        /// <param name="randomizer"></param>
        /// <param name="info"></param>
        /// <param name="newNodePosition"></param>
        /// <returns></returns>
        private static void CreateNode(out ushort newNodeId, ref Randomizer randomizer, NetInfo info, Vector3 newNodePosition)
        {
            NetManager.instance.CreateNode(out newNodeId, ref randomizer, info, newNodePosition,
                                           Singleton<SimulationManager>.instance.m_currentBuildIndex + 1);
        }

        /// <summary>
        ///     Tries to find an already existing node at the given position
        /// </summary>
        /// <param name="randomizer"></param>
        /// <param name="info"></param>
        /// <param name="newNodePosition"></param>
        /// <param name="verticalOffset"></param>
        /// <returns></returns>
        public static ushort NodeAtPosition(NetInfo        info,
                                            Vector3        newNodePosition,
                                            float          verticalOffset)
        {
            var netManager = Singleton<NetManager>.instance;

            // This should be the best possible value for snapping
            var maxDistance = info.m_halfWidth;

            Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPosition)}] Trying to find an existing node at position {newNodePosition} (+- {verticalOffset}) with maxDistance = {maxDistance}");

            // Look for nodes nearby
            if (!Singleton<ParallelRoadToolManager>.instance.IsSnappingEnabled ||
                (!PathManager.FindPathPosition(newNodePosition, info.m_class.m_service, NetInfo.LaneType.All,
                                               VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All, true, false,
                                               maxDistance, out var posA, out var posB, out _, out _) &&
                 !PathManager
                     .FindPathPosition(new Vector3(newNodePosition.x, newNodePosition.y - verticalOffset, newNodePosition.z),
                                       info.m_class.m_service,          NetInfo.LaneType.All, VehicleInfo.VehicleType.All,
                                       VehicleInfo.VehicleCategory.All, true, false, maxDistance, out posA, out posB, out _,
                                       out _) &&
                 !PathManager
                     .FindPathPosition(new Vector3(newNodePosition.x, newNodePosition.y + verticalOffset, newNodePosition.z),
                                       info.m_class.m_service,          NetInfo.LaneType.All, VehicleInfo.VehicleType.All,
                                       VehicleInfo.VehicleCategory.All, true, false, maxDistance, out posA, out posB, out _,
                                       out _))) return 0;

            Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPosition)}] FindPathPosition worked with posA.segment = {posA.m_segment} and posB.segment = {posB.m_segment}");

            if (posA.m_segment == 0) return 0;

            var startNodeId = netManager.m_segments.m_buffer[posA.m_segment].m_startNode;
            var endNodeId   = netManager.m_segments.m_buffer[posA.m_segment].m_endNode;

            var startNode = netManager.m_nodes.m_buffer[startNodeId];
            var endNode   = netManager.m_nodes.m_buffer[endNodeId];

            Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPosition)}] posA.segment is not 0, we got two nodes: {startNodeId} [{startNode.m_position}] and {endNodeId} [{endNode.m_position}]");

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

            // No node found, return 0
            return 0;
        }

        /// <summary>
        ///     Tries to find an already existing node at the given position, if there aren't we create a new one.
        /// </summary>
        /// <param name="randomizer"></param>
        /// <param name="info"></param>
        /// <param name="newNodePosition"></param>
        /// <param name="verticalOffset"></param>
        /// <returns></returns>
        public static ushort NodeAtPositionOrNew(ref Randomizer randomizer,
                                                 NetInfo        info,
                                                 Vector3        newNodePosition,
                                                 float          verticalOffset)
        {
            // Check if we can find a node at the given position
            var newNodeId = NodeAtPosition(info, newNodePosition, verticalOffset);
            if (newNodeId != 0) return newNodeId;

            Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPosition)}] No nodes has been found for position {newNodePosition}, creating a new one.");

            // Both startNode and endNode were not found, we need to create a new one
            CreateNode(out newNodeId, ref randomizer, info, newNodePosition);
            return newNodeId;
        }
    }
}
