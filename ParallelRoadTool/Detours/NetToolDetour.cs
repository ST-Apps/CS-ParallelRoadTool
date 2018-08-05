using System.Reflection;
using ColossalFramework;
using ParallelRoadTool.Redirection;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Wrappers;
using UnityEngine;

namespace ParallelRoadTool.Detours
{
    /// <summary>
    ///     Detour used to hook into the RenderOverlay method for segments.
    /// </summary>
    public struct NetToolDetour
    {
        #region Detour

        private static readonly MethodInfo From = typeof(NetTool).GetMethod("RenderOverlay",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null,
            new[]
            {
                typeof(RenderManager.CameraInfo), typeof(NetInfo), typeof(Color), typeof(NetTool.ControlPoint),
                typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint)
            },
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

        /// <summary>
        ///     This methods skips our detour by calling the original method from the game, allowing the rendering for a single
        ///     segment.
        /// </summary>
        /// <param name="cameraInfo"></param>
        /// <param name="info"></param>
        /// <param name="color"></param>
        /// <param name="startPoint"></param>
        /// <param name="middlePoint"></param>
        /// <param name="endPoint"></param>
        private static void RenderOverlayOriginal(RenderManager.CameraInfo cameraInfo, NetInfo info, Color color,
            NetTool.ControlPoint startPoint, NetTool.ControlPoint middlePoint, NetTool.ControlPoint endPoint)
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

        /// <summary>
        ///     Overlay's core method.
        ///     First we render the base overlay, then we render an overlay for each of the selected roads, shifting them with the
        ///     correct offsets.
        ///     TODO: Probably RenderHelperLines is what we need to fix the look with curves, but detouring it makes Unity crash so
        ///     we have to live with this little issue.
        /// </summary>
        /// <param name="cameraInfo"></param>
        /// <param name="info"></param>
        /// <param name="color"></param>
        /// <param name="startPoint"></param>
        /// <param name="middlePoint"></param>
        /// <param name="endPoint"></param>
        private void RenderOverlay(RenderManager.CameraInfo cameraInfo, NetInfo info, Color color,
            NetTool.ControlPoint startPoint, NetTool.ControlPoint middlePoint, NetTool.ControlPoint endPoint)
        {
            // Let's render the original segment
            RenderOverlayOriginal(cameraInfo, info, color, startPoint, middlePoint, endPoint);

            for (var i = 0; i < ParallelRoadTool.SelectedRoadTypes.Count; i++)
            {
                var currentRoadInfos = ParallelRoadTool.SelectedRoadTypes[i];

                // Horizontal offset must be negated to appear on the correct side of the original segment
                //float horizontalOffset = 0;
                //if (Singleton<ParallelRoadTool>.instance.IsLeftHandTraffic)
                //    horizontalOffset = currentRoadInfos.HorizontalOffset;
                //else
                //    horizontalOffset = -currentRoadInfos.HorizontalOffset;

                var horizontalOffset = currentRoadInfos.HorizontalOffset * ((Singleton<ParallelRoadTool>.instance.IsLeftHandTraffic) ? 1 : -1);
                var verticalOffset = currentRoadInfos.VerticalOffset;

                // If the user didn't select a NetInfo we'll use the one he's using for the main road                
                var selectedNetInfo = info.GetNetInfoWithElevation(currentRoadInfos.NetInfo ?? info, out var isSlope);
                // If the user is using a vertical offset we try getting the relative elevated net info and use it
                if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() !=
                    ItemClass.CollisionType.Elevated)
                    selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).elevated ?? selectedNetInfo;

                // 50 shades of blue
                var newColor = new Color(Mathf.Clamp(color.r * 255 + i, 0, 255) / 255,
                    Mathf.Clamp(color.g * 255 + i, 0, 255) / 255, Mathf.Clamp(color.b * 255 + i, 0, 255) / 255,
                    color.a);

                // Render parallel segments by shifting the position of the 3 ControlPoint
                RenderOverlayOriginal(cameraInfo, selectedNetInfo, newColor, new NetTool.ControlPoint
                {
                    m_direction = startPoint.m_direction,
                    m_elevation = startPoint.m_elevation,
                    m_node = startPoint.m_node,
                    m_outside = startPoint.m_outside,
                    // startPoint may have a (0,0,0) direction, in that case we use the one from the middlePoint which is accurate enough to avoid overlapping starting nodes
                    m_position =
                        startPoint.m_position.Offset(
                            startPoint.m_direction == Vector3.zero ? middlePoint.m_direction : startPoint.m_direction,
                            horizontalOffset, verticalOffset),
                    m_segment = startPoint.m_segment
                }, new NetTool.ControlPoint
                {
                    m_direction = middlePoint.m_direction,
                    m_elevation = middlePoint.m_elevation,
                    m_node = middlePoint.m_node,
                    m_outside = middlePoint.m_outside,
                    m_position =
                        middlePoint.m_position.Offset(middlePoint.m_direction, horizontalOffset, verticalOffset),
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