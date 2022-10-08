using ColossalFramework;
using ColossalFramework.Math;
using CSUtil.Commons;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Managers;
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
        var currentStartPosition = startPoint.m_position + currentStartDirection + Vector3.up * verticalOffset;
        var currentEndPosition = endPoint.m_position     + currentEndDirection   + Vector3.up * verticalOffset;

        Log._Debug($">>>> START: {startPoint.m_position} --> {currentStartPosition}");
        Log._Debug($">>>> END  : {endPoint.m_position} --> {currentEndPosition}");

        // Get two intersection points for the pairs (start, mid) and (mid, end)
        var middlePointStartPosition = middlePoint.m_position + currentStartDirection;
        var middlePointEndPosition = middlePoint.m_position   + currentEndDirection;
        var middlePointStartLine = Line2.XZ(currentStartPosition, middlePointStartPosition);
        var middlePointEndLine = Line2.XZ(currentEndPosition,     middlePointEndPosition);

        // Now that we have the intersection we can get our actual middle point shifted by horizontalOffset
        middlePointStartLine.Intersect(middlePointEndLine, out var ix, out var iy);
        var currentMiddlePosition = (middlePointEndPosition - currentEndPosition) * ix + currentEndPosition + Vector3.up * verticalOffset;

        // Finally set offset control points by copying everything but the position
        currentStartPoint = startPoint with
        {
            m_node = 0,
            m_segment = 0,
            m_position = currentStartPosition //new Vector3(currentStartPosition.x, startPoint.m_position.y + verticalOffset, currentStartPosition.z)
        };
        currentMiddlePoint = middlePoint with
        {
            m_node = 0,
            m_segment = 0,
            m_position = currentMiddlePosition //new Vector3(currentMiddlePosition.x, middlePoint.m_position.y + verticalOffset, currentMiddlePosition.z)
        };
        currentEndPoint = endPoint with
        {
            m_node = 0,
            m_segment = 0,
            m_position = currentEndPosition //new Vector3(currentEndPosition.x, endPoint.m_position.y + verticalOffset, currentEndPosition.z)
        };

        // if (!Singleton<ParallelRoadToolManager>.instance.IsSnappingEnabled) return;
        //currentStartPoint.m_node = NodeUtils.NodeIdAtPosition(currentStartPosition);
        //currentEndPoint.m_node   = NodeUtils.NodeIdAtPosition(currentEndPosition);

        Log._Debug($">>> Updated start and end nodes: {currentStartPoint.m_node}, {currentEndPoint.m_node}");

        //         Log._Debug($">>>>>\nSTART:\t\t\t{startPoint.m_position}\nCURRENT_START:\t{currentStartPosition}\nEND:\t\t\t{endPoint.m_position}\nCURRENT_END:\t{currentEndPosition}");

        if (AtPosition(currentStartPoint.m_position, Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes[0].NetInfo,
                       out currentStartPoint.m_node, out currentStartPoint.m_segment))
            Log._Debug($">>> currentStartPoint: {currentStartPoint.m_node} - {currentStartPoint.m_segment}");

        if (AtPosition(currentEndPoint.m_position, Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes[0].NetInfo,
                       out currentEndPoint.m_node, out currentEndPoint.m_segment))
            Log._Debug($">>> currentEndPoint: {currentEndPoint.m_node} - {currentEndPoint.m_segment}");

        // var sn = NodeUtils.NodeIdAtPosition(currentStartPosition);
        // var en = NodeUtils.NodeIdAtPosition(currentEndPosition);

        // Log._Debug($">>>>> {sn} - {en}");
    }

    private static bool AtPosition(Vector3 position, NetInfo netInfo, out ushort nodeId, out ushort segmentId)
    {
        nodeId    = 0;
        segmentId = 0;

        var m_mouseRay = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(position));
        var m_mouseRayLength = Camera.main.farClipPlane;
        var input = new ToolBase.RaycastInput(m_mouseRay, m_mouseRayLength)
        {
            m_ignoreNodeFlags = NetNode.Flags.None, m_ignoreSegmentFlags = NetSegment.Flags.None
        };

        var origin = input.m_ray.origin;
        var normalized = input.m_ray.direction.normalized;
        var _b = input.m_ray.origin + normalized * input.m_length;
        var ray = new Segment3(origin, _b);
        var output = new ToolBase.RaycastOutput();

        if (!Singleton<NetManager>.instance.RayCast(netInfo, ray, input.m_netSnap, input.m_segmentNameOnly, input.m_netService.m_service,
                                                    input.m_netService2.m_service, input.m_netService.m_subService, input.m_netService2.m_subService,
                                                    input.m_netService.m_itemLayers, input.m_netService2.m_itemLayers, input.m_ignoreNodeFlags,
                                                    input.m_ignoreSegmentFlags, out var a, out output.m_netNode, out output.m_netSegment))
            return false;

        if (output.m_netNode != 0)
            nodeId = output.m_netNode;

        if (output.m_netSegment != 0)
            segmentId = output.m_netSegment;

        return nodeId != 0 || segmentId != 0;
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
