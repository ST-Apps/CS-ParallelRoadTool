using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using ICities;
using ParallelRoadTool.Detours;
using ParallelRoadTool.UI;
using UnityEngine;

namespace ParallelRoadTool
{
    /// <summary>
    ///     Mod's "launcher" class.
    ///     It also acts as a "controller" to connect the mod with its UI.
    ///
    /// TODO: Drag & drop is not as smooth as before.
    /// TODO: Window should be also visible when a network is selected, we can't rely on shortcuts only.
    /// TODO: Removing a network while continuing a segment leads to wrong connections.
    /// TODO: When updating a road with the tool enabled, weird stuff happens. We must disable the tool during upgrade.
    /// TODO: Move UI to an event-based system again.
    /// </summary>
    public class ParallelRoadTool : MonoBehaviour
    {
        public const string SettingsFileName = "ParallelRoadTool";

        public static ParallelRoadTool Instance;

        public static List<NetInfo> AvailableRoadTypes = new List<NetInfo>();
        public static List<NetTypeItem> SelectedRoadTypes = new List<NetTypeItem>();

        private UIOptionsPanel _mainPanel;
        private UIMainWindow _mainWindow;
        private UINetList _netList;

        public NetTool NetTool;
        public NetInfo NetToolSelection;

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

        public bool IsToolActive()
        {
            return _mainPanel.ToolToggleButton.isChecked && NetTool.enabled;
        }

        private void AdjustNetOffset(float step)
        {
            // Adjust all offsets on keypress
            var index = 0;
            foreach (var item in SelectedRoadTypes)
            {
                item.HorizontalOffset += (1 + index) * step;
                index++;
            }

            _netList.RenderList();
        }

        #region Unity

        public void Start()
        {
            // Main UI init
            var view = UIView.GetAView();
            _mainWindow = view.FindUIComponent<UIMainWindow>("PRT_MainWindow");
            if (_mainWindow != null)
                Destroy(_mainWindow);

            DebugUtils.Log("Adding UI components");
            _mainWindow = view.AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;
            if (_mainWindow != null)
            {
                _mainPanel = _mainWindow.AddUIComponent(typeof(UIOptionsPanel)) as UIOptionsPanel;
                _netList = _mainWindow.AddUIComponent(typeof(UINetList)) as UINetList;
                if (_netList != null)
                {
                    _netList.List = SelectedRoadTypes;
                    _netList.OnChangedCallback = () =>
                    {
                        DebugUtils.Log($"_netList.OnChangedCallback (selected {SelectedRoadTypes.Count})");
                        NetManagerDetour.NetworksCount = SelectedRoadTypes.Count;
                    };
                }

                //_netList.RenderList();

                var space = _mainWindow.AddUIComponent<UIPanel>();
                space.size = new Vector2(1, 1);
            }

            // Find NetTool and deploy
            try
            {
                NetTool = FindObjectOfType<NetTool>();
                if (NetTool == null)
                {
                    DebugUtils.Log("Net Tool not found");
                    enabled = false;
                    return;
                }

                // Available networks loading
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

                DebugUtils.Log("Initialized");
            }
            catch (Exception e)
            {
                DebugUtils.Log("Start failed");
                DebugUtils.LogException(e);
                enabled = false;
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
                    _mainPanel.ToolToggleButton.isChecked = !_mainPanel.ToolToggleButton.isChecked;

                if (OptionsKeymapping.decreaseOffset.IsPressed(e)) AdjustNetOffset(-1f);
                if (OptionsKeymapping.increaseOffset.IsPressed(e)) AdjustNetOffset(1f);

                var currentSelectedNetwork = NetTool.m_prefab;
                if (NetToolSelection == currentSelectedNetwork) return;
                NetToolSelection = currentSelectedNetwork;
                _netList.UpdateCurrrentTool(NetToolSelection);
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }

        #endregion
    }

    public class ParallelRoadToolLoader : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            // Reload mod if re-created after level has been loaded. For development
            /*if (loading.loadingComplete)
            {
                ParallelRoadTool.instance = new GameObject("ParallelRoadTool").AddComponent<ParallelRoadTool>();
            }*/
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (ParallelRoadTool.Instance == null)
                ParallelRoadTool.Instance = new GameObject("ParallelRoadTool").AddComponent<ParallelRoadTool>();
            else
                ParallelRoadTool.Instance.Start();
        }
    }
}