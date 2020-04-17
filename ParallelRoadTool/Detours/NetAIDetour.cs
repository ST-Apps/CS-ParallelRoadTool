using ColossalFramework;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Redirection;
using ParallelRoadTool.Utils;
using ParallelRoadTool.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ParallelRoadTool.Detours
{
    public struct NetAIDetour
    {

        #region Detour

        private static readonly MethodInfo From = typeof(PlayerNetAI).GetMethod("GetConstructionCost",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
            null,
            new[]
            {
                typeof(Vector3), typeof(Vector3),typeof(float), typeof(float)
            },
            null);

        private static readonly MethodInfo To =
            typeof(NetAIDetour).GetMethod("GetConstructionCost", BindingFlags.NonPublic | BindingFlags.Instance);

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
        private int GetConstructionCost(Vector3 startPos, Vector3 endPos, float startHeight, float endHeight)
        {
            // Disable our detour so that we can call the original method
            Revert();

            try
            {
                // Get initial cost for the currently selected network
                var cost = Singleton<ParallelRoadTool>.instance.CurrentNetwork.m_netAI.GetConstructionCost(startPos, endPos, startHeight, endHeight);

                for (var i = 0; i < Singleton<ParallelRoadTool>.instance.SelectedRoadTypes.Count; i++)
                {
                    var currentRoadInfos = Singleton<ParallelRoadTool>.instance.SelectedRoadTypes[i];

                    // Horizontal offset must be negated to appear on the correct side of the original segment                    
                    var verticalOffset = currentRoadInfos.VerticalOffset;

                    // If the user didn't select a NetInfo we'll use the one he's using for the main road                
                    var selectedNetInfo = currentRoadInfos.NetInfo.GetNetInfoWithElevation(currentRoadInfos.NetInfo, out _);

                    // If the user is using a vertical offset we try getting the relative elevated net info and use it
                    if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() != ItemClass.CollisionType.Elevated)
                        selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).elevated ?? selectedNetInfo;
                    
                    // Add the cost for the parallel segment
                    // TODO: it would be good to have startPos and endPos for the parallel segments instead of the ones of the first segment, but we don't have the direction here so we can't offset properly.
                    cost += selectedNetInfo.m_netAI.GetConstructionCost(startPos, endPos, startHeight, endHeight);
                }

                return cost;
            }
            finally
            {
                // Restore our detour so that we can call the original method
                Deploy();
            }
        }

    }
}
