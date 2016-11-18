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

namespace FineRoadAnarchy
{
    public class FineRoadAnarchy : MonoBehaviour
    {
        public const string settingsFileName = "FineRoadAnarchy";

        public static FineRoadAnarchy instance;

        public static FastList<NetInfo> bendingPrefabs = new FastList<NetInfo>();

        public static bool bendingEnabled = true;
        public static bool snappingEnabled = true;

        public void Start()
        {
            bendingPrefabs.Clear();
            int count = PrefabCollection<NetInfo>.PrefabCount();
            for (uint i = 0; i < count; i++)
            {
                NetInfo prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                if(prefab != null && prefab.m_enableBendingSegments)
                {
                    bendingPrefabs.Add(prefab);
                }
            }
            Redirector<NetInfoDetour>.Deploy();
        }

        public void OnDestroy()
        {
            Redirector<NetInfoDetour>.Revert();
            DisableAnarchy();
        }

        public void EnableAnarchy()
        {
            Redirector<NetToolDetour>.Deploy();
            Redirector<BuildingToolDetour>.Deploy();
            Redirector<RoadAIDetour>.Deploy();
            Redirector<PedestrianPathAIDetour>.Deploy();
            Redirector<TrainTrackAIDetour>.Deploy();

        }

        public void DisableAnarchy()
        {
            Redirector<NetToolDetour>.Revert();
            Redirector<BuildingToolDetour>.Revert();
            Redirector<RoadAIDetour>.Revert();
            Redirector<PedestrianPathAIDetour>.Revert();
            Redirector<TrainTrackAIDetour>.Revert();
        }

        public void EnableBending()
        {
            for(int i = 0; i<bendingPrefabs.m_size; i++)
            {
                bendingPrefabs.m_buffer[i].m_enableBendingSegments = true;
            }
            bendingEnabled = true;
        }

        public void DisableBending()
        {
            for (int i = 0; i < bendingPrefabs.m_size; i++)
            {
                bendingPrefabs.m_buffer[i].m_enableBendingSegments = false;
            }
            bendingEnabled = false;
        }

        public void OnGUI()
        {
            try
            {
                Event e = Event.current;

                // Checking key presses
                if (OptionsKeymapping.toggleAnarchy.IsPressed(e))
                {
                    if(Redirector<NetToolDetour>.IsDeployed())
                    {
                        DisableAnarchy();
                    }
                    else
                    {
                        EnableAnarchy();
                    }
                }
                else if (OptionsKeymapping.toggleBending.IsPressed(e))
                {
                    if(bendingEnabled)
                    {
                        DisableBending();
                    }
                    else
                    {
                        EnableBending();
                    }
                }
                else if (OptionsKeymapping.toggleSnapping.IsPressed(e))
                {
                    snappingEnabled = !snappingEnabled;
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }
    }

    public class FineRoadAnarchyLoader : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (FineRoadAnarchy.instance == null)
            {
                // Creating the instance
                FineRoadAnarchy.instance = new GameObject("FineRoadAnarchy").AddComponent<FineRoadAnarchy>();
            }
            else
            {
                FineRoadAnarchy.instance.Start();
            }
        }
    }
}