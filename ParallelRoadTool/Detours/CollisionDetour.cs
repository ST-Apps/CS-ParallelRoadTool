using System;
using System.Runtime.InteropServices;
using System.Reflection;
using UnityEngine;

using ColossalFramework.Math;

using ParallelRoadTool.Redirection;

namespace ParallelRoadTool.Detours
{
    [TargetType(typeof(BuildingManager))]
    public class CollisionBuildingManagerDetour
    {
        [RedirectMethod]
        public bool OverlapQuad(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, ItemClass.Layer layers, ushort ignoreBuilding, ushort ignoreNode1, ushort ignoreNode2, ulong[] buildingMask)
        {
            return false;
        }
    }

    [TargetType(typeof(NetManager))]
    public class CollisionNetManagerDetour
    {
        [RedirectMethod]
        public bool OverlapQuad(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, ItemClass.Layer layers, ushort ignoreNode1, ushort ignoreNode2, ushort ignoreSegment, ulong[] segmentMask)
        {
            return false;
        }
    }

    [TargetType(typeof(NetNode))]
    public struct CollisionNetNodeDetour
    {
        [RedirectMethod]
        private bool TestNodeBuilding(ushort nodeID, BuildingInfo info, Vector3 position, float angle)
        {
            return true;
        }
    }

    unsafe public struct CollisionZoneBlockDetour
    {
        private static MethodInfo from = typeof(ZoneBlock).GetMethod("CalculateImplementation1", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo to = typeof(CollisionZoneBlockDetour).GetMethod("CalculateImplementation1", BindingFlags.NonPublic | BindingFlags.Instance);

        private static RedirectCallsState m_state;
        private static bool m_deployed = false;

        public static void Deploy()
        {
            if (!m_deployed)
            {
                m_state = RedirectionHelper.RedirectCalls(from, to);
                m_deployed = true;
            }
        }

        public static void Revert()
        {
            if (m_deployed)
            {
                RedirectionHelper.RevertRedirect(from, m_state);
                m_deployed = false;
            }
        }

        private void CalculateImplementation1(ushort blockID, ushort segmentID, ref NetSegment data, ref ulong valid, float minX, float minZ, float maxX, float maxZ)
        {
            if(data.Info.m_flattenTerrain)
            {
                RedirectionHelper.RevertRedirect(from, m_state);
                
                fixed (void* pointer = &this)
                {
                    ZoneBlock* block = (ZoneBlock*)pointer;

                    object[] param = new object[] { blockID, segmentID, data, valid, minX, minZ, maxX, maxZ };
                    from.Invoke(*block, param);
                    valid = (ulong)param[3];
                }

                m_state = RedirectionHelper.RedirectCalls(from, to);
            }
        }
    }
}
