using ColossalFramework.Math;
using CSUtil.Commons;
using ParallelRoadTool.Extensions;
using UnityEngine;

namespace ParallelRoadTool.Utils;

internal static class ControlPointUtils
{
    /// <summary>
    ///     Given 3 <see cref="NetTool.ControlPoint" /> we generate the corresponding 3 by offsetting the original ones.
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="middlePoint"></param>
    /// <param name="endPoint"></param>
    /// <param name="horizontalOffset"></param>
    /// <param name="verticalOffset"></param>
    /// <param name="selectedNetInfo"></param>
    /// <param name="currentStartPoint"></param>
    /// <param name="currentMiddlePoint"></param>
    /// <param name="currentEndPoint"></param>
    /// <param name="middlePointStartPosition"></param>
    /// <param name="middlePointEndPosition"></param>
    public static void GenerateOffsetControlPoints(NetTool.ControlPoint     startPoint,
                                                   NetTool.ControlPoint     middlePoint,
                                                   NetTool.ControlPoint     endPoint,
                                                   float                    horizontalOffset,
                                                   float                    verticalOffset,
                                                   NetInfo                  selectedNetInfo,
                                                   NetTool.Mode             netMode,
                                                   out NetTool.ControlPoint currentStartPoint,
                                                   out NetTool.ControlPoint currentMiddlePoint,
                                                   out NetTool.ControlPoint currentEndPoint)
    {
        // To offset both starting and ending point we need to get the right direction
        var startDirection = startPoint.m_direction.NormalizeWithOffset(horizontalOffset).RotateXZ();
        var endDirection = endPoint.m_direction.NormalizeWithOffset(horizontalOffset).RotateXZ();

        // Move start and end point to the correct direction
        var currentStartPosition = startPoint.m_position + startDirection + Vector3.up * verticalOffset;
        var currentEndPosition = endPoint.m_position     + endDirection   + Vector3.up * verticalOffset;

        // Finally set offset control points by copying everything but the position
        currentStartPoint = startPoint with
        {
            m_node = 0, m_segment = 0, m_position = currentStartPosition, m_elevation = startPoint.m_elevation + verticalOffset
        };
        currentEndPoint = endPoint with
        {
            m_node = 0, m_segment = 0, m_position = currentEndPosition, m_elevation = endPoint.m_elevation + verticalOffset
        };

        // Now that we have the intersection we can get our actual middle point shifted by horizontalOffset
        var currentMiddlePosition = VectorUtils.Intersection(currentStartPosition, startPoint.m_direction, currentEndPosition, endPoint.m_direction);

        // Finally set the point
        currentMiddlePoint = middlePoint with
        {
            m_node = 0,
            m_segment = 0,
            m_position = currentMiddlePosition,
            m_elevation = (currentStartPoint.m_elevation + currentEndPoint.m_elevation) / 2
        };

        // Check if already have nodes at position and use them
        if (currentStartPoint.m_position.AtPosition(selectedNetInfo, out currentStartPoint.m_node, out currentStartPoint.m_segment))
            Log._Debug($"[{nameof(ControlPointUtils)}.{nameof(GenerateOffsetControlPoints)}] Found a node at {currentStartPoint.m_position} with nodeId={currentStartPoint.m_node} and segmentId={currentStartPoint.m_segment} (start)");

        if (currentEndPoint.m_position.AtPosition(selectedNetInfo, out currentEndPoint.m_node, out currentEndPoint.m_segment))
            Log._Debug($"[{nameof(ControlPointUtils)}.{nameof(GenerateOffsetControlPoints)}] Found a node at {currentEndPoint.m_position} with nodeId={currentEndPoint.m_node} and segmentId={currentEndPoint.m_segment} (end)");
    }

    /// <summary>
    ///     Generates the middle <see cref="NetTool.ControlPoint" /> between two <see cref="NetTool.ControlPoint" /> using a
    ///     <see cref="Bezier3" /> curve and getting its tangent.
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    /// <returns></returns>
    public static NetTool.ControlPoint GenerateMiddlePoint(NetTool.ControlPoint startPoint, NetTool.ControlPoint endPoint)
    {
        var bezier = new Bezier3 { a = startPoint.m_position, d = endPoint.m_position };
        NetSegment.CalculateMiddlePoints(bezier.a, startPoint.m_direction, bezier.d, -endPoint.m_direction, true, true, out bezier.b, out bezier.c);

        // we can now extract both position and direction from the tangent
        var position = bezier.Position(0.5f);
        var direction = bezier.Tangent(0.5f);

        // Direction however will be oriented towards the middle point, so we need to rotate it by -90°
        direction.y = 0;
        direction   = Quaternion.Euler(0, -90, 0) * direction.normalized;

        return new NetTool.ControlPoint { m_direction = direction, m_position = position };
    }

    /// <summary>
    ///     Checks if the segment between the provided <see cref="NetTool.ControlPoint" /> can be created by verifying if the
    ///     result <see cref="ToolBase.ToolErrors" /> is <see cref="ToolBase.ToolErrors.None" />
    /// </summary>
    /// <param name="netInfo"></param>
    /// <param name="currentStartPoint"></param>
    /// <param name="currentMiddlePoint"></param>
    /// <param name="currentEndPoint"></param>
    /// <param name="isReversed"></param>
    /// <returns></returns>
    public static bool CanCreate(NetInfo              netInfo,
                                 NetTool.ControlPoint currentStartPoint,
                                 NetTool.ControlPoint currentMiddlePoint,
                                 NetTool.ControlPoint currentEndPoint,
                                 bool                 isReversed)
    {
        return NetTool.CreateNode(netInfo, currentStartPoint, currentMiddlePoint, currentEndPoint, NetTool.m_nodePositionsSimulation, 1000, true,
                                  false, true, true, false, isReversed, 0, out _, out _, out _, out _) == ToolBase.ToolErrors.None;
    }
}
