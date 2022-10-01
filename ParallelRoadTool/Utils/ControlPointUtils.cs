using ColossalFramework.Math;
using ParallelRoadTool.Extensions;
using UnityEngine;

namespace ParallelRoadTool.Utils;

internal static class ControlPointUtils
{
    /// <summary>
    ///     Clones and offsets the provided triple of control points.
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="middlePoint"></param>
    /// <param name="endPoint"></param>
    /// <param name="horizontalOffset"></param>
    /// <param name="verticalOffset"></param>
    /// <param name="currentStartPoint"></param>
    /// <param name="currentMiddlePoint"></param>
    /// <param name="currentEndPoint"></param>
    public static void GenerateOffsetControlPoints(NetTool.ControlPoint     startPoint,
                                                   NetTool.ControlPoint     middlePoint,
                                                   NetTool.ControlPoint     endPoint,
                                                   float                    horizontalOffset,
                                                   float                    verticalOffset,
                                                   out NetTool.ControlPoint currentStartPoint,
                                                   out NetTool.ControlPoint currentMiddlePoint,
                                                   out NetTool.ControlPoint currentEndPoint)
    {
        // To offset both starting and ending point we need to get the right direction
        var currentStartDirection = startPoint.m_direction.NormalizeWithOffset(horizontalOffset).RotateXZ();
        var currentEndDirection = endPoint.m_direction.NormalizeWithOffset(horizontalOffset).RotateXZ();

        // Move start and end point to the correct direction
        var currentStartPosition = startPoint.m_position + currentStartDirection;
        var currentEndPosition = endPoint.m_position     + currentEndDirection;

        // Get two intersection points for the pairs (start, mid) and (mid, end)
        var middlePointStartPosition = middlePoint.m_position + currentStartDirection;
        var middlePointEndPosition = middlePoint.m_position   + currentEndDirection;
        var middlePointStartLine = Line2.XZ(currentStartPosition, middlePointStartPosition);
        var middlePointEndLine = Line2.XZ(currentEndPosition,     middlePointEndPosition);

        // Now that we have the intersection we can get our actual middle point shifted by horizontalOffset
        middlePointStartLine.Intersect(middlePointEndLine, out var ix, out var iy);
        var currentMiddlePosition = (middlePointEndPosition - currentEndPosition) * ix + currentEndPosition;

        // Finally set offset control points by copying everything but the position
        currentStartPoint = startPoint with
        {
            m_position = new Vector3(currentStartPosition.x, startPoint.m_position.y + verticalOffset, currentStartPosition.z)
        };
        currentMiddlePoint = middlePoint with
        {
            m_position = new Vector3(currentMiddlePosition.x, middlePoint.m_position.y + verticalOffset, currentMiddlePosition.z)
        };
        currentEndPoint = endPoint with
        {
            m_position = new Vector3(currentEndPosition.x, endPoint.m_position.y + verticalOffset, currentEndPosition.z)
        };
    }
}
