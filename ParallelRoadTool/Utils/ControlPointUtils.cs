using System;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
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
            m_node = 0,
            m_segment = 0,
            m_position = new Vector3(currentStartPosition.x, startPoint.m_position.y + verticalOffset, currentStartPosition.z)
        };
        currentMiddlePoint = middlePoint with
        {
            m_node = 0,
            m_segment = 0,
            m_position = new Vector3(currentMiddlePosition.x, middlePoint.m_position.y + verticalOffset, currentMiddlePosition.z)
        };
        currentEndPoint = endPoint with
        {
            m_node = 0,
            m_segment = 0,
            m_position = new Vector3(currentEndPosition.x, endPoint.m_position.y + verticalOffset, currentEndPosition.z)
        };

        //var m_mouseRay = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(currentStartPoint.m_position));
        //var m_mouseRayLength = Camera.main.farClipPlane;
        //ToolBase.RaycastInput input = new ToolBase.RaycastInput(m_mouseRay, m_mouseRayLength);
        //Vector3 origin = input.m_ray.origin;
        //Vector3 normalized = input.m_ray.direction.normalized;
        //Vector3 _b = input.m_ray.origin + normalized * input.m_length;
        //Segment3 ray = new Segment3(origin, _b);
        //ToolBase.RaycastOutput output = new ToolBase.RaycastOutput();
        //if (Singleton<NetManager>.instance.RayCast(input.m_buildObject as NetInfo, ray, input.m_netSnap, input.m_segmentNameOnly,
        //                                           input.m_netService.m_service, input.m_netService2.m_service, input.m_netService.m_subService,
        //                                           input.m_netService2.m_subService, input.m_netService.m_itemLayers,
        //                                           input.m_netService2.m_itemLayers, input.m_ignoreNodeFlags, input.m_ignoreSegmentFlags, out var a,
        //                                           out output.m_netNode, out output.m_netSegment))
        //{

        //    //var rayCastMethod = typeof(ToolBase).GetMethod("RayCast", BindingFlags.NonPublic | BindingFlags.Static);
        //    //ToolBase.RaycastOutput output = new ToolBase.RaycastOutput();
        //    //var args = new object[] { input, output };
        //    //rayCastMethod.Invoke(null, args);
        //    //tmpPatch.RayCast(input, out var output);
        //    if (output.m_netNode != 0)
        //        currentStartPoint.m_node = output.m_netNode;
        //}
    }

    //[HarmonyPatch]
    //internal static class tmpPatch
    //{
    //    [HarmonyReversePatch]
    //    [HarmonyPatch(typeof(ToolBase), "RayCast", typeof(ToolBase.RaycastInput), typeof(ToolBase.RaycastOutput))]
    //    internal static bool RayCast(ToolBase.RaycastInput input, out ToolBase.RaycastOutput output)
    //    {
    //        // No implementation is required as this will call the original method
    //        throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
    //    }
    //}
}
