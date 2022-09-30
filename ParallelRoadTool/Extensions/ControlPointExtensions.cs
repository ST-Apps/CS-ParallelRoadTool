using ColossalFramework.Math;
using UnityEngine;

namespace ParallelRoadTool.Extensions;

internal static class ControlPointExtensions
{
    public static NetTool.ControlPoint MoveOffset(this NetTool.ControlPoint controlPoint,
                                                  float                     horizontalOffset,
                                                  float                     verticalOffset,
                                                  Vector3?                  fallbackDirection = null,
                                                  Vector3?                  overrideDirection = null)
    {
        var direction = controlPoint.m_direction;
        if (direction == Vector3.zero && fallbackDirection.HasValue) direction = fallbackDirection.Value;
        if (overrideDirection != null) direction                               = overrideDirection.Value;

        return controlPoint with
        {
            m_elevation = controlPoint.m_elevation + verticalOffset,
            m_position = controlPoint.m_position.MoveOffset(direction, horizontalOffset, verticalOffset)
        };
    }

    public static Vector3 FindMiddlePointPosition(this NetTool.ControlPoint thisPoint, NetTool.ControlPoint otherPoint)
    {
        //var bezier = new Bezier3 { a = thisPoint.m_position, d = otherPoint.m_position };
        //NetSegment.CalculateMiddlePoints(bezier.a, thisPoint.m_direction, bezier.d, -otherPoint.m_direction, true, true, out bezier.b, out bezier.c);

        //return bezier.Position(0.5f);

        return Vector3.Lerp(otherPoint.m_position, thisPoint.m_position, 0.5f);

        // return new Vector3((thisPoint.m_position.x + otherPoint.m_position.x) / 2, thisPoint.m_position.y, (thisPoint.m_position.z + otherPoint.m_position.z) / 2);
        //P1.x = (P0.x + P2.x) / 2;
        //P1.y = P0.y - 140;
        //P1.z = (P0.z + P2.z) / 2;
    }
}
