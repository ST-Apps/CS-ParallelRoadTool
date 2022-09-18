using System;
using ColossalFramework;
using CSUtil.Commons;
using HarmonyLib;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Models;
using ParallelRoadTool.Wrappers;
using UnityEngine;

// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local

namespace ParallelRoadTool.Patches
{
    [HarmonyPatch(typeof(PlayerNetAI),
                  nameof(PlayerNetAI.GetConstructionCost), typeof(Vector3), typeof(Vector3), typeof(float), typeof(float))]
    internal class NetAIPatch
    {
        // We compute the cost for each parallel/stacked ones to get to the final cost and then we add it to the one received as input for the original segment
        private static void Postfix(Vector3 startPos, Vector3 endPos, float startHeight, float endHeight, ref int __result)
        {
            try
            {
                // We only run if the mod is set as Active
                if (!ParallelRoadToolManager.ModStatuses.IsFlagSet(ModStatuses.Active))
                    return;

                foreach (var currentRoadInfos in Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes)
                {
                    // Horizontal offset must be negated to appear on the correct side of the original segment                    
                    var verticalOffset = currentRoadInfos.VerticalOffset;

                    // If the user didn't select a NetInfo we'll use the one he's using for the main road                
                    var selectedNetInfo = currentRoadInfos.NetInfo.GetNetInfoWithElevation(currentRoadInfos.NetInfo, out _);

                    // If the user is using a vertical offset we try getting the relative elevated net info and use it
                    if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() != ItemClass.CollisionType.Elevated)
                        selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).elevated ?? selectedNetInfo;

                    // Add the cost for the parallel segment
                    __result += NetAIReversePatch.GetConstructionCost(selectedNetInfo.m_netAI, startPos, endPos, startHeight, endHeight);
                }
            }
            catch (Exception e)
            {
                // Log the exception and return 0 as we can't recover from this
                Log._DebugOnlyError($"[{nameof(NetAIPatch)}.{nameof(Postfix)}] GetConstructionCost failed.");
                Log.Exception(e);

                __result = 0;
            }
        }

        /// <summary>
        ///     The reverse patch is meant as an easy way to access the original <see cref="PlayerNetAI.GetConstructionCost" />
        ///     method.
        /// </summary>
        [HarmonyPatch]
        private class NetAIReversePatch
        {
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(PlayerNetAI),
                          nameof(PlayerNetAI.GetConstructionCost), typeof(Vector3), typeof(Vector3), typeof(float), typeof(float))]
            public static int GetConstructionCost(object instance, Vector3 startPos, Vector3 endPos, float startHeight, float endHeight)
            {
                // No implementation is required as this will call the original method
                throw new NotImplementedException("It's a stub");
            }
        }
    }
}
