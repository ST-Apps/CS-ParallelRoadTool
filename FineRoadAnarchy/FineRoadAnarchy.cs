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
        
        public ToolController m_toolController;
        public NetTool m_netTool;

        private OptionsPanel m_panel;

        private int m_tries;

        public void Start()
        {
            try
            {
                m_toolController = GameObject.Find("Tool Controller").GetComponent<ToolController>();
                m_netTool = m_toolController.GetComponent<NetTool>();

                m_tries = 0;
                bendingPrefabs.Clear();

                int count = PrefabCollection<NetInfo>.PrefabCount();
                for (uint i = 0; i < count; i++)
                {
                    NetInfo prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                    if (prefab != null)
                    {
                        if (prefab.m_enableBendingSegments)
                        {
                            bendingPrefabs.Add(prefab);
                        }
                    }
                }
                Redirector<NetInfoDetour>.Deploy();

                m_panel = UIView.GetAView().AddUIComponent(typeof(OptionsPanel)) as OptionsPanel;

                DebugUtils.Log("Initialized");
            }
            catch(Exception e)
            {
                DebugUtils.Log("Start failed");
                DebugUtils.LogException(e);
            }
        }
        
        public void Update()
        {
            try
            {
                if (m_netTool.m_prefab != null)
                {
                    if (m_netTool.enabled)
                    {
                        CanCollide(m_netTool.m_prefab, !anarchy);
                    }
                    else
                    {
                        CanCollide(m_netTool.m_prefab, true);
                    }
                }

                if (m_tries < 5)
                {
                    UIPanel frtPanel = UIView.GetAView().FindUIComponent<UIPanel>("FRT_ToolOptionsPanel");

                    if (frtPanel != null)
                    {
                        frtPanel.height += m_panel.height + 8;

                        frtPanel.AttachUIComponent(m_panel.gameObject);
                        m_panel.relativePosition = new Vector3(8, frtPanel.height - m_panel.height - 8);
                        m_panel.width = frtPanel.width - 16;

                        frtPanel.GetComponentInChildren<UIDragHandle>().height = frtPanel.height;

                        m_tries = 5;
                    }

                    m_tries++;
                }
                else if (m_tries == 5)
                {
                    UIMainWindow window = UIView.GetAView().AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;

                    window.AttachUIComponent(m_panel.gameObject);
                    window.size = new Vector2(228, 180);
                    m_panel.relativePosition = new Vector3(8, 28);
                    m_panel.width = window.width - 16;

                    window.height = 36 + m_panel.height;

                    m_tries++;
                }
            }
            catch (Exception e)
            {
                m_tries = 6;

                DebugUtils.Log("Update failed");
                DebugUtils.LogException(e);
            }
        }

        public void OnDestroy()
        {
            Redirector<NetInfoDetour>.Revert();
            anarchy = false;
        }

        public void CanCollide(NetInfo prefab, bool value)
        {
            if(prefab != null)
            {
                prefab.m_canCollide = value;

                RoadAI roadAI = prefab.m_netAI as RoadAI;
                if(roadAI != null)
                {
                    if (roadAI.m_elevatedInfo != null) roadAI.m_elevatedInfo.m_canCollide = value;
                    if (roadAI.m_bridgeInfo != null) roadAI.m_bridgeInfo.m_canCollide = value;
                    if (roadAI.m_tunnelInfo != null) roadAI.m_tunnelInfo.m_canCollide = value;
                    if (roadAI.m_slopeInfo != null) roadAI.m_slopeInfo.m_canCollide = value;
                }
            }
        }

        public bool anarchy
        {
            get
            {
                return Redirector<NetToolDetour>.IsDeployed();
            }

            set
            {
                if(anarchy != value)
                {
                    if(value)
                    {
                        DebugUtils.Log("Enabling anarchy");
                        Redirector<NetToolDetour>.Deploy();
                        Redirector<BuildingToolDetour>.Deploy();
                        Redirector<RoadAIDetour>.Deploy();
                        Redirector<PedestrianPathAIDetour>.Deploy();
                        Redirector<TrainTrackAIDetour>.Deploy();
                    }
                    else
                    {
                        DebugUtils.Log("Disabling anarchy");
                        Redirector<NetToolDetour>.Revert();
                        Redirector<BuildingToolDetour>.Revert();
                        Redirector<RoadAIDetour>.Revert();
                        Redirector<PedestrianPathAIDetour>.Revert();
                        Redirector<TrainTrackAIDetour>.Revert();
                    }
                }
            }
        }

        public bool bending
        {
            get
            {
                return bendingPrefabs.m_size > 0 && bendingPrefabs.m_buffer[0].m_enableBendingSegments;
            }

            set
            {
                if (bending != value)
                {
                    for (int i = 0; i < bendingPrefabs.m_size; i++)
                    {
                        bendingPrefabs.m_buffer[i].m_enableBendingSegments = value;
                    }
                }
            }
        }


        public bool snapping
        {
            set;
            get;
        }

        public void OnGUI()
        {
            try
            {
                if (!m_toolController.IsInsideUI && Cursor.visible)
                {
                    Event e = Event.current;

                    // Checking key presses
                    if (OptionsKeymapping.toggleAnarchy.IsPressed(e))
                    {
                        m_panel.m_anarchy.SimulateClick();
                    }
                    else if (OptionsKeymapping.toggleBending.IsPressed(e))
                    {
                        m_panel.m_bending.SimulateClick();
                    }
                    else if (OptionsKeymapping.toggleSnapping.IsPressed(e))
                    {
                        m_panel.m_snapping.SimulateClick();
                    }
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