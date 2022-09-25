﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ColossalFramework;
using ColossalFramework.UI;
using CSUtil.Commons;
using ICities;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Models;
using ParallelRoadTool.Patches;
using ParallelRoadTool.UI;
using UnityEngine;

namespace ParallelRoadTool.Managers
{
    /// <summary>
    ///     Mod's main controller and data storage.
    /// </summary>

    // ReSharper disable once ClassNeverInstantiated.Global
    public class ParallelRoadToolManager : MonoBehaviour
    {

        #region Fields

        /// <summary>
        /// True if auto-save has been loaded already, false if not.
        /// </summary>
        private static bool _autoSaveLoaded;

        #endregion

        #region Properties

        /// <summary>
        ///     Main controller for everything UI-related
        /// </summary>
        private static UIController UIController => Singleton<UIController>.instance;

        /// <summary>
        ///     <see cref="List{T}" /> containing all the available <see cref="NetInfo" /> objects.
        ///     A <see cref="NetInfo" /> can be any kind of network that the user can build.
        /// </summary>
        public List<NetInfo> AvailableRoadTypes { get; private set; }

        /// <summary>
        ///     <see cref="List{T}" /> containing all the selected <see cref="NetInfoItem" /> objects.
        ///     This contains all the parallel/stacked networks that will be built once a main segment is created.
        /// </summary>
        public List<NetInfoItem> SelectedNetworkTypes { get; } = new();

        /// <summary>
        ///     This tells if the user wants the parallel/stacked nodes to snap with already existing nodes.
        /// </summary>
        public bool IsSnappingEnabled { get; private set; }

        /// <summary>
        ///     True if the current map is using left-hand traffic.
        /// </summary>
        public bool IsLeftHandTraffic { get; private set; }

        /// <summary>
        ///     True if we detect a mouse long press, needed to prevent multiple updates when Anarchy is on.
        ///     HACK - [ISSUE-84]
        /// </summary>
        public bool IsMouseLongPress { get; set; }

        /// <summary>
        ///     True only if <see cref="AppMode" /> is <see cref="AppMode.Game" />.
        /// </summary>
        public static bool IsInGameMode { get; set; }

        /// <summary>
        ///     Flags for the current statuses for the mod. For more details see: <see cref="Models.ModStatuses" />.
        /// </summary>
        public static ModStatuses ModStatuses { get; private set; } = ModStatuses.Disabled;

        #endregion

        #region Callbacks

        /// <summary>
        ///     Adds another parallel/stacked segment to current configuration by using the currently selected
        ///     <see cref="NetInfo" /> inside <see cref="NetTool" />.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIController_AddNetworkButtonEventClicked(object sender, EventArgs e)
        {
            var netInfo = ToolsModifierControl.GetTool<NetTool>().Prefab;

            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(UIController_AddNetworkButtonEventClicked)}] Adding a new network [{netInfo.GenerateBeautifiedNetName()}]");

            // Previous item's offset so that we can try to separate this one from previous one without overlapping
            var prevOffset = SelectedNetworkTypes.Any() ? SelectedNetworkTypes.Last().HorizontalOffset : 0;
            var item = new NetInfoItem(netInfo, prevOffset + netInfo.m_halfWidth * 2, 0, false);

            AddNetwork(item);
        }

        /// <summary>
        /// Forces sorting and refreshing the current selected networks
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIControllerOnSortNetworksButtonEventClicked(object sender, EventArgs e)
        {
            RefreshNetworks(true);
        }

        /// <summary>
        ///     Removes the <see cref="NetInfo" /> at provided index from the current configuration.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="index"></param>
        private void UIController_DeleteNetworkButtonEventClicked(UIComponent component, int index)
        {
            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(UIController_DeleteNetworkButtonEventClicked)}] Removing network at index {index}.");

            RemoveNetwork(index);
        }

        /// <summary>
        ///     Toggles mod off by setting <see cref="ModStatuses" /> to not have the <see cref="ModStatuses.Active" /> flag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UIController_ClosedButtonEventClicked(object sender, EventArgs e)
        {
            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(UIController_ClosedButtonEventClicked)}] Received click on close button.");
            ModStatuses &= ~ModStatuses.Active;

            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(UIController_ClosedButtonEventClicked)}] New mod status is: {ModStatuses:g}.");
            UIController.UpdateVisibility(ModStatuses);
        }

        /// <summary>
        ///     Handle changes in the current selected tool. Mod is only enabled if <see cref="NetTool" /> is currently enabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolControllerPatch_CurrentToolChanged(object sender, CurrentToolChangedEventArgs e)
        {
            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(ToolControllerPatch_CurrentToolChanged)}] Changed tool to {e.Tool.GetType().Name}.");

            if (e.Tool is NetTool)
                ModStatuses |= ModStatuses.Enabled;
            else
                ModStatuses &= ~ModStatuses.Enabled;

            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(ToolControllerPatch_CurrentToolChanged)}] New mod status is: {ModStatuses:g}.");
            UIController.UpdateVisibility(ModStatuses);
        }

        /// <summary>
        ///     Toggles the mod on/off by flipping the <see cref="ModStatuses.Active" /> flag on <see cref="ModStatuses" />.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="value"></param>
        private void UIController_ToolToggleButtonEventCheckChanged(UIComponent component, bool value)
        {
            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(UIController_ToolToggleButtonEventCheckChanged)}] Changed tool button to {value}.");

            ToggleModActiveStatus(true, value);
        }

        /// <summary>
        ///     Updates the main <see cref="NetInfo" /> item which is always synced with the one selected in
        ///     <see cref="NetTool.Prefab" />.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NetToolsPrefabPatch_CurrentNetInfoChanged(object sender, CurrentNetInfoPrefabChangedEventArgs e)
        {
            UIController.UpdateCurrentNetwork(e.Prefab);
        }

        /// <summary>
        ///     Handles changes in a single <see cref="NetInfoItem" /> configuration.
        ///     If user changed the network type only we update it, otherwise we just update the custom properties.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="value"></param>
        private void UIController_NetTypeEventChanged(UIComponent component, NetTypeItemEventArgs value)
        {
            // Check if we need to just replace the selected network or if we are updating other properties
            if (value.SelectedNetworkName != null)
            {
                // Network only, we change its type and copy over our custom properties
                SelectedNetworkTypes[value.ItemIndex] = new NetInfoItem(AvailableRoadTypes.First(n => n.name == value.SelectedNetworkName))
                {
                    HorizontalOffset = SelectedNetworkTypes[value.ItemIndex].HorizontalOffset,
                    VerticalOffset = SelectedNetworkTypes[value.ItemIndex].VerticalOffset,
                    IsReversed = SelectedNetworkTypes[value.ItemIndex].IsReversed
                };
            }
            else
            {
                // Update customizable properties
                var targetItem = SelectedNetworkTypes[value.ItemIndex];
                targetItem.HorizontalOffset = value.HorizontalOffset;
                targetItem.VerticalOffset = value.VerticalOffset;
                targetItem.IsReversed = value.IsReversed;
            }


            // In both cases we need to refresh networks
            RefreshNetworks();
        }

        /// <summary>
        ///     Toggles snapping on/off
        /// </summary>
        /// <param name="component"></param>
        /// <param name="value"></param>
        private void UIController_ToggleSnappingButtonEventCheckChanged(UIComponent component, bool value)
        {
            IsSnappingEnabled = value;
        }

        /// <summary>
        ///     Handles keypress for horizontal increase/decrease key-binding.
        ///     Horizontal offsets scale based on <see cref="NetInfoItem" />'s index too.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="step"></param>
        private void UIController_OnHorizontalOffsetKeypress(UIComponent component, float step)
        {
            for (var i = 0; i < SelectedNetworkTypes.Count; i++)
            {
                var item = SelectedNetworkTypes[i];
                item.HorizontalOffset += (1 + i) * step;
            }

            RefreshNetworks();
        }

        /// <summary>
        ///     Handles keypress for vertical increase/decrease key-binding.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="step"></param>
        private void UIController_OnVerticalOffsetKeypress(UIComponent component, float step)
        {
            for (var i = 0; i < SelectedNetworkTypes.Count; i++)
            {
                var item = SelectedNetworkTypes[i];
                item.VerticalOffset += (1 + i) * step;
            }

            RefreshNetworks();
        }

        /// <summary>
        ///     Handles changes on milestones by reloading <see cref="AvailableRoadTypes" /> to include the newly unlocked
        ///     networks.
        /// </summary>
        private void OnMilestoneUpdate()
        {
            Log.Info($"[{nameof(ParallelRoadToolManager)}.{nameof(OnMilestoneUpdate)}] Milestones updated, reloading networks...");

            LoadNetworks();
        }

        #endregion

        #region Unity

        #region Lifecycle

        // TODO: Awake/Start or ctor??
        public void Awake()
        {
            try
            {
                // If NetTool is not available we can't move further
                if (ToolsModifierControl.GetTool<NetTool>() == null)
                {
                    Log.Warning($"[{nameof(ParallelRoadToolManager)}.{nameof(Awake)}] Net Tool not found, can't deploy!");

                    // Fully disable mod's GameComponent
                    enabled = false;
                    return;
                }

                Log.Info($"[{nameof(ParallelRoadToolManager)}.{nameof(Awake)}] Loading version: {Mod.Instance.Name} ({nameof(IsInGameMode)} is {IsInGameMode}).");

                // Initialize support data
                SelectedNetworkTypes.Clear();
                IsSnappingEnabled = false;
                IsLeftHandTraffic = Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True;

                // Load available networks
                LoadNetworks();

                // Mod is now fully enabled
                ModStatuses ^= ModStatuses.Disabled;
                ModStatuses ^= ModStatuses.Deployed;

                Log.Info($"[{nameof(ParallelRoadToolManager)}.{nameof(Start)}] Mod status is now {ModStatuses:g}.");
            }
            catch (Exception e)
            {
                Log._DebugOnlyError($"[{nameof(ParallelRoadToolManager)}.{nameof(Awake)}] Loading failed.");
                Log.Exception(e);

                // Fully disable mod's GameComponent
                enabled = false;
            }
        }

        /// <summary>
        ///     This method initializes mod's first time loading.
        ///     If <see cref="NetTool" /> is detected we initialize all the support structures, load the available networks and
        ///     finally create the UI.
        /// </summary>
        public void Start()
        {
            try
            {
                Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(Start)}] Adding UI components");

                UIController.Initialize();
                UIController.UpdateVisibility(ModStatuses);

                AttachToEvents();

                Log.Info($"[{nameof(ParallelRoadToolManager)}.{nameof(Start)}] Loaded");
            }
            catch (Exception e)
            {
                Log._DebugOnlyError($"[{nameof(ParallelRoadToolManager)}.{nameof(Start)}] Loading failed");
                Log.Exception(e);

                // Fully disable mod's GameComponent
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
                Log.Info($"[{nameof(ParallelRoadToolManager)}.{nameof(OnDestroy)}] Destroying...");

                // Save current networks
                SavePreset();

                DetachFromEvents();

                // Reset data structures
                AvailableRoadTypes.Clear();
                SelectedNetworkTypes.Clear();
                IsSnappingEnabled = false;
                IsLeftHandTraffic = false;
                _autoSaveLoaded = false;

                // Clean all the UI components
                UIController.Cleanup();

                Log.Info($"[{nameof(ParallelRoadToolManager)}.{nameof(OnDestroy)}] Destroyed");
            }
            catch (Exception e)
            {
                // HACK - [ISSUE 31]
                Log._DebugOnlyError($"[{nameof(ParallelRoadToolManager)}.{nameof(OnDestroy)}] Destroy failed");
                Log.Exception(e);

                // Fully disable mod's GameComponent
                enabled = false;
            }
        }

        #endregion

        #endregion

        #region Control

        #region Internals

        private void AddNetwork(NetInfoItem item)
        {
            SelectedNetworkTypes.Add(item);
            UIController.AddNetwork(item);

            NetManagerPatch.NetworksCount = SelectedNetworkTypes.Count;
        }

        /// <summary>
        ///     Unsubscribe from all the events that we're handling
        /// </summary>
        private void DetachFromEvents()
        {
            ToolControllerPatch.CurrentToolChanged -= ToolControllerPatch_CurrentToolChanged;
            NetToolsPrefabPatch.CurrentNetInfoChanged -= NetToolsPrefabPatch_CurrentNetInfoChanged;
            UIController.ToolToggleButtonEventCheckChanged -= UIController_ToolToggleButtonEventCheckChanged;
            UIController.ToggleSnappingButtonEventCheckChanged -= UIController_ToggleSnappingButtonEventCheckChanged;
            UIController.CloseButtonEventClicked -= UIController_ClosedButtonEventClicked;
            UIController.AddNetworkButtonEventClicked -= UIController_AddNetworkButtonEventClicked;
            UIController.SortNetworksButtonEventClicked -= UIControllerOnSortNetworksButtonEventClicked;
            UIController.DeleteNetworkButtonEventClicked -= UIController_DeleteNetworkButtonEventClicked;
            UIController.OnHorizontalOffsetKeypress -= UIController_OnHorizontalOffsetKeypress;
            UIController.OnVerticalOffsetKeypress -= UIController_OnVerticalOffsetKeypress;
            UIController.NetTypeEventChanged -= UIController_NetTypeEventChanged;

            if (IsInGameMode)
                Singleton<UnlockManager>.instance.m_milestonesUpdated -= OnMilestoneUpdate;
        }

        /// <summary>
        ///     Subscribe to all the events that we're handling
        /// </summary>
        private void AttachToEvents()
        {
            ToolControllerPatch.CurrentToolChanged += ToolControllerPatch_CurrentToolChanged;
            NetToolsPrefabPatch.CurrentNetInfoChanged += NetToolsPrefabPatch_CurrentNetInfoChanged;
            UIController.ToolToggleButtonEventCheckChanged += UIController_ToolToggleButtonEventCheckChanged;
            UIController.ToggleSnappingButtonEventCheckChanged += UIController_ToggleSnappingButtonEventCheckChanged;
            UIController.CloseButtonEventClicked += UIController_ClosedButtonEventClicked;
            UIController.AddNetworkButtonEventClicked += UIController_AddNetworkButtonEventClicked;
            UIController.SortNetworksButtonEventClicked += UIControllerOnSortNetworksButtonEventClicked;
            UIController.DeleteNetworkButtonEventClicked += UIController_DeleteNetworkButtonEventClicked;
            UIController.OnHorizontalOffsetKeypress += UIController_OnHorizontalOffsetKeypress;
            UIController.OnVerticalOffsetKeypress += UIController_OnVerticalOffsetKeypress;
            UIController.NetTypeEventChanged += UIController_NetTypeEventChanged;

            // Subscribe to milestones updated, but only if we're not in map editor
            if (IsInGameMode)
                Singleton<UnlockManager>.instance.m_milestonesUpdated += OnMilestoneUpdate;
        }

        /// <summary>
        ///     Sorts and refreshes <see cref="SelectedNetworkTypes" />, updating the UI at the end.
        /// </summary>
        private void RefreshNetworks(bool sort = false)
        {
            if (sort)
            {
                // Sort networks based on ascending HorizontalOffset
                var sorted = SelectedNetworkTypes.OrderBy(r => r.HorizontalOffset).ToList();

                // Clear and rebuild the list with the currently selected networks
                SelectedNetworkTypes.Clear();
                SelectedNetworkTypes.AddRange(sorted);
            }

            // Update the UI
            UIController.RefreshNetworks(SelectedNetworkTypes);

            // Update networks count to patches
            NetManagerPatch.NetworksCount = SelectedNetworkTypes.Count;
        }

        /// <summary>
        ///     Remove the network at index from the <see cref="SelectedNetworkTypes" /> and calls <see cref="RefreshNetworks" />.
        /// </summary>
        /// <param name="index"></param>
        private void RemoveNetwork(int index)
        {
            SelectedNetworkTypes.RemoveAt(index);
            RefreshNetworks();
        }

        /// <summary>
        ///     We load all the available networks, based on the currently unlocked milestones (only if <see cref="IsInGameMode" />
        ///     is true, otherwise we'll load them all).
        ///     Once the networks are loaded, we can update the already displayed UI to show the newly loaded networks.
        /// </summary>
        private void LoadNetworks()
        {
            // Available networks loading
            var count = PrefabCollection<NetInfo>.PrefabCount();
            AvailableRoadTypes = new List<NetInfo>();

            // HACK - [ISSUE-64] before being able to sort we need to generate names, so we use a SortedDictionary for the first pass
            var sortedNetworks = new SortedDictionary<string, NetInfo>();
            for (uint i = 0; i < count; i++)
            {
                var prefab = PrefabCollection<NetInfo>.GetPrefab(i);
                if (prefab == null) continue;

                var networkName = prefab.GenerateBeautifiedNetName();
                var atlasName = prefab.m_Atlas?.name;

                // Skip items with no atlas set as they're not networks (e.g. train lines)
                if (string.IsNullOrEmpty(atlasName))
                {
                    Log._Debug(@$"[{nameof(ParallelRoadToolManager)}.{nameof(LoadNetworks)}] Skipping ""{networkName}"" because it's not a real network (atlas is not set).");
                    continue;
                }

                // No need to skip stuff in map editor mode
                if (!IsInGameMode || prefab.m_UnlockMilestone == null || prefab.m_UnlockMilestone.IsPassed())
                    sortedNetworks[networkName] = prefab;
                else
                    Log._Debug(@$"[{nameof(ParallelRoadToolManager)}.{nameof(LoadNetworks)}] Skipping ""{networkName}"" because ""{prefab.m_UnlockMilestone.m_name}"" is not passed yet.");

                Log._Debug(@$"[{nameof(ParallelRoadToolManager)}.{nameof(LoadNetworks)}] Loaded ""{networkName}"" with atlas ""{atlasName}"" and thumbnail ""{prefab.m_Thumbnail}"" [{prefab.GetService():g}].");
            }

            AvailableRoadTypes.AddRange(sortedNetworks.Values.ToList());

            Log.Info($"[{nameof(ParallelRoadToolManager)}.{nameof(LoadNetworks)}] Loaded {AvailableRoadTypes.Count} networks.");
        }

        #endregion

        #region Public API

        /// <summary>
        ///     Flips the status for <see cref="ModStatuses" /> Active flag, effectively activating/disactivating the mod.
        ///     This change propagates to <see cref="UIController" /> to update the UI accordingly.
        /// </summary>
        public void ToggleModActiveStatus(bool force = false, bool value = false)
        {
            if (!force)
                // Toggle active status flag
                ModStatuses ^= ModStatuses.Active;
            else
            {
                // We're forcing a specific value so set/unset the relative flag
                if (value)
                    ModStatuses |= ModStatuses.Active;
                else
                    ModStatuses &= ~ModStatuses.Active;
            }

            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(ToggleModActiveStatus)}] New mod status is: {ModStatuses:g}.");

            // Update the UI status based on the new Active value
            UIController.UpdateVisibility(ModStatuses);

            // Load auto-save from previous session, if any
            if (_autoSaveLoaded) return;
            LoadPreset();
            _autoSaveLoaded = true;
        }

        /// <summary>
        ///     Returns the <see cref="NetInfo" /> matching the provided name
        /// </summary>
        /// <param name="networkName"></param>
        /// <returns></returns>
        public NetInfo FromName(string networkName)
        {
            return AvailableRoadTypes.FirstOrDefault(n => n.name == networkName || n.GenerateBeautifiedNetName() == networkName);
        }

        /// <summary>
        ///     Saves the current configuration as a preset
        /// </summary>
        /// <param name="fileName"></param>
        public void SavePreset(string fileName = null)
        {
            PresetsManager.SavePreset(SelectedNetworkTypes, fileName);
        }

        /// <summary>
        ///     Loads the current configuration from a preset and reloads
        ///     <see cref="SelectedNetworkTypes" /> to reflect these new networks.
        /// </summary>
        /// <param name="fileName"></param>
        public void LoadPreset(string fileName = null)
        {
            var preset = PresetsManager.LoadPreset(fileName);

            SelectedNetworkTypes.Clear();
            foreach (var netInfoItem in preset)
            {
                AddNetwork(netInfoItem);
            }

            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(LoadPreset)}] {nameof(SelectedNetworkTypes)} now contains: ({string.Join(", ", SelectedNetworkTypes.Select(n => n.BeautifiedName).ToArray())}).");

            NetManagerPatch.NetworksCount = SelectedNetworkTypes.Count;
            RefreshNetworks();

        }

        #endregion

        #endregion
    }
}