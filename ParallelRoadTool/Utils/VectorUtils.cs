using ColossalFramework.Math;
using UnityEngine;

namespace ParallelRoadTool.Utils;

internal static class VectorUtils
{
    public static Vector3 Intersection(Vector3 startPosition, Vector3 startDirection, Vector3 endPosition, Vector3 endDirection)
    {
        //middlePointStartPosition = middlePoint.m_position + startDirection;
        //middlePointEndPosition   = middlePoint.m_position + endDirection;
        //var middlePointStartLine = Line2.XZ(currentStartPosition, middlePointStartPosition);
        //var middlePointEndLine = Line2.XZ(currentEndPosition,     middlePointEndPosition);

        //// Now that we have the intersection we can get our actual middle point shifted by horizontalOffset
        //middlePointStartLine.Intersect(middlePointEndLine, out var ix, out var iy);
        //var currentMiddlePosition = (middlePointEndPosition - currentEndPosition) * iy + currentEndPosition + Vector3.up * verticalOffset;

        // Project both start point and endPoint far away following their direction
        // We first reverse endPoint's direction and normalize it
        endDirection   = -endDirection.normalized;
        startDirection = startDirection.normalized;
        var endProjection = endPosition     + endDirection   * 1000;
        var startProjection = startPosition + startDirection * 1000;

        // We can now compute the two lines that will be intersected
        var startLine = Line2.XZ(startPosition, startProjection);
        var endLine = Line2.XZ(endPosition,     endProjection);
        startLine.Intersect(endLine, out _, out var iy);

        return (endProjection - endPosition) * iy + endPosition;
    }
}
