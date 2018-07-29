using System;
using System.Collections.Generic;
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
                for (uint i = 0; i < count; i++)
                {
                    var prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                    if (prefab != null) AddNetworkType(prefab);
                }

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
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }

        #endregion

        public bool Export(string filename)
        {
            string path = Path.Combine(SaveFolder, filename + ".xml");
            Directory.CreateDirectory(SaveFolder);

            List<PresetNetItem> PresetItems = SelectedRoadTypes.Select(NetTypeItem => new PresetNetItem {HorizontalOffset = NetTypeItem.HorizontalOffset, IsReversed = NetTypeItem.IsReversed, NetName = NetTypeItem.NetInfo.name, VerticalOffset = NetTypeItem.VerticalOffset}).ToList();

            var xmlSerializer = new XmlSerializer(typeof(List<PresetNetItem>));

            try
            {
                using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(path))
                {
                    xmlSerializer.Serialize(streamWriter, PresetItems);
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("Couldn't export networks");
                DebugUtils.LogException(e);

                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Export failed", "The networks couldn't be exported to '" + path + "'\n\n" + e.Message, true);
                return false;
            }
            return true;
        }

        public void Import(string filename)
        {
            string path = Path.Combine(SaveFolder, filename + ".xml");
            var PresetItems = new List<PresetNetItem>();

            var xmlSerializer = new XmlSerializer(typeof(List<PresetNetItem>));

            try
            {
                using (System.IO.StreamReader streamReader = new System.IO.StreamReader(path))
                {
                    PresetItems = (List<PresetNetItem>)xmlSerializer.Deserialize(streamReader);
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("Couldn't import networks");
                DebugUtils.LogException(e);

                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Import failed", "The networks couldn't be imported from '" + path + "'\n\n" + e.Message, true);
            }

            var netTypeItems = new List<NetTypeItem>();
            SelectedRoadTypes.Clear();
            foreach (PresetNetItem preset in PresetItems)
            {
                NetInfo netInfo;

                netInfo = PrefabCollection<NetInfo>.FindLoaded(preset.NetName);
                if (netInfo != null)
                {
                    DebugUtils.Log("Adding network:" + netInfo.name);
                    var n = new NetTypeItem(netInfo, preset.HorizontalOffset, preset.VerticalOffset, preset.IsReversed);
                    netTypeItems.Add(n);
                    SelectedRoadTypes.Add(n);
                }
                else
                {
                    //TODO action for missing networks needed here
                }
            }
            DebugUtils.Log("Network count: " + netTypeItems.Count);
            //_mainWindow._netList.List.Clear();
            _mainWindow._netList.List = netTypeItems;
            _mainWindow._netList.RenderList();
            _mainWindow._netList.Changed();
        }

        public void Delete(string filename)
        {
            try
            {
                string path = Path.Combine(SaveFolder, filename + ".xml");

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                DebugUtils.Log("Couldn't delete file");
                DebugUtils.LogException(ex);

                return;
            }
        }
    }

        private void AddNetworkType(NetInfo net)
        {
            AvailableRoadNames[AvailableRoadTypes.Count] = net.GenerateBeautifiedNetName();
            AvailableRoadTypes.Add(net);
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

            if (value && ToolsModifierControl.advisorPanel.isVisible && ToolsModifierControl.advisorPanel.isOpen)
                _mainWindow.ShowTutorial();
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