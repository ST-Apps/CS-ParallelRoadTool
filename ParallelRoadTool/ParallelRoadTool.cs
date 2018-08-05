using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using ParallelRoadTool.Detours;
using ParallelRoadTool.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Base;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool
{
    /// <summary>
    ///     Mod's main controller and data storage.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ParallelRoadTool : MonoBehaviour
    {
        #region Properties

        #region Data

        public readonly List<NetInfo> AvailableRoadTypes = new List<NetInfo>();
        public readonly List<NetTypeItem> SelectedRoadTypes = new List<NetTypeItem>();

        public string[] AvailableRoadNames;

        public static bool IsInGameMode;
        public bool IsSnappingEnabled;
        public bool IsLeftHandTraffic;

        private bool _isToolActive;
        public bool IsToolActive
        {
            get => _isToolActive && ToolsModifierControl.GetTool<NetTool>().enabled;

            private set
            {
                if (IsToolActive == value) return;                
                ToggleDetours(value);
                _isToolActive = value;                
            }
        }        

        #endregion

        #region UI

        private UIMainWindow _mainWindow;

        #endregion

        #endregion        

        #region Unity

        public void Start()
        {
            // Find NetTool and deploy
            try
            {                
                if (ToolsModifierControl.GetTool<NetTool>() == null)
                {
                    DebugUtils.Log("Net Tool not found");
                    enabled = false;
                    return;
                }

                // Available networks loading
                DebugUtils.Log("Loading all available networks.");

                var count = PrefabCollection<NetInfo>.PrefabCount();
                AvailableRoadTypes.Clear();
                AvailableRoadNames = new string[count + 1];

                // Default item, creates a net with the same type as source
                AddNetworkType(null);
                var addedNetworksCount = 1;

                for (uint i = 0; i < count; i++)
                {
                    var prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                    if (prefab != null) AddNetworkType(prefab, addedNetworksCount++);
                }

                DebugUtils.Log($"Loaded {AvailableRoadTypes.Count} networks.");

                for (var i = 0; i < AvailableRoadTypes.Count; i++)
                {
                    DebugUtils.Log($"ROAD: {AvailableRoadTypes[i].GenerateBeautifiedNetName()} | NAME: {AvailableRoadNames[i]}");
                }

                IsLeftHandTraffic = Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic ==
                                    SimulationMetaData.MetaBool.True;

                DebugUtils.Log($"IsLeftHandTraffic = {IsLeftHandTraffic}");

                NetManagerDetour.Deploy();
                NetToolDetour.Deploy();

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
            try
            {
                DebugUtils.Log("Destroying ...");

                NetManagerDetour.Revert();

                UnsubscribeToUIEvents();
                AvailableRoadTypes.Clear();
                SelectedRoadTypes.Clear();
                IsToolActive = false;
                IsSnappingEnabled = false;
                IsLeftHandTraffic = false;
                _mainWindow.OnDestroy();
                _mainWindow = null;
            }
            catch
            {
                // HACK - [ISSUE 31]
            }
        }

        public void OnGUI()
        {
            try
            {
                if (UIView.HasModalInput() || UIView.HasInputFocus() || !IsToolActive) return;
                var e = Event.current;

                // Checking key presses
                if (OptionsKeymapping.toggleParallelRoads.IsPressed(e)) _mainWindow.ToggleToolCheckbox();

                if (OptionsKeymapping.decreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(-1f);

                if (OptionsKeymapping.increaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(1f);

                if (OptionsKeymapping.decreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(-1f, false);

                if (OptionsKeymapping.increaseVerticalOffset.IsPressed(e)) AdjustNetOffset(1f, false);
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }

        #endregion

        #region Utils

        private static void ToggleDetours(bool toolEnabled)
        {
            if (toolEnabled)
            {
                DebugUtils.Log("Enabling parallel road support");
                NetManagerDetour.Deploy();
                NetToolDetour.Deploy();
            }
            else
            {
                DebugUtils.Log("Disabling parallel road support");
                NetManagerDetour.Revert();
                NetToolDetour.Revert();
            }
        }

        private void AdjustNetOffset(float step, bool isHorizontal = true)
        {
            // Adjust all offsets on keypress
            var index = 0;
            foreach (var item in SelectedRoadTypes)
            {
                if (isHorizontal)
                    item.HorizontalOffset += (1 + index) * step;
                else
                    item.VerticalOffset += (1 + index) * step;
                index++;
            }

            _mainWindow.RenderNetList();
        }
        
        private void AddNetworkType(NetInfo net, int index = 0)
        {
            AvailableRoadTypes.Add(net);
            AvailableRoadNames[index] = net.GenerateBeautifiedNetName();
        }

        #endregion

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
            _mainWindow.OnSnappingToggled += MainWindowOnOnSnappingToggled;
        }

        private void MainWindowOnOnSnappingToggled(UIComponent component, bool value)
        {
            IsSnappingEnabled = value;
        }

        private void MainWindowOnOnNetworksListCountChanged(object sender, System.EventArgs e)
        {
            NetManagerDetour.NetworksCount = SelectedRoadTypes.Count;
        }

        private void MainWindowOnOnParallelToolToggled(UIComponent component, bool value)
        {
            IsToolActive = value;

            if (value && ToolsModifierControl.advisorPanel.isVisible && ToolsModifierControl.advisorPanel.isOpen)
                _mainWindow.ShowTutorial();
        }

        #endregion        
    }
}
