using System;
using ColossalFramework;
using ColossalFramework.Math;
using CSUtil.Commons;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Patches;
using ParallelRoadTool.Settings;
using UnityEngine;

namespace ParallelRoadTool.Utils;

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
    /// <param name="info"></param>
    /// <param name="newNodePosition"></param>
    /// <param name="verticalOffset"></param>
    /// <returns></returns>
    public static ushort NodeAtPosition(NetInfo info, Vector3 newNodePosition, float verticalOffset)
    {
        var netManager = Singleton<NetManager>.instance;

        // This should be the best possible value for snapping
        var maxDistance = info.m_halfWidth;

        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPosition)}] Trying to find an existing node at position {newNodePosition} (+- {verticalOffset}) with maxDistance = {maxDistance}");

        // Look for nodes nearby
        if (!Singleton<ParallelRoadToolManager>.instance.IsSnappingEnabled ||
            (!PathManager.FindPathPosition(newNodePosition, info.m_class.m_service, NetInfo.LaneType.All, VehicleInfo.VehicleType.All,
                                           VehicleInfo.VehicleCategory.All, true, false, maxDistance, out var posA, out var posB, out _, out _) &&
             !PathManager.FindPathPosition(new Vector3(newNodePosition.x, newNodePosition.y - verticalOffset, newNodePosition.z),
                                           info.m_class.m_service, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All,
                                           true, false, maxDistance, out posA, out posB, out _, out _) &&
             !PathManager.FindPathPosition(new Vector3(newNodePosition.x, newNodePosition.y + verticalOffset, newNodePosition.z),
                                           info.m_class.m_service, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All,
                                           true, false, maxDistance, out posA, out posB, out _, out _)))
            return 0;

        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPosition)}] FindPathPosition worked with posA.segment = {posA.m_segment} and posB.segment = {posB.m_segment}");

        if (posA.m_segment == 0) return 0;

        var startNodeId = netManager.m_segments.m_buffer[posA.m_segment].m_startNode;
        var endNodeId = netManager.m_segments.m_buffer[posA.m_segment].m_endNode;

        var startNode = netManager.m_nodes.m_buffer[startNodeId];
        var endNode = netManager.m_nodes.m_buffer[endNodeId];

        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPosition)}] posA.segment is not 0, we got two nodes: {startNodeId} [{startNode.m_position}] and {endNodeId} [{endNode.m_position}]");

        // Get node closer to current position
        if (startNodeId != 0 && endNodeId != 0)
            return (newNodePosition - startNode.m_position).sqrMagnitude < (newNodePosition - endNode.m_position).sqrMagnitude
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
    public static ushort NodeAtPositionOrNew(ref Randomizer randomizer, NetInfo info, Vector3 newNodePosition, float verticalOffset)
    {
        // Check if we can find a node at the given position
        var newNodeId = NodeAtPosition(info, newNodePosition, verticalOffset);
        if (newNodeId != 0) return newNodeId;

        Log._Debug($"[{nameof(NetManagerPatch)}.{nameof(NodeAtPosition)}] No nodes has been found for position {newNodePosition}, creating a new one.");

        // Both startNode and endNode were not found, we need to create a new one
        CreateNode(out newNodeId, ref randomizer, info, newNodePosition);
        return newNodeId;
    }

    /// <summary>
    ///     Finds the intersection between the line that goes from endPosition to startPosition and the one that goes from
    ///     previousPosition along previousDirection.
    ///     Horizontal offset is also taken care of.
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="endPosition"></param>
    /// <param name="direction"></param>
    /// <param name="previousPosition"></param>
    /// <param name="previousDirection"></param>
    /// <param name="horizontalOffset"></param>
    /// <param name="intersectionPoint"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    public static bool FindIntersectionByOffset(Vector3                  startPosition,
                                                Vector3                  endPosition,
                                                Vector3                  direction,
                                                Vector3                  previousPosition,
                                                Vector3                  previousDirection,
                                                float                    horizontalOffset,
                                                out Vector3              intersectionPoint,
                                                RenderManager.CameraInfo camera = null)
    {
        // With 0° 180° angles we can just return previousPosition
        var currentAngle = Vector3.Angle(direction, previousDirection);
        if (currentAngle == 0f || Math.Abs(currentAngle - 180f) < 10)
        {
            intersectionPoint = previousPosition;
            return false;
        }

        // Since ending point's direction will point to starting point ones we need to invert its direction
        var currentEndPointOrientation = -direction.normalized;

        // We now turn the current ending direction by 90° to face the offset direction
        var offsetOrientation = Quaternion.AngleAxis(-90, Vector3.up) * -direction;

        // Given the offset direction we can set two points on that will be used to draw the line.
        // Those points are set by just moving the current ending point at the edge of the screen but still on the parallel lin.
        var offsetSegmentEndPoint = endPosition   + offsetOrientation.normalized * horizontalOffset + currentEndPointOrientation * 1000;
        var offsetSegmentStartPoint = endPosition + offsetOrientation.normalized * horizontalOffset;

        // If the offset start point is different from previous ending point it means we're not connecting to the previous segment.
        // If we're not connecting to the previous segment we can't reuse its data so we must stop here
        // IMPORTANT: curved segments have start and end nodes inverted for some reason
        if (startPosition == previousPosition)
        {
            // These points are created by getting the previous ending point and stretching it to the edge of the map in both directions
            var previousSegmentEndPoint = previousPosition   + previousDirection * 500;
            var previousSegmentStartPoint = previousPosition - previousDirection * 500;

            // We can finally compute the intersection by getting the two lines and checking if the intersect.
            var offsetLine = Line2.XZ(offsetSegmentStartPoint,     offsetSegmentEndPoint);
            var previousLine = Line2.XZ(previousSegmentStartPoint, previousSegmentEndPoint);

            // Intersect returns two vectors but they're not the coordinates of the intersection point.
            // They're just the direction in which to find this intersection.
            var intersection = offsetLine.Intersect(previousLine, out var ix, out var iy);
            intersectionPoint = (offsetSegmentEndPoint - offsetSegmentStartPoint) * ix + offsetSegmentStartPoint;

#if DEBUG
            if (camera != null && ModSettings.RenderDebugOverlay)
            {
                if (intersection)
                    RenderManager.instance.OverlayEffect.DrawCircle(camera, Color.magenta, intersectionPoint, 16, 1, 1800, true, true);

                var offsetSegment = new Segment3(offsetSegmentStartPoint,     offsetSegmentEndPoint);
                var previousSegment = new Segment3(previousSegmentStartPoint, previousSegmentEndPoint);

                RenderManager.instance.OverlayEffect.DrawSegment(RenderManager.instance.CurrentCameraInfo, Color.blue, offsetSegment, 0.1f, 8f, 1,
                                                                 1800, true, true);
                RenderManager.instance.OverlayEffect.DrawSegment(RenderManager.instance.CurrentCameraInfo, Color.green, previousSegment, 0.1f, 8f, 1,
                                                                 1800, true, true);
            }
#endif

            // If we found an intersection we can draw an helper line showing how much we will have to move the node
            if (intersection) return true;
        }

        intersectionPoint = Vector3.zero;
        return false;
    }

    /// <summary>
    ///     Retrieves the <see cref="NetNode" /> with the given id.
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    public static ref NetNode FromId(ushort nodeId)
    {
        return ref NetManager.instance.m_nodes.m_buffer[nodeId];
    }

    public static ushort NodeAtPosition(Vector3 position, NetInfo info = null)
    {
        SearchRange(position, out var rangeRow, out var rangeColumn);
        ushort nodeId = 0;

        for (var i = rangeColumn.x; i <= rangeColumn.z; i++)
        {
            for (var j = rangeRow.x; j <= rangeRow.z; j++)
            {
                nodeId = NetManager.instance.m_nodeGrid[i * 270 + j];
                ref var node = ref FromId(nodeId);

                if ((position - node.m_position).magnitude < 0.5f && (info == null || info.m_class == node.Info.m_class))
                    return nodeId;
            }
        }

        return nodeId;
    }

    private static void SearchRange(Vector3 position, out FixedVector2D rangeRow, out FixedVector2D rangeColumn)
    {
        rangeRow    = new FixedVector2D();
        rangeColumn = new FixedVector2D();

        rangeRow.x = Mathf.Max((int)((position.x - 16f) / 64f + 135f) - 1, 0);
        rangeRow.z = Mathf.Min((int)((position.x + 16f) / 64f + 135f) + 1, 269);

        rangeColumn.x = Mathf.Max((int)((position.z - 16f) / 64f + 135f) - 1, 0);
        rangeColumn.z = Mathf.Min((int)((position.z + 16f) / 64f + 135f) + 1, 269);
    }

    private static int MinCell(float value)
    {
        return Mathf.Max((int)((value - 16f) / 64f + 135f) - 1, 0);
    }

    private static int MaxCell(float value)
    {
        return Mathf.Min((int)((value + 16f) / 64f + 135f) + 1, 269);
    }
}
