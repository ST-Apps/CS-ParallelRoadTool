// <copyright file="Vector3Extensions.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Extensions;

using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

/// <summary>
///     This class provides extension methods for <see cref="Vector3"/> objects.
/// </summary>
public static class Vector3Extensions
{
    /// <summary>
    ///     Rotates the provided vector alongside the vertical axis by angle.
    /// </summary>
    /// <param name="vector">The <see cref="Vector3"/> object that needs to be rotated.</param>
    /// <param name="angle">Rotation angle expressed in degrees.</param>
    /// <returns>A copy of the original <see cref="Vector3"/> rotated by the provided angle alongisde the vertical axis.</returns>
    public static Vector3 RotateXZ(this Vector3 vector, float angle = 90)
    {
        return Quaternion.Euler(0, angle, 0) * vector;
    }

    /// <summary>
    ///     To be fair I have no idea on what this does, ported it from
    ///     <a href="https://codepen.io/brunoimbrizi/pen/VYEWgY?editors=0010">here</a>.
    ///     I guess it applies the offset to vector while also normalizing it.
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static Vector3 NormalizeWithOffset(this Vector3 vector, float offset)
    {
        if (vector.magnitude <= 0)
        {
            return vector;
        }

        var offsetMagnitude = offset / vector.magnitude;
        return vector with { x = vector.x * offsetMagnitude, z = vector.z * offsetMagnitude };
    }

    /// <summary>
    ///     Use ray-casting to detect if we have a node or a segment at specific position.
    /// </summary>
    /// <param name="position"><see cref="Vector3"/> containing the coordinates in which to search for a node or a segment.</param>
    /// <param name="netInfo">The <see cref="NetInfo"/> to look for.</param>
    /// <param name="nodeId">Node id at the given <see cref="Vector3"/> position, if any.</param>
    /// <param name="segmentId">Segment id at the given <see cref="Vector3"/> position, if any.</param>
    /// <returns>true if a node or a segment are found.</returns>
    public static bool AtPosition(this Vector3 position, NetInfo netInfo, out ushort nodeId, out ushort segmentId)
    {
        nodeId = 0;
        segmentId = 0;

        // Define input parameters for the RayCast
        var positionRay = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(position));
        var positionRayLength = Camera.main.farClipPlane;
        var input = new ToolBase.RaycastInput(positionRay, positionRayLength)
        {
            m_ignoreNodeFlags = NetNode.Flags.None,
            m_ignoreSegmentFlags = NetSegment.Flags.None,
        };
        var startPoint = input.m_ray.origin;
        var normalizedDirection = input.m_ray.direction.normalized;
        var endPoint = input.m_ray.origin + (normalizedDirection * input.m_length);
        var ray = new Segment3(startPoint, endPoint);
        var output = default(ToolBase.RaycastOutput);

        // RayCast and get the output
        if (!Singleton<NetManager>.instance.RayCast(
            netInfo,
            ray,
            input.m_netSnap,
            input.m_segmentNameOnly,
            input.m_netService.m_service,
            input.m_netService2.m_service,
            input.m_netService.m_subService,
            input.m_netService2.m_subService,
            input.m_netService.m_itemLayers,
            input.m_netService2.m_itemLayers,
            input.m_ignoreNodeFlags,
            input.m_ignoreSegmentFlags,
            out _,
            out output.m_netNode,
            out output.m_netSegment))
        {
            return false;
        }

        // Extract node id and segment id
        if (output.m_netNode != 0)
        {
            nodeId = output.m_netNode;
        }

        if (output.m_netSegment != 0)
        {
            segmentId = output.m_netSegment;
        }

        // Return true if we found either a node or a segment in the specified position
        return nodeId != 0 || segmentId != 0;
    }
}
