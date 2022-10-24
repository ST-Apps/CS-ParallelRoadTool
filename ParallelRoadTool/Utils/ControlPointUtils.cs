// <copyright file="ControlPointUtils.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Utils;

using ColossalFramework;
using ColossalFramework.Math;
using CSUtil.Commons;
using Extensions;
using Managers;
using Patches;
using UnityEngine;

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
    /// <param name="networkIndex"></param>
    /// <param name="netMode"></param>
    /// <param name="currentStartPoint"></param>
    /// <param name="currentMiddlePoint"></param>
    /// <param name="currentEndPoint"></param>
    public static void GenerateOffsetControlPoints(
        NetTool.ControlPoint startPoint,
        NetTool.ControlPoint middlePoint,
        NetTool.ControlPoint endPoint,
        float horizontalOffset,
        float verticalOffset,
        NetInfo selectedNetInfo,
        int networkIndex,
        NetTool.Mode netMode,
        out NetTool.ControlPoint currentStartPoint,
        out NetTool.ControlPoint currentMiddlePoint,
        out NetTool.ControlPoint currentEndPoint)
    {
        // If ALT is pressed we switch the horizontal offset.
        // This is useful to upgrade curved roads when they appear on the wrong side.
        // Curved roads have some weird things happening with directions so that two close segments will have opposite start/end direction signs.
        // This difference causes the two close segments to be upgraded on two different sides, leading to weird results.
        if (ModifiersManager.IsAltPressed)
        {
            horizontalOffset *= -1;
        }

        // To offset both starting and ending point we need to get the right direction
        var startDirection = startPoint.m_direction.NormalizeWithOffset(horizontalOffset).RotateXZ();
        var endDirection = endPoint.m_direction.NormalizeWithOffset(horizontalOffset).RotateXZ();

        // Move start and end point to the correct direction
        var currentStartPosition = startPoint.m_position + startDirection + (Vector3.up * verticalOffset);
        var currentEndPosition = endPoint.m_position + endDirection + (Vector3.up * verticalOffset);

        // Finally set offset control points by copying everything but the position
        currentStartPoint = startPoint with
        {
            m_node = 0, m_segment = 0, m_position = currentStartPosition, m_elevation = startPoint.m_elevation + verticalOffset
        };
        currentEndPoint = endPoint with
        {
            m_node = 0, m_segment = 0, m_position = currentEndPosition, m_elevation = endPoint.m_elevation + verticalOffset
        };

        switch (netMode)
        {
            // If we're on straight mode we just set middlePoint as endPoint because we don't need more than 2 control points
            case NetTool.Mode.Straight:
                currentMiddlePoint = currentEndPoint with { m_elevation = (currentStartPoint.m_elevation + currentEndPoint.m_elevation) / 2 };

                // Handle angle compensation feature.
                // Check if we need to look for an intersection point to move our previously created ending point.
                // This is needed because certain angles will cause the segments to overlap.
                // To fix this we create a parallel line from the original segment, we extend a line from the previous ending point and check if they intersect.
                if (Singleton<ParallelRoadToolManager>.instance.IsAngleCompensationEnabled &&
                    Singleton<ParallelRoadToolManager>.instance.PullGeneratedNodes(startPoint.m_node, out var previousEndPoint))
                {
                    // Initialize the intersection point that we will use to move currentStartPoint to
                    var intersectionPoint = Vector3.zero;

                    // This means that we're trying to build a perfectly straight segment, so we use endPoint's direction
                    if (middlePoint.m_direction == endPoint.m_direction)
                    {
                        intersectionPoint = VectorUtils.Intersection(
                            previousEndPoint[networkIndex].m_position,
                            previousEndPoint[networkIndex].m_direction,
                            currentEndPoint.m_position,
                            currentEndPoint.m_direction,
                            out var startLine,
                            out var endLine);
                    }
                    else
                    {
                        // Otherwise we're in bending mode so we need to consider startPoint's direction
                        intersectionPoint = VectorUtils.Intersection(
                            previousEndPoint[networkIndex].m_position,
                            previousEndPoint[networkIndex].m_direction,
                            currentStartPoint.m_position,
                            currentStartPoint.m_direction,
                            out var startLine,
                            out var endLine);
                    }

                    // We found a valid intersection point, set startPoint's position to that one
                    if (intersectionPoint != Vector3.zero)
                    {
                        currentStartPoint.m_position = intersectionPoint;

                        // If we are in bending mode we also need to update middle point's position, just as we would do for curves
                        if (middlePoint.m_direction != endPoint.m_direction)
                        {
                            // In this case we also need to update middle point because this is not a straight segment anymore
                            // We use middle point for intersection because it's more accurate than endpoint
                            var currentMiddlePosition = VectorUtils.Intersection(
                                currentStartPosition,
                                startPoint.m_direction,
                                middlePoint.m_position,
                                middlePoint.m_direction.RotateXZ(-45).normalized,
                                out _,
                                out _);

                            // Finally set the point
                            currentMiddlePoint = middlePoint with
                            {
                                m_node = 0,
                                m_segment = 0,
                                m_position = currentMiddlePosition == Vector3.zero ? currentEndPoint.m_position : currentMiddlePosition,
                                m_elevation = (currentStartPoint.m_elevation + currentEndPoint.m_elevation) / 2
                            };
                        }
                    }
                }

                break;
            case NetTool.Mode.Upgrade:
            {
                var middleDirection = middlePoint.m_direction.NormalizeWithOffset(horizontalOffset).RotateXZ();
                var currentMiddlePosition = middlePoint.m_position + middleDirection + (Vector3.up * verticalOffset);

                currentMiddlePoint = middlePoint with
                {
                    m_node = 0, m_segment = 0, m_position = currentMiddlePosition, m_elevation = middlePoint.m_elevation + verticalOffset
                };
                break;
            }

            case NetTool.Mode.Curved:
            case NetTool.Mode.Freeform:
            default:
            {
                // If not on straight mode we compute the middle point by getting the intersection between startPoint and endPoint
                // Both points will be projected in the respective directions to find the intersection
                var currentMiddlePosition = VectorUtils.Intersection(
                    currentStartPosition,
                    startPoint.m_direction,
                    currentEndPosition,
                    endPoint.m_direction,
                    out _,
                    out _);

                // Finally set the point
                currentMiddlePoint = middlePoint with
                {
                    m_node = 0,
                    m_segment = 0,
                    m_position = currentMiddlePosition == Vector3.zero ? currentEndPoint.m_position : currentMiddlePosition,
                    m_elevation = (currentStartPoint.m_elevation + currentEndPoint.m_elevation) / 2
                };
                break;
            }
        }

        // Check if already have nodes at position and use them, but only if snapping is on (disable it for upgrade mode to prevent unintended results)
        if (Singleton<ParallelRoadToolManager>.instance.IsSnappingEnabled && netMode != NetTool.Mode.Upgrade)
        {
            if (currentStartPoint.m_position.AtPosition(selectedNetInfo, out currentStartPoint.m_node, out currentStartPoint.m_segment))
            {
                Log._Debug($"[{nameof(ControlPointUtils)}.{nameof(GenerateOffsetControlPoints)}] Found a node at {currentStartPoint.m_position} with nodeId={currentStartPoint.m_node} and segmentId={currentStartPoint.m_segment} (start)");
            }

            if (currentEndPoint.m_position.AtPosition(selectedNetInfo, out currentEndPoint.m_node, out currentEndPoint.m_segment))
            {
                Log._Debug($"[{nameof(ControlPointUtils)}.{nameof(GenerateOffsetControlPoints)}] Found a node at {currentEndPoint.m_position} with nodeId={currentEndPoint.m_node} and segmentId={currentEndPoint.m_segment} (end)");
            }
        }

        // If nodes are still 0 we can check if we have some matching the ones in our buffer
        if (currentStartPoint.m_node == 0 &&
            Singleton<ParallelRoadToolManager>.instance.PullGeneratedNodes(startPoint.m_node, out var tempCurrentStartPoint) &&
            networkIndex < tempCurrentStartPoint.Length && tempCurrentStartPoint[networkIndex].m_node != 0)
        {
            currentStartPoint.m_node = tempCurrentStartPoint[networkIndex].m_node;
            Log._Debug($"[{nameof(ControlPointUtils)}.{nameof(GenerateOffsetControlPoints)}] Set currentStartPoint's node to {currentStartPoint.m_node} from buffer (start)");
        }

        if (currentEndPoint.m_node == 0 &&
            Singleton<ParallelRoadToolManager>.instance.PullGeneratedNodes(endPoint.m_node, out var tempCurrentEndPoint) &&
            networkIndex < tempCurrentEndPoint.Length && tempCurrentEndPoint[networkIndex].m_node != 0)
        {
            currentEndPoint.m_node = tempCurrentEndPoint[networkIndex].m_node;
            Log._Debug($"[{nameof(ControlPointUtils)}.{nameof(GenerateOffsetControlPoints)}] Set currentEndPoint's node to {currentEndPoint.m_node} from buffer (start)");
        }
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
        direction = Quaternion.Euler(0, -90, 0) * direction.normalized;

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
    public static bool CanCreate(
        NetInfo netInfo,
        NetTool.ControlPoint currentStartPoint,
        NetTool.ControlPoint currentMiddlePoint,
        NetTool.ControlPoint currentEndPoint,
        bool isReversed)
    {
        return NetToolReversePatch.CreateNode(netInfo, currentStartPoint, currentMiddlePoint, currentEndPoint, NetTool.m_nodePositionsSimulation, 1000, true,
                                              false, true, true, false, isReversed, 0, out _, out _, out _, out _) == ToolBase.ToolErrors.None;
    }
}
