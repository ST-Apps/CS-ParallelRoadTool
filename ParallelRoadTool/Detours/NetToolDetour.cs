using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ColossalFramework;
using ColossalFramework.Math;
using FineRoadTool;
using ICities;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Redirection;
using ParallelRoadTool.Wrappers;
using UnityEngine;

namespace ParallelRoadTool.Detours
{
    public struct NetToolDetour
    {
        #region Detour
        private static readonly MethodInfo From = typeof(NetTool).GetMethod("RenderOverlay",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null,
            new[] { typeof(RenderManager.CameraInfo), typeof(NetInfo), typeof(Color), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint) },
            null);

        private static readonly MethodInfo To =
            typeof(NetToolDetour).GetMethod("RenderOverlay", BindingFlags.NonPublic | BindingFlags.Instance);

        private static RedirectCallsState _state;
        private static bool _deployed;

        public static void Deploy()
        {
            if (_deployed) return;
            _state = RedirectionHelper.RedirectCalls(From, To);
            _deployed = true;
        }

        public static void Revert()
        {
            if (!_deployed) return;
            RedirectionHelper.RevertRedirect(From, _state);
            _deployed = false;
        }
        #endregion

        #region Utils

        private static void RenderOverlayOriginal(RenderManager.CameraInfo cameraInfo, NetInfo info, Color color, NetTool.ControlPoint startPoint, NetTool.ControlPoint middlePoint, NetTool.ControlPoint endPoint)
        {
            Revert();

            From.Invoke(ParallelRoadTool.NetTool, new object[]
            {
                cameraInfo,
                info,
                color,
                startPoint,
                middlePoint,
                endPoint
            });

            Deploy();
        }

        #endregion

        private void RenderOverlay(RenderManager.CameraInfo cameraInfo, NetInfo info, Color color, NetTool.ControlPoint startPoint, NetTool.ControlPoint middlePoint, NetTool.ControlPoint endPoint)
        {
            RenderOverlayOriginal(cameraInfo, info, color, startPoint, middlePoint, endPoint);

            if (!ParallelRoadTool.Instance.IsToolActive) return;

            foreach (var currentRoadInfos in ParallelRoadTool.SelectedRoadTypes)
            {
                var horizontalOffset = -currentRoadInfos.HorizontalOffset;
                var verticalOffset = currentRoadInfos.VerticalOffset;

                // If the user didn't select a NetInfo we'll use the one he's using for the main road                
                var selectedNetInfo = info.GetNetInfoWithElevation(currentRoadInfos.NetInfo ?? info, out var isSlope);
                // If the user is using a vertical offset we try getting the relative elevated net info and use it
                if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() !=
                    ItemClass.CollisionType.Elevated)
                    selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).elevated ?? selectedNetInfo;

                RenderOverlayOriginal(cameraInfo, selectedNetInfo, Color.red, new NetTool.ControlPoint
                {
                    m_direction = startPoint.m_direction,
                    m_elevation = startPoint.m_elevation,
                    m_node = startPoint.m_node,
                    m_outside = startPoint.m_outside,
                    m_position = startPoint.m_position.Offset(startPoint.m_direction, horizontalOffset, verticalOffset),
                    m_segment = startPoint.m_segment
                }, new NetTool.ControlPoint
                {
                    m_direction = middlePoint.m_direction,
                    m_elevation = middlePoint.m_elevation,
                    m_node = middlePoint.m_node,
                    m_outside = middlePoint.m_outside,
                    m_position = middlePoint.m_position.Offset(middlePoint.m_direction, horizontalOffset, verticalOffset),
                    m_segment = middlePoint.m_segment
                }, new NetTool.ControlPoint
                {
                    m_direction = endPoint.m_direction,
                    m_elevation = endPoint.m_elevation,
                    m_node = endPoint.m_node,
                    m_outside = endPoint.m_outside,
                    m_position = endPoint.m_position.Offset(endPoint.m_direction, horizontalOffset, verticalOffset),
                    m_segment = endPoint.m_segment
                });
            }
        }
    }    
}
