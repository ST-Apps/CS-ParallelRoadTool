using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;
using ICities;
using ParallelRoadTool.Detours;
using ParallelRoadTool.EventArgs;
using ParallelRoadTool.Redirection;
using UnityEngine;

namespace ParallelRoadTool
{
    /// <summary>
    /// Mod's "launcher" class.
    /// It also acts as a "controller" to connect the mod with its UI.
    /// </summary>
    public class ParallelRoadTool : MonoBehaviour
    {
        public const string SettingsFileName = "ParallelRoadTool";
        public const float DefaultHorizontalOffset = 15f;

        public static ParallelRoadTool Instance;

        public static List<NetInfo> AvailableRoadTypes = new List<NetInfo>();
        public static List<Tuple<NetInfo, float>> SelectedRoadTypes = new List<Tuple<NetInfo, float>>();

        public NetTool NetTool { get; private set; }

        private OptionsPanel _mainPanel;
        private UIMainWindow _mainWindow;        

        public static bool IsParallelEnabled
        {
            get => NetManagerDetour.IsDeployed();

            set
            {                
                if (IsParallelEnabled == value) return;
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

        #region Unity

        public void Start()
        {
            try
            {
                NetTool = FindObjectOfType<NetTool>();
                if (NetTool == null)
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

                if (_mainPanel == null)
                {
                    _mainPanel = UIView.GetAView().AddUIComponent(typeof(OptionsPanel)) as OptionsPanel;
                }
                else
                {
                    _mainPanel.m_parallel.isChecked = false;
                }

                SubscribeToUiEvents();

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
                if (_mainWindow == null)
                {
                    DebugUtils.Log("Parallel Road Tool window not found");

                    _mainWindow = UIView.GetAView().AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;

                    if (_mainWindow == null) return;

                    _mainWindow.AttachUIComponent(_mainPanel.gameObject);
                    _mainWindow.size = new Vector2(450, 180);
                    _mainPanel.relativePosition = new Vector3(8, 28);                    
                }

                _mainPanel.width = _mainWindow.width - 16;
                _mainWindow.height = 36 + _mainPanel.height;
            }
            catch (Exception e)
            {
                DebugUtils.Log("Update failed");
                DebugUtils.LogException(e);
            }
        }

        public void OnDestroy()
        {
            NetManagerDetour.Revert();
            UnsubscribeToUiEvents();
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
                    _mainPanel.m_parallel.isChecked = !_mainPanel.m_parallel.isChecked;
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }

        #endregion

        #region Events

        private void SubscribeToUiEvents()
        {
            _mainPanel.ParallelToolToggled += MainPanelOnOnParallelToolToggled;
            _mainPanel.NetworksConfigurationChanged += MainPanelOnOnNetworksConfigurationChanged;
        }

        private void UnsubscribeToUiEvents()
        {
            _mainPanel.ParallelToolToggled -= MainPanelOnOnParallelToolToggled;
            _mainPanel.NetworksConfigurationChanged -= MainPanelOnOnNetworksConfigurationChanged;
        }

        private void MainPanelOnOnNetworksConfigurationChanged(object sender, NetworksConfigurationChangedEventArgs e)
        {
            DebugUtils.Log("ParallelRoadTool.MainPanelOnOnNetworksConfigurationChanged()");
            SelectedRoadTypes = e.NetworkConfigurations.ToList();            
            NetManagerDetour.NetworksCount = SelectedRoadTypes.Count;
        }

        private void MainPanelOnOnParallelToolToggled(object sender, ParallelToolToggledEventArgs e)
        {
            DebugUtils.Log("ParallelRoadTool.MainPanelOnOnParallelToolToggled()");
            IsParallelEnabled = e.IsEnabled;
            _mainPanel.ToggleDropdowns(IsParallelEnabled);
        }

        #endregion
    }

    public class ParallelRoadToolLoader : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            if (ParallelRoadTool.Instance == null)
                ParallelRoadTool.Instance = new GameObject("ParallelRoadTool").AddComponent<ParallelRoadTool>();
            else
                ParallelRoadTool.Instance.Start();

            if (mode != LoadMode.LoadAsset && mode != LoadMode.NewAsset) return;

            GameAreaManager.instance.m_maxAreaCount =
                GameAreaManager.AREAGRID_RESOLUTION * GameAreaManager.AREAGRID_RESOLUTION;
            for (var i = 0; i < GameAreaManager.instance.m_maxAreaCount; i++)
                GameAreaManager.instance.m_areaGrid[i] = i + 1;
            GameAreaManager.instance.m_areaCount = GameAreaManager.instance.m_maxAreaCount;
        }
    }
}
