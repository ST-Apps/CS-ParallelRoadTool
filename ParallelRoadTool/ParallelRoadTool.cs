using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using ICities;
using ParallelRoadTool.Detours;
using ParallelRoadTool.Redirection;
using UnityEngine;

namespace ParallelRoadTool
{
    public class ParallelRoadTool : MonoBehaviour
    {
        public const string settingsFileName = "ParallelRoadTool";

        public static ParallelRoadTool instance;

        public static List<NetInfo> AvailableRoadTypes = new List<NetInfo>();

        public static List<Tuple<NetInfo, float>> SelectedRoadTypes = new List<Tuple<NetInfo, float>>();

        public NetTool m_netTool;

        private OptionsPanel m_panel;

        private int m_tries;

        public static bool IsParallelEnabled
        {
            get => NetManagerDetour.IsDeployed();

            set
            {
                if (IsParallelEnabled != value)
                {
                    if (value)
                    {
                        DebugUtils.Log("Enabling parallel road support");
                        NetManagerDetour.Deploy();
                    }
                    else
                    {
                        DebugUtils.Log("Disabling parallel road support");
                        NetManagerDetour.Revert();
                    }
                }
            }
        }

        public static List<NetInfo> SelectedNetworks { get; set; }

        public void Start()
        {
            try
            {
                m_netTool = FindObjectOfType<NetTool>();
                if (m_netTool == null)
                {
                    DebugUtils.Log("Net Tool not found");
                    enabled = false;
                    return;
                }

                DebugUtils.Log("Loading all available networks.");

                AvailableRoadTypes.Clear();

                var count = PrefabCollection<NetInfo>.PrefabCount();

                // Default item, creates a net with the same type as source
                AvailableRoadTypes.Add(null);

                for (uint i = 0; i < count; i++)
                {
                    var prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                    if (prefab != null) AvailableRoadTypes.Add(prefab);
                }

                DebugUtils.Log($"Loaded {AvailableRoadTypes.Count} networks.");

                NetManagerDetour.Deploy();

                if (m_panel == null)
                {
                    m_tries = 0;
                    m_panel = UIView.GetAView().AddUIComponent(typeof(OptionsPanel)) as OptionsPanel;
                }
                else
                {
                    m_panel.m_parallel.isChecked = false;
                }

                DebugUtils.Log("Initialized");
            }
            catch (Exception e)
            {
                DebugUtils.Log("Start failed");
                DebugUtils.LogException(e);
                enabled = false;
            }
        }

        public void Update()
        {
            try
            {
                if (m_tries < 5)
                {
                    var frtPanel = UIView.GetAView().FindUIComponent<UIPanel>("PRT_ToolOptionsPanel");

                    if (frtPanel != null)
                    {
                        DebugUtils.Log("Parallel Road Tool window found");

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
                    DebugUtils.Log("Parallel Road Tool window not found");

                    var window = UIView.GetAView().AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;

                    window.AttachUIComponent(m_panel.gameObject);
                    window.size = new Vector2(400, 180);
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
            NetManagerDetour.Revert();
            IsParallelEnabled = false;
        }

        public void OnGUI()
        {
            try
            {
                if (UIView.HasModalInput() || UIView.HasInputFocus()) return;
                var e = Event.current;

                // Checking key presses
                if (OptionsKeymapping.toggleParallelRoads.IsPressed(e))
                    m_panel.m_parallel.isChecked = !m_panel.m_parallel.isChecked;
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }
    }

    public class ParallelRoadToolLoader : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (ParallelRoadTool.instance == null)
                ParallelRoadTool.instance = new GameObject("ParallelRoadTool").AddComponent<ParallelRoadTool>();
            else
                ParallelRoadTool.instance.Start();

            if (mode == LoadMode.LoadAsset || mode == LoadMode.NewAsset)
            {
                GameAreaManager.instance.m_maxAreaCount =
                    GameAreaManager.AREAGRID_RESOLUTION * GameAreaManager.AREAGRID_RESOLUTION;
                for (var i = 0; i < GameAreaManager.instance.m_maxAreaCount; i++)
                    GameAreaManager.instance.m_areaGrid[i] = i + 1;
                GameAreaManager.instance.m_areaCount = GameAreaManager.instance.m_maxAreaCount;
            }
        }
    }
}