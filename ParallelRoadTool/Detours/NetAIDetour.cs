using System;
using System.Reflection;
using ColossalFramework;
using CSUtil.Commons;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Wrappers;
using RedirectionFramework;
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
                                                                                    typeof(Vector3), typeof(Vector3), typeof(float), typeof(float)
                                                                                },
                                                                                null);

        private static readonly MethodInfo To =
            typeof(NetAIDetour).GetMethod("GetConstructionCost", BindingFlags.NonPublic | BindingFlags.Static);

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
        ///     First we get the cost for the main selected segment, then we compute the cost for each parallel/stacked ones to get to the final cost.
        /// </summary>

        // ReSharper disable once UnusedMember.Local
        private static int GetConstructionCost(Vector3 startPos, Vector3 endPos, float startHeight, float endHeight)
        {
            // Disable our detour so that we can call the original method
            Revert();

            try
            {
                // Get initial cost for the currently selected network
                var cost = ParallelRoadTool.CurrentNetwork.m_netAI.GetConstructionCost(startPos, endPos, startHeight, endHeight);

                foreach (var currentRoadInfos in Singleton<ParallelRoadTool>.instance.SelectedRoadTypes)
                {
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
            catch (Exception e)
            {
                // Log the exception and return 0 as we can't recover from this
                Log._DebugOnlyError($"[{nameof(NetAIDetour)}.{nameof(GetConstructionCost)}] GetConstructionCost failed.");
                Log.Exception(e);

                return 0;
            }
            finally
            {
                // Restore our detour so that we can call the original method
                Deploy();
            }
        }
    }
}
