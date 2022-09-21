using System;
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
using static VehicleSelector;

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
        /// Main controller for everything UI-related
        /// </summary>
        private static UIController UIController => Singleton<UIController>.instance;

        #endregion

        #region Properties

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
        /// Flags for the current statuses for the mod. For more details see: <see cref="Models.ModStatuses"/>.
        /// </summary>
        public static ModStatuses ModStatuses { get; private set; } = ModStatuses.Disabled;

        /// <summary>
        ///     Currently selected <see cref="NetInfo" /> within <see cref="NetTool" />.
        /// </summary>
        public static NetInfo CurrentNetwork => Singleton<NetTool>.instance.m_prefab;

        #endregion

        #region Callbacks

        private void UIController_AddNetworkButtonEventClicked(object sender, EventArgs e)
        {
            var netInfo = ToolsModifierControl.GetTool<NetTool>().Prefab;

            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(UIController_AddNetworkButtonEventClicked)}] Adding a new network [{netInfo.GenerateBeautifiedNetName()}]");

            // Previous item's offset so that we can try to separate this one from previous one without overlapping
            var prevOffset = SelectedNetworkTypes.Any() ? SelectedNetworkTypes.Last().HorizontalOffset : 0;
            var item = new NetInfoItem(netInfo, prevOffset + netInfo.m_halfWidth * 2, 0, false);

            SelectedNetworkTypes.Add(item);
            UIController.AddNetwork(item);
            NetManagerPatch.NetworksCount = SelectedNetworkTypes.Count;
        }

        private void UIController_DeleteNetworkButtonEventClicked(UIComponent component, int index)
        {
            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(UIController_DeleteNetworkButtonEventClicked)}] Removing network at index {index}.");

            RemoveNetwork(index);
        }

        private void UIController_ClosedButtonEventClicked(object sender, EventArgs e)
        {
            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(UIController_ClosedButtonEventClicked)}] Received click on close button.");
            ModStatuses &= ~ModStatuses.Active;

            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(UIController_ClosedButtonEventClicked)}] New mod status is: {ModStatuses:g}.");
            UIController.UpdateVisibility(ModStatuses);
        }

        private void ToolControllerPatch_CurrentToolChanged(object sender, CurrentToolChangedEventArgs e)
        {
            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(ToolControllerPatch_CurrentToolChanged)}] Changed tool to {e.Tool.GetType().Name}.");

            if (e.Tool is NetTool)
            {
                ModStatuses |= ModStatuses.Enabled;
            }
            else
            {
                ModStatuses &= ~ModStatuses.Enabled;
            }

            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(ToolControllerPatch_CurrentToolChanged)}] New mod status is: {ModStatuses:g}.");
            UIController.UpdateVisibility(ModStatuses);
        }

        private void UIController_ToolToggleButtonEventCheckChanged(UIComponent component, bool value)
        {
            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(UIController_ToolToggleButtonEventCheckChanged)}] Changed tool button to {value}.");

            if (value)
            {
                ModStatuses |= ModStatuses.Active;
            }
            else
            {
                ModStatuses &= ~ModStatuses.Active;
            }

            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(UIController_ToolToggleButtonEventCheckChanged)}] New mod status is: {ModStatuses:g}.");
            UIController.UpdateVisibility(ModStatuses);
        }

        private void NetToolsPrefabPatch_CurrentNetInfoChanged(object sender, CurrentNetInfoPrefabChangedEventArgs e)
        {
            UIController.UpdateCurrentNetwork(e.Prefab);
        }

        private void UIController_NetTypeEventChanged(UIComponent component, NetTypeItemEventArgs value)
        {
            var targetItem = SelectedNetworkTypes[value.ItemIndex];
            targetItem.HorizontalOffset = value.HorizontalOffset;
            targetItem.VerticalOffset = value.VerticalOffset;
            targetItem.IsReversed = value.IsReversedNetwork;

            RefreshNetworks();
        }

        private void UIController_ToggleSnappingButtonEventCheckChanged(UIComponent component, bool value)
        {
            IsSnappingEnabled = value;
        }

        private void UIController_OnHorizontalOffsetKeypress(UIComponent component, float step)
        {
            for (var i = 0; i < SelectedNetworkTypes.Count; i++)
            {
                var item = SelectedNetworkTypes[i];
                item.HorizontalOffset += (1 + i) * step;
            }
            RefreshNetworks();
        }

        private void UIController_OnVerticalOffsetKeypress(UIComponent component, float step)
        {
            for (var i = 0; i < SelectedNetworkTypes.Count; i++)
            {
                var item = SelectedNetworkTypes[i];
                item.VerticalOffset += (1 + i) * step;
            }
            RefreshNetworks();
        }

        private void OnMilestoneUpdate()
        {
            Log.Info($"[{nameof(ParallelRoadToolManager)}.{nameof(OnMilestoneUpdate)}] Milestones updated, reloading networks...");

            LoadNetworks();
        }

        #endregion

        #region Unity

        #region Lifecycle

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

                // Remove existing auto-save 
                if (File.Exists(Configuration.AutoSaveFilePath))
                    File.Delete(Configuration.AutoSaveFilePath);

                Log.Info($"[{nameof(ParallelRoadToolManager)}.{nameof(OnDestroy)}] Saving networks...");

                // Save current networks 
                //PresetsUtils.Export(Configuration.AutoSaveFileName);

                DetachFromEvents();

                // Reset data structures
                AvailableRoadTypes.Clear();
                SelectedNetworkTypes.Clear();
                IsSnappingEnabled = false;
                IsLeftHandTraffic = false;

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

        private void DetachFromEvents()
        {
            ToolControllerPatch.CurrentToolChanged -= ToolControllerPatch_CurrentToolChanged;
            NetToolsPrefabPatch.CurrentNetInfoChanged -= NetToolsPrefabPatch_CurrentNetInfoChanged;
            UIController.ToolToggleButtonEventCheckChanged -= UIController_ToolToggleButtonEventCheckChanged;
            UIController.ToggleSnappingButtonEventCheckChanged -= UIController_ToggleSnappingButtonEventCheckChanged;
            UIController.CloseButtonEventClicked -= UIController_ClosedButtonEventClicked;
            UIController.AddNetworkButtonEventClicked -= UIController_AddNetworkButtonEventClicked;
            UIController.DeleteNetworkButtonEventClicked -= UIController_DeleteNetworkButtonEventClicked;
            UIController.OnHorizontalOffsetKeypress -= UIController_OnHorizontalOffsetKeypress;
            UIController.OnVerticalOffsetKeypress -= UIController_OnVerticalOffsetKeypress;
            UIController.NetTypeEventChanged -= UIController_NetTypeEventChanged;

            if (IsInGameMode)
                Singleton<UnlockManager>.instance.m_milestonesUpdated -= OnMilestoneUpdate;
        }

        private void AttachToEvents()
        {
            ToolControllerPatch.CurrentToolChanged += ToolControllerPatch_CurrentToolChanged;
            NetToolsPrefabPatch.CurrentNetInfoChanged += NetToolsPrefabPatch_CurrentNetInfoChanged;
            UIController.ToolToggleButtonEventCheckChanged += UIController_ToolToggleButtonEventCheckChanged;
            UIController.ToggleSnappingButtonEventCheckChanged += UIController_ToggleSnappingButtonEventCheckChanged;
            UIController.CloseButtonEventClicked += UIController_ClosedButtonEventClicked;
            UIController.AddNetworkButtonEventClicked += UIController_AddNetworkButtonEventClicked;
            UIController.DeleteNetworkButtonEventClicked += UIController_DeleteNetworkButtonEventClicked;
            UIController.OnHorizontalOffsetKeypress += UIController_OnHorizontalOffsetKeypress;
            UIController.OnVerticalOffsetKeypress += UIController_OnVerticalOffsetKeypress;
            UIController.NetTypeEventChanged += UIController_NetTypeEventChanged;

            // Subscribe to milestones updated, but only if we're not in map editor
            if (IsInGameMode)
                Singleton<UnlockManager>.instance.m_milestonesUpdated += OnMilestoneUpdate;
        }

        /// <summary>
        /// Sorts and refreshes <see cref="SelectedNetworkTypes"/>, updating the UI at the end.
        /// </summary>
        private void RefreshNetworks()
        {
            // Sort networks based on ascending HorizontalOffset
            var sorted = SelectedNetworkTypes.OrderBy(r => r.HorizontalOffset).ToList();

            // Clear and rebuild the list with the currently selected networks
            SelectedNetworkTypes.Clear();
            SelectedNetworkTypes.AddRange(sorted);

            // Update the UI
            UIController.RefreshNetworks(SelectedNetworkTypes);

            // Update networks count to patches
            NetManagerPatch.NetworksCount = SelectedNetworkTypes.Count;
        }

        /// <summary>
        /// Remove the network at index from the <see cref="SelectedNetworkTypes"/> and calls <see cref="RefreshNetworks"/>.
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
        /// Flips the status for <see cref="ModStatuses"/> Active flag, effectively activating/disactivating the mod.
        /// This change propagates to <see cref="UIController"/> to update the UI accordingly.
        /// </summary>
        public void ToggleModActiveStatus()
        {
            // Toggle active status flag
            ModStatuses ^= ModStatuses.Active;

            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(ToggleModActiveStatus)}] New mod status is: {ModStatuses:g}.");

            // Update the UI status based on the new Active value
            UIController.UpdateVisibility(ModStatuses);
        }

        /// <summary>
        /// Returns the <see cref="NetInfo"/> matching the provided name
        /// </summary>
        /// <param name="networkName"></param>
        /// <returns></returns>
        public NetInfo FromName(string networkName)
        {
            return AvailableRoadTypes.FirstOrDefault(n => n.name == networkName);
        }

        public void SavePreset(string fileName)
        {
            PresetsManager.SavePreset(fileName);
        }

        public void LoadPreset(string fileName)
        {
            var preset = PresetsManager.LoadPreset(fileName);

            SelectedNetworkTypes.Clear();
            foreach (var netInfoItem in PresetsManager.ToNetInfoItems(preset))
            {
                SelectedNetworkTypes.Add(netInfoItem);
                UIController.AddNetwork(netInfoItem);
            }

            NetManagerPatch.NetworksCount = SelectedNetworkTypes.Count;
            RefreshNetworks();

            Log._Debug($"[{nameof(ParallelRoadToolManager)}.{nameof(LoadPreset)}] {nameof(SelectedNetworkTypes)} now contains: ({string.Join(", ", SelectedNetworkTypes.Select(n => n.BeautifiedName).ToArray())}).");
        }

        #endregion

        #endregion
    }
}
