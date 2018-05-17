using UnityEngine;

using ColossalFramework.Math;

using ParallelRoadTool.Redirection;

namespace ParallelRoadTool
{
    [TargetType(typeof(NetTool))]
    public class NetToolDetour : NetTool
    {
        [RedirectMethod]
        private static bool CheckStartAndEnd(ushort upgrading, ushort startSegment, ushort startNode, ushort endSegment, ushort endNode, ulong[] collidingSegmentBuffer)
        {
            return true;

            /*bool result = true;
            if (startSegment != 0 && endSegment != 0)
            {
                NetManager instance = NetManager.instance;
                ushort startNode2 = instance.m_segments.m_buffer[(int)startSegment].m_startNode;
                ushort endNode2 = instance.m_segments.m_buffer[(int)startSegment].m_endNode;
                ushort startNode3 = instance.m_segments.m_buffer[(int)endSegment].m_startNode;
                ushort endNode3 = instance.m_segments.m_buffer[(int)endSegment].m_endNode;
                if (startSegment == endSegment || startNode2 == startNode3 || startNode2 == endNode3 || endNode2 == startNode3 || endNode2 == endNode3)
                {
                    if (collidingSegmentBuffer != null)
                    {
                        collidingSegmentBuffer[startSegment >> 6] |= 1uL << (int)startSegment;
                        collidingSegmentBuffer[endSegment >> 6] |= 1uL << (int)endSegment;
                    }
                    result = false;
                }
            }
            if (startSegment != 0 && endNode != 0)
            {
                NetManager instance2 = NetManager.instance;
                if (instance2.m_segments.m_buffer[(int)startSegment].m_startNode == endNode || instance2.m_segments.m_buffer[(int)startSegment].m_endNode == endNode)
                {
                    if (collidingSegmentBuffer != null)
                    {
                        collidingSegmentBuffer[startSegment >> 6] |= 1uL << (int)startSegment;
                    }
                    result = false;
                }
            }
            if (endSegment != 0 && startNode != 0)
            {
                NetManager instance3 = NetManager.instance;
                if (instance3.m_segments.m_buffer[(int)endSegment].m_startNode == startNode || instance3.m_segments.m_buffer[(int)endSegment].m_endNode == startNode)
                {
                    if (collidingSegmentBuffer != null)
                    {
                        collidingSegmentBuffer[endSegment >> 6] |= 1uL << (int)endSegment;
                    }
                    result = false;
                }
            }
            if (startNode != 0 && endNode != 0 && upgrading == 0)
            {
                NetManager instance4 = NetManager.instance;
                for (int i = 0; i < 8; i++)
                {
                    ushort segment = instance4.m_nodes.m_buffer[(int)startNode].GetSegment(i);
                    if (segment != 0)
                    {
                        ushort startNode4 = instance4.m_segments.m_buffer[(int)segment].m_startNode;
                        ushort endNode4 = instance4.m_segments.m_buffer[(int)segment].m_endNode;
                        if ((startNode4 == startNode && endNode4 == endNode) || (startNode4 == endNode && endNode4 == startNode))
                        {
                            if (collidingSegmentBuffer != null)
                            {
                                collidingSegmentBuffer[segment >> 6] |= 1uL << (int)segment;
                            }
                            result = false;
                        }
                    }
                }
            }
            return result;*/
        }

        [RedirectMethod]
        private static bool CanAddSegment(ushort nodeID, Vector3 direction, ulong[] collidingSegmentBuffer, ushort ignoreSegment)
        {
            return true;

            /*NetNode netNode = NetManager.instance.m_nodes.m_buffer[(int)nodeID];
            bool flag = (netNode.m_flags & NetNode.Flags.Double) != NetNode.Flags.None && ignoreSegment == 0;
            bool result = true;
            if ((netNode.m_flags & (NetNode.Flags.Middle | NetNode.Flags.Untouchable)) == (NetNode.Flags.Middle | NetNode.Flags.Untouchable) && netNode.CountSegments(NetSegment.Flags.Untouchable, ignoreSegment) >= 2)
            {
                flag = true;
            }
            for (int i = 0; i < 8; i++)
            {
                ushort segment = netNode.GetSegment(i);
                if (segment != 0 && segment != ignoreSegment)
                {
                    NetSegment netSegment = NetManager.instance.m_segments.m_buffer[(int)segment];
                    Vector3 vector = (nodeID != netSegment.m_startNode) ? netSegment.m_endDirection : netSegment.m_startDirection;
                    float num = direction.x * vector.x + direction.z * vector.z;
                    if (flag || num > 0.75f)
                    {
                        if (collidingSegmentBuffer != null)
                        {
                            collidingSegmentBuffer[segment >> 6] |= 1uL << (int)segment;
                        }
                        result = false;
                    }
                }
            }
            return result;*/
        }

        [RedirectMethod]
        private static bool CanAddNode(ushort segmentID, Vector3 position, Vector3 direction, bool checkDirection, ulong[] collidingSegmentBuffer)
        {
            return true;

            /*bool flag = true;
            NetSegment netSegment = NetManager.instance.m_segments.m_buffer[(int)segmentID];
            if ((netSegment.m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
            {
                flag = false;
            }
            if (checkDirection)
            {
                Vector3 vector;
                Vector3 vector2;
                netSegment.GetClosestPositionAndDirection(position, out vector, out vector2);
                float num = direction.x * vector2.x + direction.z * vector2.z;
                if (num > 0.75f || num < -0.75f)
                {
                    flag = false;
                }
            }
            if (!NetTool.CanAddNode(segmentID, netSegment.m_startNode, position, direction))
            {
                flag = false;
            }
            if (!NetTool.CanAddNode(segmentID, netSegment.m_endNode, position, direction))
            {
                flag = false;
            }
            if (!flag && collidingSegmentBuffer != null)
            {
                collidingSegmentBuffer[segmentID >> 6] |= 1uL << (int)segmentID;
            }
            return flag;*/
        }

        [RedirectMethod]
        new public static bool CheckCollidingSegments(ulong[] segmentMask, ulong[] buildingMask, ushort upgrading)
        {
            return false;

            /*NetManager instance = Singleton<NetManager>.instance;
            int num = segmentMask.Length;
            bool result = false;
            for (int i = 0; i < num; i++)
            {
                ulong num2 = segmentMask[i];
                if (num2 != 0uL)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        if ((num2 & 1uL << j) != 0uL)
                        {
                            int num3 = i << 6 | j;
                            if (num3 != (int)upgrading)
                            {
                                NetInfo info = instance.m_segments.m_buffer[num3].Info;
                                int publicServiceIndex = ItemClass.GetPublicServiceIndex(info.m_class.m_service);
                                if ((publicServiceIndex != -1 && !info.m_autoRemove) || (instance.m_segments.m_buffer[num3].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None)
                                {
                                    result = true;
                                }
                                else
                                {
                                    NetTool.CheckCollidingNode(instance.m_segments.m_buffer[num3].m_startNode, segmentMask, buildingMask);
                                    NetTool.CheckCollidingNode(instance.m_segments.m_buffer[num3].m_endNode, segmentMask, buildingMask);
                                }
                            }
                            else
                            {
                                segmentMask[num3 >> 6] &= ~(1uL << num3);
                            }
                        }
                    }
                }
            }
            return result;*/
        }

        [RedirectMethod]
        private static ToolBase.ToolErrors CanCreateSegment(NetInfo segmentInfo, ushort startNode, ushort startSegment, ushort endNode, ushort endSegment, ushort upgrading, Vector3 startPos, Vector3 endPos, Vector3 startDir, Vector3 endDir, ulong[] collidingSegmentBuffer, bool testEnds)
        {
            return ToolBase.ToolErrors.None;

            /*ToolBase.ToolErrors toolErrors = ToolBase.ToolErrors.None;
            NetManager instance = Singleton<NetManager>.instance;
            Vector3 b;
            Vector3 b2;
            if (startSegment != 0 && startNode == 0)
            {
                NetInfo info = instance.m_segments.m_buffer[(int)startSegment].Info;
                Vector3 vector;
                Vector3 vector2;
                instance.m_segments.m_buffer[(int)startSegment].GetClosestPositionAndDirection(startPos, out vector, out vector2);
                vector2 = VectorUtils.NormalizeXZ(vector2);
                ushort startNode2 = instance.m_segments.m_buffer[(int)startSegment].m_startNode;
                ushort endNode2 = instance.m_segments.m_buffer[(int)startSegment].m_endNode;
                Vector3 position = instance.m_nodes.m_buffer[(int)startNode2].m_position;
                Vector3 position2 = instance.m_nodes.m_buffer[(int)endNode2].m_position;
                Vector3 startDirection = instance.m_segments.m_buffer[(int)startSegment].m_startDirection;
                Vector3 endDirection = instance.m_segments.m_buffer[(int)startSegment].m_endDirection;
                Vector3 vector3;
                bool flag;
                NetSegment.CalculateCorner(segmentInfo, startPos, endPos, startDir, endDir, info, position, -vector2, startDirection, info, position2, vector2, endDirection, 0, 0, false, true, out b, out vector3, out flag);
                Vector3 vector4;
                NetSegment.CalculateCorner(segmentInfo, startPos, endPos, startDir, endDir, info, position, -vector2, startDirection, info, position2, vector2, endDirection, 0, 0, false, false, out b2, out vector4, out flag);
                toolErrors |= NetTool.CanCreateSegment(startSegment, startNode2, endNode, info, startPos, position, -vector2, startDirection, segmentInfo, endPos, startDir, endDir, collidingSegmentBuffer);
                toolErrors |= NetTool.CanCreateSegment(startSegment, endNode2, endNode, info, startPos, position2, vector2, endDirection, segmentInfo, endPos, startDir, endDir, collidingSegmentBuffer);
            }
            else if (upgrading != 0 || startNode == 0)
            {
                Vector3 vector3;
                bool flag;
                NetSegment.CalculateCorner(segmentInfo, startPos, endPos, startDir, endDir, null, Vector3.zero, Vector3.zero, Vector3.zero, null, Vector3.zero, Vector3.zero, Vector3.zero, upgrading, startNode, false, true, out b, out vector3, out flag);
                Vector3 vector4;
                NetSegment.CalculateCorner(segmentInfo, startPos, endPos, startDir, endDir, null, Vector3.zero, Vector3.zero, Vector3.zero, null, Vector3.zero, Vector3.zero, Vector3.zero, upgrading, startNode, false, false, out b2, out vector4, out flag);
                if (startNode != 0)
                {
                    toolErrors |= NetTool.CanCreateSegment(segmentInfo, startNode, upgrading, endPos, startDir, endDir, collidingSegmentBuffer);
                }
            }
            else
            {
                Vector3 vector3;
                bool flag;
                NetSegment.CalculateCorner(segmentInfo, startPos, endPos, startDir, endDir, segmentInfo, endPos, startDir, endDir, null, Vector3.zero, Vector3.zero, Vector3.zero, 0, startNode, false, true, out b, out vector3, out flag);
                Vector3 vector4;
                NetSegment.CalculateCorner(segmentInfo, startPos, endPos, startDir, endDir, segmentInfo, endPos, startDir, endDir, null, Vector3.zero, Vector3.zero, Vector3.zero, 0, startNode, false, false, out b2, out vector4, out flag);
                if (startNode != 0)
                {
                    toolErrors |= NetTool.CanCreateSegment(segmentInfo, startNode, 0, endPos, startDir, endDir, collidingSegmentBuffer);
                }
            }
            Vector3 a;
            Vector3 a2;
            if (endSegment != 0 && endNode == 0)
            {
                NetInfo info2 = instance.m_segments.m_buffer[(int)endSegment].Info;
                Vector3 vector5;
                Vector3 vector6;
                instance.m_segments.m_buffer[(int)endSegment].GetClosestPositionAndDirection(startPos, out vector5, out vector6);
                vector6 = VectorUtils.NormalizeXZ(vector6);
                ushort startNode3 = instance.m_segments.m_buffer[(int)endSegment].m_startNode;
                ushort endNode3 = instance.m_segments.m_buffer[(int)endSegment].m_endNode;
                Vector3 position3 = instance.m_nodes.m_buffer[(int)startNode3].m_position;
                Vector3 position4 = instance.m_nodes.m_buffer[(int)endNode3].m_position;
                Vector3 startDirection2 = instance.m_segments.m_buffer[(int)endSegment].m_startDirection;
                Vector3 endDirection2 = instance.m_segments.m_buffer[(int)endSegment].m_endDirection;
                Vector3 vector7;
                bool flag2;
                NetSegment.CalculateCorner(segmentInfo, endPos, startPos, endDir, startDir, info2, position3, -vector6, startDirection2, info2, position4, vector6, endDirection2, 0, 0, false, false, out a, out vector7, out flag2);
                Vector3 vector8;
                NetSegment.CalculateCorner(segmentInfo, endPos, startPos, endDir, startDir, info2, position3, -vector6, startDirection2, info2, position4, vector6, endDirection2, 0, 0, false, true, out a2, out vector8, out flag2);
                toolErrors |= NetTool.CanCreateSegment(endSegment, startNode3, startNode, info2, endPos, position3, -vector6, startDirection2, segmentInfo, startPos, endDir, startDir, collidingSegmentBuffer);
                toolErrors |= NetTool.CanCreateSegment(endSegment, endNode3, startNode, info2, endPos, position4, vector6, endDirection2, segmentInfo, startPos, endDir, startDir, collidingSegmentBuffer);
            }
            else if (upgrading != 0 || endNode == 0)
            {
                Vector3 vector7;
                bool flag2;
                NetSegment.CalculateCorner(segmentInfo, endPos, startPos, endDir, startDir, null, Vector3.zero, Vector3.zero, Vector3.zero, null, Vector3.zero, Vector3.zero, Vector3.zero, upgrading, endNode, false, false, out a, out vector7, out flag2);
                Vector3 vector8;
                NetSegment.CalculateCorner(segmentInfo, endPos, startPos, endDir, startDir, null, Vector3.zero, Vector3.zero, Vector3.zero, null, Vector3.zero, Vector3.zero, Vector3.zero, upgrading, endNode, false, true, out a2, out vector8, out flag2);
                if (endNode != 0)
                {
                    toolErrors |= NetTool.CanCreateSegment(segmentInfo, endNode, upgrading, startPos, endDir, startDir, collidingSegmentBuffer);
                }
            }
            else
            {
                Vector3 vector7;
                bool flag2;
                NetSegment.CalculateCorner(segmentInfo, endPos, startPos, endDir, startDir, segmentInfo, startPos, endDir, startDir, null, Vector3.zero, Vector3.zero, Vector3.zero, 0, endNode, false, false, out a, out vector7, out flag2);
                Vector3 vector8;
                NetSegment.CalculateCorner(segmentInfo, endPos, startPos, endDir, startDir, segmentInfo, startPos, endDir, startDir, null, Vector3.zero, Vector3.zero, Vector3.zero, 0, endNode, false, true, out a2, out vector8, out flag2);
                if (endNode != 0)
                {
                    toolErrors |= NetTool.CanCreateSegment(segmentInfo, endNode, 0, startPos, endDir, startDir, collidingSegmentBuffer);
                }
            }
            if ((a.x - b.x) * startDir.x + (a.z - b.z) * startDir.z < 2f)
            {
                toolErrors |= ToolBase.ToolErrors.TooShort;
            }
            if ((b.x - a.x) * endDir.x + (b.z - a.z) * endDir.z < 2f)
            {
                toolErrors |= ToolBase.ToolErrors.TooShort;
            }
            if ((a2.x - b2.x) * startDir.x + (a2.z - b2.z) * startDir.z < 2f)
            {
                toolErrors |= ToolBase.ToolErrors.TooShort;
            }
            if ((b2.x - a2.x) * endDir.x + (b2.z - a2.z) * endDir.z < 2f)
            {
                toolErrors |= ToolBase.ToolErrors.TooShort;
            }
            if (VectorUtils.LengthSqrXZ(a - b) * segmentInfo.m_maxSlope * segmentInfo.m_maxSlope * 4f < (a.y - b.y) * (a.y - b.y))
            {
                toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
            }
            if (VectorUtils.LengthSqrXZ(a2 - b2) * segmentInfo.m_maxSlope * segmentInfo.m_maxSlope * 4f < (a2.y - b2.y) * (a2.y - b2.y))
            {
                toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
            }
            return toolErrors;*/
        }

        [RedirectMethod]
        private static ToolBase.ToolErrors TestNodeBuilding(BuildingInfo info, Vector3 position, Vector3 direction, ushort ignoreNode, ushort ignoreSegment, ushort ignoreBuilding, bool test, ulong[] collidingSegmentBuffer, ulong[] collidingBuildingBuffer)
        {
            return ToolBase.ToolErrors.None;

            /*Vector2 vector = new Vector2(direction.x, direction.z);
            Vector2 vector2 = new Vector2(direction.z, -direction.x);
            if (info.m_placementMode == BuildingInfo.PlacementMode.Roadside)
            {
                vector2 *= (float)info.m_cellWidth * 4f - 0.8f;
                vector *= (float)info.m_cellLength * 4f - 0.8f;
            }
            else
            {
                vector2 *= (float)info.m_cellWidth * 4f;
                vector *= (float)info.m_cellLength * 4f;
            }
            if (info.m_circular)
            {
                vector2 *= 0.7f;
                vector *= 0.7f;
            }
            ItemClass.CollisionType collisionType = ItemClass.CollisionType.Terrain;
            if (info.m_class.m_layer == ItemClass.Layer.WaterPipes)
            {
                collisionType = ItemClass.CollisionType.Underground;
            }
            Vector2 a = VectorUtils.XZ(position);
            Quad2 quad = default(Quad2);
            quad.a = a - vector2 - vector;
            quad.b = a - vector2 + vector;
            quad.c = a + vector2 + vector;
            quad.d = a + vector2 - vector;
            ToolBase.ToolErrors toolErrors = ToolBase.ToolErrors.None;
            float minY = Mathf.Min(position.y, Singleton<TerrainManager>.instance.SampleRawHeightSmooth(position));
            float maxY = position.y + info.m_generatedInfo.m_size.y;
            Singleton<NetManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, info.m_class.m_layer, ignoreNode, 0, ignoreSegment, collidingSegmentBuffer);
            Singleton<BuildingManager>.instance.OverlapQuad(quad, minY, maxY, collisionType, info.m_class.m_layer, ignoreBuilding, ignoreNode, 0, collidingBuildingBuffer);
            if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None)
            {
                float num = 256f;
                if (quad.a.x < -num || quad.a.x > num || quad.a.y < -num || quad.a.y > num)
                {
                    toolErrors |= ToolBase.ToolErrors.OutOfArea;
                }
                if (quad.b.x < -num || quad.b.x > num || quad.b.y < -num || quad.b.y > num)
                {
                    toolErrors |= ToolBase.ToolErrors.OutOfArea;
                }
                if (quad.c.x < -num || quad.c.x > num || quad.c.y < -num || quad.c.y > num)
                {
                    toolErrors |= ToolBase.ToolErrors.OutOfArea;
                }
                if (quad.d.x < -num || quad.d.x > num || quad.d.y < -num || quad.d.y > num)
                {
                    toolErrors |= ToolBase.ToolErrors.OutOfArea;
                }
            }
            else if (Singleton<GameAreaManager>.instance.QuadOutOfArea(quad))
            {
                toolErrors |= ToolBase.ToolErrors.OutOfArea;
            }
            if (!Singleton<BuildingManager>.instance.CheckLimits())
            {
                toolErrors |= ToolBase.ToolErrors.TooManyObjects;
            }
            return toolErrors;*/
        }

        [RedirectMethod]
        private static ToolBase.ToolErrors CheckNodeHeights(NetInfo info, FastList<NetTool.NodePosition> nodeBuffer)
        {
            bool flag = info.m_netAI.BuildUnderground();
            bool flag2 = info.m_netAI.SupportUnderground();
            if (info.m_netAI.LinearMiddleHeight())
            {
                if (nodeBuffer.m_size >= 3)
                {
                    Vector2 b = VectorUtils.XZ(nodeBuffer.m_buffer[0].m_position);
                    Vector2 b2 = VectorUtils.XZ(nodeBuffer.m_buffer[nodeBuffer.m_size - 1].m_position);
                    float y = nodeBuffer.m_buffer[0].m_position.y;
                    float y2 = nodeBuffer.m_buffer[nodeBuffer.m_size - 1].m_position.y;
                    for (int i = 1; i < nodeBuffer.m_size - 1; i++)
                    {
                        NetTool.NodePosition nodePosition = nodeBuffer.m_buffer[i];
                        float num = Vector2.Distance(VectorUtils.XZ(nodePosition.m_position), b);
                        float num2 = Vector2.Distance(VectorUtils.XZ(nodePosition.m_position), b2);
                        nodePosition.m_position.y = Mathf.Lerp(y, y2, num / Mathf.Max(1f, num + num2));
                        nodeBuffer.m_buffer[i] = nodePosition;
                    }
                }

                return ToolBase.ToolErrors.None;
            }
            else
            {
                bool flag3 = false;
                for (int j = 1; j < nodeBuffer.m_size; j++)
                {
                    NetTool.NodePosition nodePosition2 = nodeBuffer.m_buffer[j - 1];
                    NetTool.NodePosition nodePosition3 = nodeBuffer.m_buffer[j];
                    float num4 = VectorUtils.LengthXZ(nodePosition3.m_position - nodePosition2.m_position);
                    float num5 = num4 * info.m_maxSlope;
                    nodePosition3.m_minY = Mathf.Max(nodePosition3.m_minY, nodePosition2.m_minY - num5);
                    nodePosition3.m_maxY = Mathf.Min(nodePosition3.m_maxY, nodePosition2.m_maxY + num5);
                    if (!flag2)
                    {
                        nodePosition3.m_terrainHeight = Mathf.Min(nodePosition3.m_terrainHeight, nodePosition3.m_position.y + 7.98f);
                        nodePosition3.m_minY = Mathf.Max(nodePosition3.m_minY, nodePosition3.m_terrainHeight - 7.99f);
                    }
                    nodeBuffer.m_buffer[j] = nodePosition3;
                }
                for (int k = nodeBuffer.m_size - 2; k >= 0; k--)
                {
                    NetTool.NodePosition nodePosition4 = nodeBuffer.m_buffer[k + 1];
                    NetTool.NodePosition nodePosition5 = nodeBuffer.m_buffer[k];
                    float num6 = VectorUtils.LengthXZ(nodePosition5.m_position - nodePosition4.m_position);
                    float num7 = num6 * info.m_maxSlope;
                    nodePosition5.m_minY = Mathf.Max(nodePosition5.m_minY, nodePosition4.m_minY - num7);
                    nodePosition5.m_maxY = Mathf.Min(nodePosition5.m_maxY, nodePosition4.m_maxY + num7);
                    nodeBuffer.m_buffer[k] = nodePosition5;
                }
                for (int l = 0; l < nodeBuffer.m_size; l++)
                {
                    NetTool.NodePosition nodePosition6 = nodeBuffer.m_buffer[l];
                    if (nodePosition6.m_minY > nodePosition6.m_maxY)
                    {
                        return ToolBase.ToolErrors.None; /*SlopeTooSteep;*/
                    }
                    if (nodePosition6.m_position.y > nodePosition6.m_maxY)
                    {
                        nodePosition6.m_position.y = nodePosition6.m_maxY;
                        if (!flag && nodePosition6.m_elevation >= -8f)
                        {
                            nodePosition6.m_minY = nodePosition6.m_maxY;
                        }
                        flag3 = true;
                    }
                    else if (nodePosition6.m_position.y < nodePosition6.m_minY)
                    {
                        nodePosition6.m_position.y = nodePosition6.m_minY;
                        if (flag || nodePosition6.m_elevation < -8f)
                        {
                            nodePosition6.m_maxY = nodePosition6.m_minY;
                        }
                        flag3 = true;
                    }
                    nodeBuffer.m_buffer[l] = nodePosition6;
                }
                if (nodeBuffer.m_size << 1 == 0)
                {
                    return ToolBase.ToolErrors.None; /*SlopeTooSteep;*/
                }
                if (!flag3)
                {
                    for (int m = 1; m < nodeBuffer.m_size - 1; m++)
                    {
                        NetTool.NodePosition nodePosition7 = nodeBuffer.m_buffer[m - 1];
                        NetTool.NodePosition nodePosition8 = nodeBuffer.m_buffer[m];
                        float num8 = VectorUtils.LengthXZ(nodePosition8.m_position - nodePosition7.m_position);
                        float num9 = num8 * info.m_maxSlope;
                        if (flag || nodePosition8.m_elevation < -8f)
                        {
                            if (nodePosition8.m_position.y > nodePosition7.m_position.y + num9)
                            {
                                nodePosition8.m_position.y = nodePosition7.m_position.y + num9;
                            }
                        }
                        else if (nodePosition8.m_position.y < nodePosition7.m_position.y - num9)
                        {
                            nodePosition8.m_position.y = nodePosition7.m_position.y - num9;
                        }
                        nodeBuffer.m_buffer[m] = nodePosition8;
                    }
                    for (int n = nodeBuffer.m_size - 2; n > 0; n--)
                    {
                        NetTool.NodePosition nodePosition9 = nodeBuffer.m_buffer[n + 1];
                        NetTool.NodePosition nodePosition10 = nodeBuffer.m_buffer[n];
                        float num10 = VectorUtils.LengthXZ(nodePosition10.m_position - nodePosition9.m_position);
                        float num11 = num10 * info.m_maxSlope;
                        if (flag || nodePosition10.m_elevation < -8f)
                        {
                            if (nodePosition10.m_position.y > nodePosition9.m_position.y + num11)
                            {
                                nodePosition10.m_position.y = nodePosition9.m_position.y + num11;
                            }
                        }
                        else if (nodePosition10.m_position.y < nodePosition9.m_position.y - num11)
                        {
                            nodePosition10.m_position.y = nodePosition9.m_position.y - num11;
                        }
                        nodeBuffer.m_buffer[n] = nodePosition10;
                    }
                    int num12;
                    int num13;
                    info.m_netAI.GetElevationLimits(out num12, out num13);
                    if (num13 > num12 && !flag)
                    {
                        int num15;
                        for (int num14 = 0; num14 < nodeBuffer.m_size - 1; num14 = num15)
                        {
                            NetTool.NodePosition nodePosition11 = nodeBuffer.m_buffer[num14];
                            num15 = num14 + 1;
                            float num16 = 0f;
                            bool flag4 = nodeBuffer.m_buffer[num15].m_position.y >= nodeBuffer.m_buffer[num15].m_terrainHeight + 8f;
                            bool flag5 = nodeBuffer.m_buffer[num15].m_position.y <= nodeBuffer.m_buffer[num15].m_terrainHeight - 8f;
                            if (!flag2)
                            {
                                flag5 = false;
                            }
                            if (flag4 || flag5)
                            {
                                while (num15 < nodeBuffer.m_size)
                                {
                                    NetTool.NodePosition nodePosition12 = nodeBuffer.m_buffer[num15];
                                    num16 += VectorUtils.LengthXZ(nodePosition12.m_position - nodePosition11.m_position);
                                    if (flag4 && nodePosition12.m_position.y < nodePosition12.m_terrainHeight + 8f)
                                    {
                                        break;
                                    }
                                    if (flag5 && nodePosition12.m_position.y > nodePosition12.m_terrainHeight - 8f)
                                    {
                                        break;
                                    }
                                    nodePosition11 = nodePosition12;
                                    if (num15 == nodeBuffer.m_size - 1)
                                    {
                                        break;
                                    }
                                    num15++;
                                }
                            }
                            float y3 = nodeBuffer.m_buffer[num14].m_position.y;
                            float y4 = nodeBuffer.m_buffer[num15].m_position.y;
                            nodePosition11 = nodeBuffer.m_buffer[num14];
                            float num17 = 0f;
                            num16 = Mathf.Max(1f, num16);
                            for (int num18 = num14 + 1; num18 < num15; num18++)
                            {
                                NetTool.NodePosition nodePosition13 = nodeBuffer.m_buffer[num18];
                                num17 += VectorUtils.LengthXZ(nodePosition13.m_position - nodePosition11.m_position);
                                if (flag5)
                                {
                                    nodePosition13.m_position.y = Mathf.Min(nodePosition13.m_position.y, Mathf.Lerp(y3, y4, num17 / num16));
                                }
                                else
                                {
                                    nodePosition13.m_position.y = Mathf.Max(nodePosition13.m_position.y, Mathf.Lerp(y3, y4, num17 / num16));
                                }
                                nodeBuffer.m_buffer[num18] = nodePosition13;
                                nodePosition11 = nodePosition13;
                            }
                        }
                    }
                }
                ToolBase.ToolErrors toolErrors = ToolBase.ToolErrors.None;
                for (int num19 = 1; num19 < nodeBuffer.m_size - 1; num19++)
                {
                    NetTool.NodePosition nodePosition14 = nodeBuffer.m_buffer[num19 - 1];
                    NetTool.NodePosition nodePosition15 = nodeBuffer.m_buffer[num19 + 1];
                    NetTool.NodePosition nodePosition16 = nodeBuffer.m_buffer[num19];
                    /*if (flag)
                    {
                        if (nodePosition16.m_terrainHeight < nodePosition16.m_position.y)
                        {
                            toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
                        }
                    }*
                    else if (nodePosition16.m_elevation < -8f)
                    {
                        if (nodePosition16.m_terrainHeight <= nodePosition16.m_position.y + 8f)
                        {
                            toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
                        }
                    }
                    else if (!flag2 && nodePosition16.m_terrainHeight > nodePosition16.m_position.y + 8f)
                    {
                        toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
                    }*/
                    nodePosition16.m_direction.y = VectorUtils.NormalizeXZ(nodePosition15.m_position - nodePosition14.m_position).y;
                    nodeBuffer.m_buffer[num19] = nodePosition16;
                }
                return toolErrors;
            }
        }
    }
}
