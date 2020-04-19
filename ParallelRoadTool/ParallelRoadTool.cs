using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ColossalFramework;
using ColossalFramework.UI;
using CSUtil.Commons;
using ICities;
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

        /// <summary>
        ///     <see cref="List{T}" /> containing all the available <see cref="NetInfo" /> objects.
        ///     A <see cref="NetInfo" /> can be any kind of network that the user can build.
        /// </summary>
        public List<NetInfo> AvailableRoadTypes { get; private set; }

        /// <summary>
        ///     <see cref="List{T}" /> containing all the selected <see cref="NetTypeItem" /> objects.
        ///     This contains all the parallel/stacked networks that will be built once a main segment is created.
        /// </summary>
        public List<NetTypeItem> SelectedRoadTypes { get; private set; }

        /// <summary>
        ///     Array containing the beautified names for the <see cref="AvailableRoadTypes" />.
        /// </summary>
        public string[] AvailableRoadNames { get; private set; }

        /// <summary>
        ///     This tells if the user wants the parallel/stacked nodes to snap with already existing nodes.
        /// </summary>
        public bool IsSnappingEnabled { get; private set; }

        /// <summary>
        ///     True if the current map is using left-hand traffic.
        /// </summary>
        public bool IsLeftHandTraffic { get; private set; }

        /// <summary>
        ///     True only if <see cref="AppMode" /> is <see cref="AppMode.Game" />.
        /// </summary>
        public static bool IsInGameMode { get; set; }

        private bool _isToolActive;

        /// <summary>
        ///     Tool is considered active if the user enabled it and if we're currently using <see cref="NetTool" /> to draw a
        ///     network.
        ///     This prevents unnecessary executions while the user is not building networks.
        /// </summary>
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

        /// <summary>
        ///     Currently selected <see cref="NetInfo" /> within <see cref="NetTool" />.
        /// </summary>
        public NetInfo CurrentNetwork => Singleton<NetTool>.instance.m_prefab;

        #endregion

        #region UI

        /// <summary>
        ///     Main UI panel.
        /// </summary>
        private UIMainWindow _mainWindow;

        #endregion

        #endregion

        #region Unity

        /// <summary>
        ///     This method initializes mod's first time loading.
        ///     If <see cref="NetTool" /> is detected we initialize all the support structures, load the available networks and
        ///     finally create the UI.
        /// </summary>
        public void Start()
        {
            try
            {
                // Find NetTool and deploy
                if (ToolsModifierControl.GetTool<NetTool>() == null)
                {
                    Log.Warning($"[{nameof(ParallelRoadTool)}.{nameof(Start)}] Net Tool not found, can't deploy!");
                    enabled = false;
                    return;
                }

                Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(Start)}] Loading version: {ModInfo.ModName} ({nameof(IsInGameMode)} is {IsInGameMode})");

                // Init support data                              
                SelectedRoadTypes = new List<NetTypeItem>();
                IsSnappingEnabled = false;
                IsLeftHandTraffic = Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic ==
                                    SimulationMetaData.MetaBool.True;
                IsToolActive = false;

                LoadNetworks();

                // Subscribe to milestones updated, but only if we're not in map editor
                if (IsInGameMode)
                    Singleton<UnlockManager>.instance.m_milestonesUpdated += OnMilestoneUpdate;

                Log._Debug($"[{nameof(ParallelRoadTool)}.{nameof(Start)}] Adding UI components");

                // Main UI init
                var view = UIView.GetAView();
                _mainWindow = _mainWindow ??
                              view.FindUIComponent<UIMainWindow>($"{Configuration.ResourcePrefix}MainWindow");
                if (_mainWindow != null)
                    Destroy(_mainWindow);
                _mainWindow = view.AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;

                SubscribeToUIEvents();

                Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(Start)}] Loaded");
            }
            catch (Exception e)
            {
                Log._DebugOnlyError($"[{nameof(ParallelRoadTool)}.{nameof(Start)}] Loading failed");
                Log.Exception(e);

                enabled = false;
            }
        }

        /// <summary>
        ///     This destroys all the used resourced, so that we can start fresh if the user wants to load a new game.
        ///     Before destroying everything, we store an auto-save file containing the current configuration.
        /// </summary>
        public void OnDestroy()
        {
            try
            {
                Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(OnDestroy)}] Destroying...");

                // Remove existing auto-save 
                if (File.Exists(Configuration.AutoSaveFilePath))
                    File.Delete(Configuration.AutoSaveFilePath);

                Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(OnDestroy)}] Saving networks...");

                // Save current networks 
                PresetsUtils.Export(Configuration.AutoSaveFileName);

                ToggleDetours(false);
                UnsubscribeFromUIEvents();

                // Reset data structures
                AvailableRoadTypes.Clear();
                SelectedRoadTypes.Clear();
                AvailableRoadNames = null;
                IsSnappingEnabled = false;
                IsLeftHandTraffic = false;

                // Unsubscribe to milestones updated
                Singleton<UnlockManager>.instance.m_milestonesUpdated -= OnMilestoneUpdate;

                // Destroy UI
                Destroy(_mainWindow);
                _mainWindow = null;

                Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(OnDestroy)}] Destroyed");
            }
            catch (Exception e)
            {
                // HACK - [ISSUE 31]
                Log._DebugOnlyError($"[{nameof(ParallelRoadTool)}.{nameof(OnDestroy)}] Destroy failed");
                Log.Exception(e);
            }
        }

        #endregion

        #region Utils

        /// <summary>
        ///     Deploys/un-deploys the mod by toggling the custom detours.
        /// </summary>
        /// <param name="toolEnabled"></param>
        private static void ToggleDetours(bool toolEnabled)
        {
            if (toolEnabled)
            {
                Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(ToggleDetours)}] Enabling detours...");

                NetManagerDetour.Deploy();
                NetToolDetour.Deploy();
                if (IsInGameMode)
                    NetAIDetour.Deploy();
            }
            else
            {
                Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(ToggleDetours)}] Disabling detours...");

                NetManagerDetour.Revert();
                NetToolDetour.Revert();
                if (IsInGameMode)
                    NetAIDetour.Revert();
            }
        }

        /// <summary>
        ///     We load all the available networks, based on the currently unlocked milestones (only if <see cref="IsInGameMode" />
        ///     is true, otherwise we'll load them all).
        ///     Once the networks are loaded, we can update the already displayed UI to show the newly loaded networks.
        /// </summary>
        /// <param name="updateDropdowns"></param>
        private void LoadNetworks(bool updateDropdowns = false)
        {
            // Available networks loading
            var count = PrefabCollection<NetInfo>.PrefabCount();
            AvailableRoadTypes = new List<NetInfo>();

            // HACK - [ISSUE-64] before being able to sort we need to generate names, so we use a SortedDictionary for the first pass
            var sortedNetworks = new SortedDictionary<string, NetInfo>();
            for (uint i = 0; i < count; i++)
            {
                var prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                if (prefab != null)
                {
                    var networkName = prefab.GenerateBeautifiedNetName();

                    // No need to skip stuff in map editor mode
                    if (!IsInGameMode || prefab.m_UnlockMilestone == null || prefab.m_UnlockMilestone.IsPassed())
                        sortedNetworks[networkName] = prefab;
                    else
                        Log._Debug($"[{nameof(ParallelRoadTool)}.{nameof(LoadNetworks)}] Skipping {networkName} because {prefab.m_UnlockMilestone.m_name} is not passed yet.");
                }
            }

            AvailableRoadNames = new string[sortedNetworks.Keys.Count + 1];

            // Default item, creates a net with the same type as source
            AddNetworkType(null);

            Array.Copy(sortedNetworks.Keys.ToArray(), 0, AvailableRoadNames, 1, sortedNetworks.Count);
            AvailableRoadTypes.AddRange(sortedNetworks.Values.ToList());

            Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(LoadNetworks)}] Loaded {AvailableRoadTypes.Count} networks");

            if (updateDropdowns) _mainWindow.UpdateDropdowns();
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

        private void UnsubscribeFromUIEvents()
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
            Log._Debug($"[{nameof(ParallelRoadTool)}.{nameof(MainWindowOnOnItemChanged)}] Dropdown selection changed, new selection is {value.ItemIndex} (total elements: {SelectedRoadTypes.Count})");

            var item = SelectedRoadTypes[value.ItemIndex];

            var netInfo = value.IsFiltered
                              ? Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.FirstOrDefault(n => n.GenerateBeautifiedNetName() ==
                                                                                                            value.SelectedNetworkName)
                              : value.SelectedNetworkIndex == 0
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

        private void OnMilestoneUpdate()
        {
            Log.Info("Milestones updated, reloading networks...");

            LoadNetworks(true);
        }

        #endregion
    }
}
