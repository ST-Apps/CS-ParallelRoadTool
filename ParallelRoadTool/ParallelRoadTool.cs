using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using ICities;
using ParallelRoadTool.Detours;
using ParallelRoadTool.UI;
using ParallelRoadTool.UI.Base;
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
    /// TODO: Move UI to an event-based system again.
    /// </summary>
    public class ParallelRoadTool : MonoBehaviour
    {
        public const string SettingsFileName = "ParallelRoadTool";

        public static ParallelRoadTool Instance;

        public static readonly List<NetInfo> AvailableRoadTypes = new List<NetInfo>();
        public static readonly List<NetTypeItem> SelectedRoadTypes = new List<NetTypeItem>();

        private UIMainWindow _mainWindow;                
        private NetTool _netTool;
        private NetInfo _netToolSelection;

        private bool _isToolActive;

        public bool IsToolActive
        {
            get => _isToolActive && _netTool.enabled;
            private set => _isToolActive = value;
        }

        private static bool IsParallelEnabled
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

        #region Handlers

        private void UnsubscribeToUIEvents()
        {
            _mainWindow.OnParallelToolToggled -= MainWindowOnOnParallelToolToggled;
            _mainWindow.OnNetworksListCountChanged -= MainWindowOnOnNetworksListCountChanged;
        }

        private void SubscribeToUIEvents()
        {
            _mainWindow.OnParallelToolToggled += MainWindowOnOnParallelToolToggled;
            _mainWindow.OnNetworksListCountChanged += MainWindowOnOnNetworksListCountChanged;
        }

        private void MainWindowOnOnNetworksListCountChanged(object sender, System.EventArgs e)
        {
            NetManagerDetour.NetworksCount = SelectedRoadTypes.Count;
        }        

        private void MainWindowOnOnParallelToolToggled(UIComponent component, bool value)
        {
            IsParallelEnabled = value;
        }

        #endregion

        #region Utils

        private void AdjustNetOffset(float step)
        {
            // Adjust all offsets on keypress
            var index = 0;
            foreach (var item in SelectedRoadTypes)
            {
                item.HorizontalOffset += (1 + index) * step;
                index++;
            }

            _mainWindow.RenderNetList();            
        }

        #endregion

        #region Unity

        public void Start()
        {
            // Find NetTool and deploy
            try
            {
                _netTool = FindObjectOfType<NetTool>();
                if (_netTool == null)
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

                // Main UI init
                var view = UIView.GetAView();
                _mainWindow = view.FindUIComponent<UIMainWindow>("PRT_MainWindow");
                if (_mainWindow != null)
                    Destroy(_mainWindow);

                DebugUtils.Log("Adding UI components");
                _mainWindow = view.AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;

                SubscribeToUIEvents();

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
            UnsubscribeToUIEvents();
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
                {
                    _mainWindow.ToggleToolCheckbox();
                }

                if (OptionsKeymapping.decreaseOffset.IsPressed(e)) AdjustNetOffset(-1f);

                if (OptionsKeymapping.increaseOffset.IsPressed(e)) AdjustNetOffset(1f);

                var currentSelectedNetwork = _netTool.m_prefab;
                if (_netToolSelection == currentSelectedNetwork) return;
                _netToolSelection = currentSelectedNetwork;
                _mainWindow.UpdateCurrrentTool(_netToolSelection);
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
            // Re-instantiate mod if recompiled after level has been loaded. Useful for UI development, but breaks actual building!
            /*if (loading.loadingComplete)
            {
                ParallelRoadTool.Instance = new GameObject("ParallelRoadTool").AddComponent<ParallelRoadTool>();
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