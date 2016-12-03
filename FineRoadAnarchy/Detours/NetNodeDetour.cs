using UnityEngine;

using ColossalFramework.Math;

using FineRoadAnarchy.Redirection;

namespace FineRoadAnarchy.Detours
{
    [TargetType(typeof(NetNode))]
    public struct NetNodeDetour
    {
        [RedirectMethod]
        private bool TestNodeBuilding(ushort nodeID, BuildingInfo info, Vector3 position, float angle)
        {
            return true;

            /*Vector2 vector = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 vector2 = new Vector3(vector.y, -vector.x);
            if (info.m_placementMode == BuildingInfo.PlacementMode.Roadside)
            {
                vector *= (float)info.m_cellWidth * 4f - 0.8f;
                vector2 *= (float)info.m_cellLength * 4f - 0.8f;
            }
            else
            {
                vector *= (float)info.m_cellWidth * 4f;
                vector2 *= (float)info.m_cellLength * 4f;
            }
            if (info.m_circular)
            {
                vector *= 0.7f;
                vector2 *= 0.7f;
            }
            ItemClass.CollisionType collisionType = ItemClass.CollisionType.Terrain;
            if (info.m_class.m_layer == ItemClass.Layer.WaterPipes)
            {
                collisionType = ItemClass.CollisionType.Underground;
            }
            Vector2 a = VectorUtils.XZ(position);
            Quad2 quad = default(Quad2);
            quad.a = a - vector - vector2;
            quad.b = a - vector + vector2;
            quad.c = a + vector + vector2;
            quad.d = a + vector - vector2;
            float minY = Mathf.Min(position.y, Singleton<TerrainManager>.instance.SampleRawHeightSmooth(position));
            float maxY = position.y + info.m_generatedInfo.m_size.y;
            return !Singleton<NetManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, info.m_class.m_layer, nodeID, 0, 0, null) && !Singleton<BuildingManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, info.m_class.m_layer, 0, nodeID, 0, null);*/
        }
    }
}
