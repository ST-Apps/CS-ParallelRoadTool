using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ColossalFramework;
using ColossalFramework.UI;
using CSUtil.Commons;
using HarmonyLib;
using ICities;
using JetBrains.Annotations;
using ParallelRoadTool.Patches;
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
        #region NEW

        private readonly Harmony _harmony = new Harmony(ModInfo.HarmonyId);

        private ModStatuses modStatuses = ModStatuses.Disabled;

        private UIController UIController => Singleton<UIController>.instance;

        private void ToolControllerPatch_CurrentToolChanged(object sender, CurrentToolChangedEventArgs e)
        {
            Log._Debug($"[{nameof(ParallelRoadTool)}.{nameof(ToolControllerPatch_CurrentToolChanged)}] Changed tool to {e.Tool.GetType().Name}.");

            if (e.Tool is NetTool)
            {
                modStatuses |= ModStatuses.Enabled;
            }
            else
            {
                modStatuses &= ~ModStatuses.Enabled;
            }

            Log._Debug($"[{nameof(ParallelRoadTool)}.{nameof(ToolControllerPatch_CurrentToolChanged)}] New mod status is: {modStatuses:g}.");
            UIController.UpdateVisibility(modStatuses);
        }

        private void UIController_ToolToggleButtonEventCheckChanged(UIComponent component, bool value)
        {
            Log._Debug($"[{nameof(ParallelRoadTool)}.{nameof(UIController_ToolToggleButtonEventCheckChanged)}] Changed tool button to {value}.");

            if (value)
            {
                modStatuses |= ModStatuses.Active;
            } else
            {
                modStatuses &= ~ModStatuses.Active;
            }

            Log._Debug($"[{nameof(ParallelRoadTool)}.{nameof(UIController_ToolToggleButtonEventCheckChanged)}] New mod status is: {modStatuses:g}.");
            UIController.UpdateVisibility(modStatuses);
        }

        #endregion

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
        public List<NetTypeItem> SelectedRoadTypes { get; } = new List<NetTypeItem>();

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
        ///     True if we detect a mouse long press, needed to prevent multiple updates when Anarchy is on.
        ///     HACK - [ISSUE-84]
        /// </summary>
        public bool IsMouseLongPress { get; set; }

        /// <summary>
        ///     True only if <see cref="AppMode" /> is <see cref="AppMode.Game" />.
        /// </summary>
        public static bool IsInGameMode { get; set; }

        public static bool IsToolEnabled => _isToolActive && _isToolEnabled;

        /// <summary>
        /// Enabled means that the mod is ON, so we have detours deployed and our UI visible but ONLY if <see cref="_isToolActive"/> is true too.
        /// If not, this means that when the current tool becomes <see cref="NetTool"/> again we'll redeploy the detours.
        /// </summary>
        private static bool _isToolEnabled;

        /// <summary>
        /// Active means that we're currently in <see cref="NetTool"/> so we must display our button.
        /// </summary>
        private static bool _isToolActive;

        /// <summary>
        ///     Currently selected <see cref="NetInfo" /> within <see cref="NetTool" />.
        /// </summary>
        public static NetInfo CurrentNetwork => Singleton<NetTool>.instance.m_prefab;

        #endregion

        #region UI

        /// <summary>
        ///     Main UI panel.
        /// </summary>
        // private UIMainWindow _mainWindow_OLD;

        #endregion

        #endregion

        #region Unity

        public void Awake()
        {
            try
            {
                // If NetTool is not available we can't move further
                if (ToolsModifierControl.GetTool<NetTool>() == null)
                {
                    Log.Warning($"[{nameof(ParallelRoadTool)}.{nameof(Awake)}] Net Tool not found, can't deploy!");

                    // Fully disable mod's GameComponent
                    enabled = false;
                    return;
                }

                Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(Awake)}] Loading version: {ModInfo.ModName} ({nameof(IsInGameMode)} is {IsInGameMode}).");

                // Initialize support data
                SelectedRoadTypes.Clear();
                IsSnappingEnabled = false;
                IsLeftHandTraffic = Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True;
                _isToolActive = _isToolEnabled = false;

                // Load available networks
                LoadNetworks();

                // Apply harmony patches
                Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(Start)}] Patching with Harmony...");
                _harmony.PatchAll();
                Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(Start)}] Patches applied.");

                // Mod is now fully enabled
                modStatuses ^= ModStatuses.Disabled;
                modStatuses ^= ModStatuses.Deployed;
                Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(Start)}] Mod status is now {modStatuses:g}.");
            }
            catch (Exception e)
            {
                Log._DebugOnlyError($"[{nameof(ParallelRoadTool)}.{nameof(Awake)}] Loading failed.");
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
                Log._Debug($"[{nameof(ParallelRoadTool)}.{nameof(Start)}] Adding UI components");
                UIController.Initialize();
                UIController.UpdateVisibility(modStatuses);

                AttachToEvents();

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

                //ToggleDetours(false);
                DetachFromEvents();

                // Reset data structures
                AvailableRoadTypes.Clear();
                SelectedRoadTypes.Clear();
                AvailableRoadNames = null;
                IsSnappingEnabled = false;
                IsLeftHandTraffic = false;
                _isToolActive = _isToolEnabled = false;

                // Unsubscribe to milestones updated
                Singleton<UnlockManager>.instance.m_milestonesUpdated -= OnMilestoneUpdate;

                UIController.Cleanup();

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

        ///// <summary>
        /////     Deploys/un-deploys the mod by toggling the custom detours.
        ///// </summary>
        ///// <param name="toolEnabled"></param>
        //private static void ToggleDetours(bool toolEnabled)
        //{
        //    if (toolEnabled)
        //    {
        //        Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(ToggleDetours)}] Enabling detours...");

        //        NetManagerDetour.Deploy();
        //        NetToolDetour.Deploy();

        //        if (IsInGameMode)
        //            NetAIDetour.Deploy();

        //        Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(ToggleDetours)}] Enabled detours");
        //    }
        //    else
        //    {
        //        Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(ToggleDetours)}] Disabling detours...");

        //        NetManagerDetour.Revert();
        //        NetToolDetour.Revert();

        //        if (IsInGameMode)
        //            NetAIDetour.Revert();

        //        Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(ToggleDetours)}] Disabled detours");
        //    }
        //}

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
                        Log._Debug(@$"[{nameof(ParallelRoadTool)}.{nameof(LoadNetworks)}] Skipping ""{networkName}"" because ""{prefab.m_UnlockMilestone.m_name}"" is not passed yet.");

                    Log._Debug(@$"[{nameof(ParallelRoadTool)}.{nameof(LoadNetworks)}] Loaded ""{networkName}"" with atlas ""{prefab.m_Atlas?.name}"" and thumbnail ""{prefab.m_Thumbnail}"" [{prefab.GetService():g}].");
                }
            }

            AvailableRoadNames = sortedNetworks.Keys.ToArray();
            AvailableRoadTypes.AddRange(sortedNetworks.Values.ToList());

            // TODO: decide what to do with it
            // AvailableRoadNames = new string[sortedNetworks.Keys.Count + 1];

            // Default item, creates a net with the same type as source
            // TODO: decide what to do with it
            // AddNetworkType(null);

            // TODO: decide what to do with it
            // Array.Copy(sortedNetworks.Keys.ToArray(), 0, AvailableRoadNames, 1, sortedNetworks.Count);
            // TODO: decide what to do with it
            // AvailableRoadTypes.AddRange(sortedNetworks.Values.ToList());

            Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(LoadNetworks)}] Loaded {AvailableRoadTypes.Count} networks.");

            // TODO: decide what to do with it
            // if (updateDropdowns) _mainWindow_OLD.UpdateDropdowns();
        }

        private void AddNetworkType(NetInfo net)
        {
            AvailableRoadNames[AvailableRoadTypes.Count] = net.GenerateBeautifiedNetName();
            AvailableRoadTypes.Add(net);
        }

        public void ClearItems()
        {
            // TODO: decide what to do with it
            // _mainWindow.ClearItems();
        }

        public void AddItem(NetTypeItem item)
        {
            // TODO: decide what to do with it
            // _mainWindow.AddItem(item);
        }

        #endregion

        #region Handlers

        private void DetachFromEvents()
        {
            ToolControllerPatch.CurrentToolChanged -= ToolControllerPatch_CurrentToolChanged;
            UIController.ToolToggleButtonEventCheckChanged -= UIController_ToolToggleButtonEventCheckChanged;

            if (IsInGameMode)
                Singleton<UnlockManager>.instance.m_milestonesUpdated -= OnMilestoneUpdate;

            //_mainWindow_OLD.OnParallelToolToggled -= MainWindowOnOnParallelToolToggled;
            //_mainWindow_OLD.OnSnappingToggled -= MainWindowOnOnSnappingToggled;
            //_mainWindow_OLD.OnHorizontalOffsetKeypress -= MainWindowOnOnHorizontalOffsetKeypress;
            //_mainWindow_OLD.OnVerticalOffsetKeypress -= MainWindowOnOnVerticalOffsetKeypress;

            //_mainWindow_OLD.OnNetworkItemAdded -= MainWindowOnNetworkItemAdded;
            //_mainWindow_OLD.OnItemChanged -= MainWindowOnOnItemChanged;
        }

        private void AttachToEvents()
        {
            ToolControllerPatch.CurrentToolChanged += ToolControllerPatch_CurrentToolChanged;
            UIController.ToolToggleButtonEventCheckChanged += UIController_ToolToggleButtonEventCheckChanged;

            // Subscribe to milestones updated, but only if we're not in map editor
            if (IsInGameMode)
                Singleton<UnlockManager>.instance.m_milestonesUpdated += OnMilestoneUpdate;

            //_mainWindow_OLD.OnParallelToolToggled += MainWindowOnOnParallelToolToggled;
            //_mainWindow_OLD.OnSnappingToggled += MainWindowOnOnSnappingToggled;
            //_mainWindow_OLD.OnHorizontalOffsetKeypress += MainWindowOnOnHorizontalOffsetKeypress;
            //_mainWindow_OLD.OnVerticalOffsetKeypress += MainWindowOnOnVerticalOffsetKeypress;

            //_mainWindow_OLD.OnNetworkItemAdded += MainWindowOnNetworkItemAdded;
            //_mainWindow_OLD.OnItemChanged += MainWindowOnOnItemChanged;
            //_mainWindow_OLD.OnNetworkItemDeleted += MainWindowOnOnNetworkItemDeleted;
        }

        ///// <summary>
        ///// We need to react to changes in current tool.
        ///// If current tool is NOT <see cref="NetTool"/> then we must disable everything, while still remembering if the tool was enabled or not (so that we can restore this state once the tool becomes <see cref="NetTool"/> again).
        ///// </summary>
        ///// <param name="component"></param>
        ///// <param name="value"></param>
        //private void ToolBaseDetour_OnToolChanged(UIComponent component, ToolBase value)
        //{
        //    if (value is NetTool)
        //    {
        //        // If we're in NetTool, we must restore our previous state + make the button visible
        //        // TODO: decide what to do with it
        //        // _mainWindow_OLD.ToggleToolButton(true);
        //        _isToolActive = true;

        //        if (_isToolEnabled)
        //        {
        //            // This means we also need to restore detours and show our main window again
        //            //ToggleDetours(true);
        //            _mainWindow.isVisible = true;
        //        }
        //    }
        //    else
        //    {
        //        // We're not anymore in NetTool, so we must hide all of our UI and disable detours
        //        // TODO: decide what to do with it
        //        // _mainWindow_OLD.ToggleToolButton(false);
        //        _mainWindow.isVisible = false;
        //        _isToolActive = false;
        //        //ToggleDetours(false);
        //    }

        //    Log._Debug($"[{nameof(ParallelRoadTool)}.{nameof(ToolBaseDetour_OnToolChanged)}] Changed tool to {value.GetType().Name} [{nameof(_isToolActive)}: {_isToolActive}, {nameof(_isToolEnabled)}: {_isToolEnabled}]");
        //}

        private void MainWindowOnOnVerticalOffsetKeypress(UIComponent component, float step)
        {
            for (var i = 0; i < SelectedRoadTypes.Count; i++)
            {
                var item = SelectedRoadTypes[i];
                item.VerticalOffset += (1 + i) * step;
                // TODO: decide what to do with it
                // _mainWindow.UpdateItem(item, i);
            }
        }

        private void MainWindowOnOnHorizontalOffsetKeypress(UIComponent component, float step)
        {
            for (var i = 0; i < SelectedRoadTypes.Count; i++)
            {
                var item = SelectedRoadTypes[i];
                item.HorizontalOffset += (1 + i) * step;
                // TODO: decide what to do with it
                // // TODO: decide what to do with it_mainWindow.UpdateItem(item, i);
            }
        }

        private void MainWindowOnOnSnappingToggled(UIComponent component, bool value)
        {
            IsSnappingEnabled = value;
        }

        ///// <summary>
        ///// When the main button is toggled we must update <see cref="_isToolEnabled"/> but NOT <see cref="_isToolActive"/>.
        ///// This means that we're not controlling button's visibility here.
        ///// </summary>
        ///// <param name="component"></param>
        ///// <param name="value"></param>
        //private void MainWindowOnOnParallelToolToggled(UIComponent component, bool value)
        //{
        //    _isToolEnabled = value;
        //    //ToggleDetours(value);
        //    _mainWindow.isVisible = value;

        //    var netInfo = AvailableRoadTypes.Skip(2).First();
        //    Log.Info($"[{nameof(ParallelRoadTool)}.{nameof(Start)}] Loading {netInfo} in info panel");
        //    // _netInfoPanel.Refresh(netInfo);
        //}

        private void MainWindowOnNetworkItemAdded(object sender, EventArgs e)
        {
            // Previous item's offset so that we can try to separate this one from previous one without overlapping
            var prevOffset = SelectedRoadTypes.Any() ? SelectedRoadTypes.Last().HorizontalOffset : 0;
            var netInfo = ToolsModifierControl.GetTool<NetTool>().Prefab;
            var item = new NetTypeItem(netInfo, prevOffset + netInfo.m_halfWidth * 2, 0, false);
            SelectedRoadTypes.Add(item);

            // TODO: decide what to do with it
            // _mainWindow.AddItem(item);
            NetManagerPatch.NetworksCount = SelectedRoadTypes.Count;
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
            // TODO: decide what to do with it
            // _mainWindow.DeleteItem(index);

            NetManagerPatch.NetworksCount = SelectedRoadTypes.Count;
        }

        private void OnMilestoneUpdate()
        {
            Log.Info("Milestones updated, reloading networks...");

            LoadNetworks(true);
        }

        #endregion
    }
}
