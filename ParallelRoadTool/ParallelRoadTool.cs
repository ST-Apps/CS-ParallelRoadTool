using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.Detours;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI;
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

        public static bool IsInGameMode;

        public List<NetInfo> AvailableRoadTypes { get; private set; }
        public List<NetTypeItem> SelectedRoadTypes { get; private set; }
        public string[] AvailableRoadNames;

        public bool IsSnappingEnabled { get; private set; }
        public bool IsLeftHandTraffic { get; private set; }

        private bool _isToolActive;

        public bool IsToolActive
        {
            get => _isToolActive
                   && ToolsModifierControl.GetTool<NetTool>() != null
                   && ToolsModifierControl.GetTool<NetTool>().enabled;

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
            try
            {
                // Find NetTool and deploy
                if (ToolsModifierControl.GetTool<NetTool>() == null)
                {
                    DebugUtils.Log("Net Tool not found");
                    enabled = false;
                    return;
                }

                DebugUtils.Log("Loading PRT...");

                // Init support data
                var count = PrefabCollection<NetInfo>.PrefabCount();
                AvailableRoadNames = new string[count + 1];
                AvailableRoadTypes = new List<NetInfo>();
                SelectedRoadTypes = new List<NetTypeItem>();
                IsSnappingEnabled = false;
                IsLeftHandTraffic = Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic ==
                                    SimulationMetaData.MetaBool.True;
                IsToolActive = false;

                // Available networks loading
                DebugUtils.Log("Loading all available networks...");
                // Default item, creates a net with the same type as source
                AddNetworkType(null);
                // HACK - [ISSUE-64] before being able to sort we need to generate names, so we use a SortedDictionary for the first pass
                var sortedNetworks = new SortedDictionary<string, NetInfo>();
                for (uint i = 0; i < count; i++)
                {
                    var prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                    if (prefab != null) sortedNetworks[prefab.GenerateBeautifiedNetName()] = prefab;
                }

                Array.Copy(sortedNetworks.Keys.ToArray(), 0, AvailableRoadNames, 1, sortedNetworks.Count);
                AvailableRoadTypes.AddRange(sortedNetworks.Values.ToList());

                DebugUtils.Log($"Loaded {AvailableRoadTypes.Count} networks.");

                // Main UI init
                DebugUtils.Log("Adding UI components");
                var view = UIView.GetAView();
                _mainWindow = _mainWindow ??
                              view.FindUIComponent<UIMainWindow>($"{Configuration.ResourcePrefix}MainWindow");
                if (_mainWindow != null)
                    Destroy(_mainWindow);
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

                //remove existing autosave 
                if (File.Exists(Configuration.AutoSaveFilePath))
                    File.Delete(Configuration.AutoSaveFilePath);
                //save current networks 
                DebugUtils.Log("Saving networks");
                PresetsUtils.Export(Configuration.AutoSaveFileName);

                ToggleDetours(false);
                UnsubscribeToUIEvents();

                // Reset data structures
                AvailableRoadTypes.Clear();
                SelectedRoadTypes.Clear();
                IsSnappingEnabled = false;
                IsLeftHandTraffic = false;

                // Destroy UI
                Destroy(_mainWindow);
                _mainWindow = null;
                DebugUtils.Log("Destroyed! ...");
            }
            catch (Exception e)
            {
                // HACK - [ISSUE 31]
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

        private void AddNetworkType(NetInfo net)
        {
            AvailableRoadNames[AvailableRoadTypes.Count] = net.GenerateBeautifiedNetName();
            AvailableRoadTypes.Add(net);
        }

        public void ClearItems()
        {
            _mainWindow.ClearItems();
        }

        public void AddItem(NetTypeItem item)
        {
            _mainWindow.AddItem(item);
        }

        #endregion

        #region Handlers

        private void UnsubscribeToUIEvents()
        {
            _mainWindow.OnParallelToolToggled -= MainWindowOnOnParallelToolToggled;
            _mainWindow.OnSnappingToggled -= MainWindowOnOnSnappingToggled;
            _mainWindow.OnHorizontalOffsetKeypress -= MainWindowOnOnHorizontalOffsetKeypress;
            _mainWindow.OnVerticalOffsetKeypress -= MainWindowOnOnVerticalOffsetKeypress;

            _mainWindow.OnNetworkItemAdded -= MainWindowOnNetworkItemAdded;
            _mainWindow.OnItemChanged -= MainWindowOnOnItemChanged;
        }

        private void SubscribeToUIEvents()
        {
            _mainWindow.OnParallelToolToggled += MainWindowOnOnParallelToolToggled;
            _mainWindow.OnSnappingToggled += MainWindowOnOnSnappingToggled;
            _mainWindow.OnHorizontalOffsetKeypress += MainWindowOnOnHorizontalOffsetKeypress;
            _mainWindow.OnVerticalOffsetKeypress += MainWindowOnOnVerticalOffsetKeypress;

            _mainWindow.OnNetworkItemAdded += MainWindowOnNetworkItemAdded;
            _mainWindow.OnItemChanged += MainWindowOnOnItemChanged;
            _mainWindow.OnNetworkItemDeleted += MainWindowOnOnNetworkItemDeleted;
        }

        private void MainWindowOnOnVerticalOffsetKeypress(UIComponent component, float step)
        {
            for (var i = 0; i < SelectedRoadTypes.Count; i++)
            {
                var item = SelectedRoadTypes[i];
                item.VerticalOffset += (1 + i) * step;
                _mainWindow.UpdateItem(item, i);
            }
        }

        private void MainWindowOnOnHorizontalOffsetKeypress(UIComponent component, float step)
        {
            for (var i = 0; i < SelectedRoadTypes.Count; i++)
            {
                var item = SelectedRoadTypes[i];
                item.HorizontalOffset += (1 + i) * step;
                _mainWindow.UpdateItem(item, i);
            }
        }

        private void MainWindowOnOnSnappingToggled(UIComponent component, bool value)
        {
            IsSnappingEnabled = value;
        }

        private void MainWindowOnOnParallelToolToggled(UIComponent component, bool value)
        {
            IsToolActive = value;

            /*
            if (value && ToolsModifierControl.advisorPanel.isVisible && ToolsModifierControl.advisorPanel.isOpen)
                _mainWindow.ShowTutorial();
            */
        }

        private void MainWindowOnNetworkItemAdded(object sender, EventArgs e)
        {
            // Previous item's offset so that we can try to separate this one from previous one without overlapping
            var prevOffset = SelectedRoadTypes.Any() ? SelectedRoadTypes.Last().HorizontalOffset : 0;
            var netInfo = ToolsModifierControl.GetTool<NetTool>().Prefab;
            var item = new NetTypeItem(netInfo, prevOffset + netInfo.m_halfWidth * 2, 0, false);
            SelectedRoadTypes.Add(item);

            _mainWindow.AddItem(item);
            NetManagerDetour.NetworksCount = SelectedRoadTypes.Count;
        }

        private void MainWindowOnOnItemChanged(UIComponent component, NetTypeItemEventArgs value)
        {
            DebugUtils.Log($"{value.ItemIndex} / {SelectedRoadTypes.Count}");
            var item = SelectedRoadTypes[value.ItemIndex];
            var netInfo = value.SelectedNetworkIndex == 0
                ? null
                : Singleton<ParallelRoadTool>.instance.AvailableRoadTypes[value.SelectedNetworkIndex];

            item.NetInfo = netInfo;
            item.HorizontalOffset = value.HorizontalOffset;
            item.VerticalOffset = value.VerticalOffset;
            item.IsReversed = value.IsReversedNetwork;
        }

        private void MainWindowOnOnNetworkItemDeleted(UIComponent component, int index)
        {
            SelectedRoadTypes.RemoveAt(index);
            _mainWindow.DeleteItem(index);

            NetManagerDetour.NetworksCount = SelectedRoadTypes.Count;
        }

        #endregion
    }
}