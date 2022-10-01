using System;
using CSUtil.Commons;
using HarmonyLib;
using ParallelRoadTool.Extensions;
using UnityEngine;

namespace ParallelRoadTool.Patches
{
    //[HarmonyPatch(typeof(NetTool), "CreateNodeImpl", typeof(NetInfo), typeof(bool), typeof(bool), typeof(NetTool.ControlPoint),
    //              typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
    internal class NetToolNodePatch
    {
        internal static void Prefix(NetInfo              info,
                                     bool                 needMoney,
                                     bool                 switchDirection,
                                     NetTool.ControlPoint startPoint,
                                     NetTool.ControlPoint middlePoint,
                                     NetTool.ControlPoint endPoint)
        {
            Log.Info($"[{nameof(NetToolNodePatch)}.{nameof(Prefix)} Received: {info.name}, {needMoney}, {switchDirection}, {startPoint.m_position} [{startPoint.m_direction}], {middlePoint.m_position} [{middlePoint.m_direction}], {endPoint.m_position} [{endPoint.m_direction}]");

            var netTool          = ToolsModifierControl.GetTool<NetTool>();
            var horizontalOffset = 16f;
            var verticalOffset = 4f;

            var startPointDirection = startPoint.m_direction;
            if (startPointDirection == Vector3.zero)
            {
                startPointDirection.x = middlePoint.m_direction.z;
                startPointDirection.y = middlePoint.m_direction.y;
                startPointDirection.z = middlePoint.m_direction.x;
            }

            var currentStartPoint = new NetTool.ControlPoint
            {
                m_direction = startPointDirection,
                m_elevation = startPoint.m_elevation + verticalOffset,
                m_node      = 0,
                m_outside   = startPoint.m_outside,

                // startPoint may have a (0,0,0) direction, in that case we use the one from the middlePoint which is accurate enough to avoid overlapping starting nodes
                m_position
                    = startPoint.m_position.Offset(startPointDirection, horizontalOffset, verticalOffset),
                m_segment = 0
            };

            var currentMidPoint = new NetTool.ControlPoint
            {
                m_direction = middlePoint.m_direction,
                m_elevation = middlePoint.m_elevation + verticalOffset,
                m_node      = 0,
                m_outside   = middlePoint.m_outside,
                m_position  = middlePoint.m_position.Offset(middlePoint.m_direction, horizontalOffset, verticalOffset),
                m_segment   = 0
            };

            var currentEndPoint = new NetTool.ControlPoint
            {
                m_direction = endPoint.m_direction,
                m_elevation = endPoint.m_elevation + verticalOffset,
                m_node      = 0,
                m_outside   = endPoint.m_outside,
                m_position  = endPoint.m_position.Offset(endPoint.m_direction, horizontalOffset, verticalOffset),
                m_segment   = 0
            };

            Log.Info($"[{nameof(NetToolNodePatch)}.{nameof(Prefix)} Creating: {info.name}, {needMoney}, {switchDirection}, {currentStartPoint.m_position}, {currentMidPoint.m_position}, {currentEndPoint.m_position}");

            NetToolReversePatch.CreateNodeImpl(netTool, info, needMoney, switchDirection, currentStartPoint, currentMidPoint, currentEndPoint);
        }

        [HarmonyPatch]
        private class NetToolReversePatch
        {

            [HarmonyReversePatch]
            [HarmonyPatch(typeof(NetTool), "CreateNodeImpl", typeof(NetInfo), typeof(bool), typeof(bool), typeof(NetTool.ControlPoint),
                          typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
            public static bool CreateNodeImpl(object               instance,
                                              NetInfo              info,
                                              bool                 needMoney,
                                              bool                 switchDirection,
                                              NetTool.ControlPoint startPoint,
                                              NetTool.ControlPoint middlePoint,
                                              NetTool.ControlPoint endPoint)
            {
                // No implementation is required as this will call the original method
                throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
            }
        }
    }
}
