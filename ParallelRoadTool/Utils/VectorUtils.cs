// <copyright file="VectorUtils.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Utils;

using ColossalFramework.Math;
using UnityEngine;

internal static class VectorUtils
{
    public static Vector3 Intersection(Vector3 startPosition,
                                       Vector3 startDirection,
                                       Vector3 endPosition,
                                       Vector3 endDirection,
                                       out Line2 startLine,
                                       out Line2 endLine)
    {
        // Project both start point and endPoint far away following their direction
        // We first reverse endPoint's direction and normalize it
        endDirection = -endDirection.normalized;
        startDirection = startDirection.normalized;
        var endProjection = endPosition + (endDirection * 1000);
        var startProjection = startPosition + (startDirection * 1000);

        // We can now compute the two lines that will be intersected
        startLine = Line2.XZ(startPosition, startProjection);
        endLine = Line2.XZ(endPosition, endProjection);
        startLine.Intersect(endLine, out _, out var iy);

        return ((endProjection - endPosition) * iy) + endPosition;
    }
}
