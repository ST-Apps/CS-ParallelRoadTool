using ICities;
using UnityEngine;

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;

using FineRoadAnarchy.Redirection;

namespace FineRoadAnarchy.Detours
{
    [TargetType(typeof(BuildingTool))]
    public class BuildingToolDetour : BuildingTool
    {
        [RedirectMethod]
        new public static ToolBase.ToolErrors CheckSpace(BuildingInfo info, BuildingInfo.PlacementMode placementMode, int relocating, Vector3 pos, float minY, float maxY, float angle, int width, int length, bool test, ulong[] collidingSegmentBuffer, ulong[] collidingBuildingBuffer)
        {
            return ToolBase.ToolErrors.None;
            /*ToolBase.ToolErrors toolErrors = BuildingTool.CheckSpaceImpl(info, placementMode, relocating, pos, minY, maxY, angle, width, length, test, collidingSegmentBuffer, collidingBuildingBuffer);
            if (info.m_subBuildings != null && info.m_subBuildings.Length != 0)
            {
                Matrix4x4 matrix4x = default(Matrix4x4);
                matrix4x.SetTRS(pos, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), Vector3.one);
                for (int i = 0; i < info.m_subBuildings.Length; i++)
                {
                    BuildingInfo buildingInfo = info.m_subBuildings[i].m_buildingInfo;
                    Vector3 vector = matrix4x.MultiplyPoint(info.m_subBuildings[i].m_position);
                    float angle2 = info.m_subBuildings[i].m_angle * 0.0174532924f + angle;
                    float minY2;
                    float maxY2;
                    float num;
                    Building.SampleBuildingHeight(vector, angle2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, buildingInfo, out minY2, out maxY2, out num);
                    toolErrors |= BuildingTool.CheckSpaceImpl(buildingInfo, placementMode, relocating, vector, minY2, maxY2, angle2, buildingInfo.m_cellWidth, buildingInfo.m_cellLength, test, collidingSegmentBuffer, collidingBuildingBuffer);
                }
            }
            if (!test)
            {
                NetTool.ReleaseNonImportantSegments(collidingSegmentBuffer);
                BuildingTool.ReleaseNonImportantBuildings(collidingBuildingBuffer);
            }
            return toolErrors;*/
        }

        [RedirectMethod]
        new public static bool CheckCollidingBuildings(BuildingInfo other, ulong[] buildingMask, ulong[] segmentMask)
        {
            return false;

            /*BuildingManager instance = Singleton<BuildingManager>.instance;
            int num = buildingMask.Length;
            bool result = false;
            for (int i = 0; i < num; i++)
            {
                ulong num2 = buildingMask[i];
                if (num2 != 0uL)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        if ((num2 & 1uL << j) != 0uL)
                        {
                            int num3 = i << 6 | j;
                            BuildingInfo info = instance.m_buildings.m_buffer[num3].Info;
                            if (other != null && info.m_buildingAI.AllowOverlap(other) && other.m_buildingAI.AllowOverlap(info))
                            {
                                num2 &= ~(1uL << j);
                            }
                            else if ((instance.m_buildings.m_buffer[num3].m_flags & Building.Flags.Untouchable) != Building.Flags.None)
                            {
                                if (BuildingTool.CheckParentNode((ushort)num3, buildingMask, segmentMask))
                                {
                                    result = true;
                                }
                            }
                            else if (BuildingTool.IsImportantBuilding(info, (ushort)num3))
                            {
                                result = true;
                            }
                        }
                    }
                    buildingMask[i] = num2;
                }
            }
            return result;*/
        }

    }
}