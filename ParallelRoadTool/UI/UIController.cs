using ColossalFramework;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using System;
using System.Collections.Generic;
using ParallelRoadTool.Settings;
using ParallelRoadTool.UI.Main;
using UnityEngine;
using AlgernonCommons.UI;
using ParallelRoadTool.Managers;
using ParallelRoadTool.UI.Presets;

namespace ParallelRoadTool.UI
{
    /// <summary>
    /// Main controller for the UI-related components of the mod.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class UIController : MonoBehaviour
    {

        #region Events

        public event EventHandler CloseButtonEventClicked;
        public event EventHandler AddNetworkButtonEventClicked;
        public event PropertyChangedEventHandler<float> OnHorizontalOffsetKeypress;
        public event PropertyChangedEventHandler<float> OnVerticalOffsetKeypress;

        public event PropertyChangedEventHandler<NetTypeItemEventArgs> NetTypeEventChanged
        {
            add => _mainWindow.NetTypeEventChanged += value;
            remove => _mainWindow.NetTypeEventChanged -= value;
        }

        public event PropertyChangedEventHandler<bool> ToolToggleButtonEventCheckChanged
        {
            add => _mainButton.eventCheckChanged += value;
            remove => _mainButton.eventCheckChanged -= value;
        }

        public event PropertyChangedEventHandler<bool> ToggleSnappingButtonEventCheckChanged
        {
            add => _mainWindow.ToggleSnappingButtonEventCheckChanged += value;
            remove => _mainWindow.ToggleSnappingButtonEventCheckChanged -= value;
        }

        public event PropertyChangedEventHandler<int> DeleteNetworkButtonEventClicked
        {
            add => _mainWindow.DeleteNetworkButtonEventClicked += value;
            remove => _mainWindow.DeleteNetworkButtonEventClicked += value;
        }

        #endregion

        #region Callbacks

        private void MainWindow_CloseButtonEventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            CloseButtonEventClicked?.Invoke(null, null);
        }

        private void MainWindow_AddNetworkButtonEventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            AddNetworkButtonEventClicked?.Invoke(null, null);
        }

        private void MainWindowOnSavePresetButtonEventClicked(UIComponent component, UIMouseEventParameter eventparam)
        {
            // Prevent focus for main window
            _mainWindow.canFocus = false;

            StandalonePanelManager<UISavePresetWindow>.Create();

            // Restore focus for main window on modal close
            StandalonePanelManager<UISavePresetWindow>.Panel.EventClose += () =>
                                                                           {
                                                                               _mainWindow.canFocus = true;
                                                                               _mainWindow.Focus();
                                                                           };

            // Load data
            StandalonePanelManager<UISavePresetWindow>.Panel.RefreshItems(PresetsManager.ListSavedFiles());
        }

        #endregion

        #region Unity

        #region Components

        /// <summary>
        ///     Main UI panel.
        /// </summary>
        private static UIMainWindow _mainWindow;

        /// <summary>
        /// Main UI button.
        /// </summary>
        private static UIToolToggleButton _mainButton;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Check for key-bindings and activate their corresponding action.
        /// </summary>
        public void OnGUI()
        {
            if (UIView.HasModalInput()
                || UIView.HasInputFocus()
                || !Singleton<ParallelRoadToolManager>.exists
                || !(ToolsModifierControl.toolController.CurrentTool is NetTool))
                return;

            var e = Event.current;

            if (e.isMouse)
            {
                // HACK - [ISSUE-84] Report if we're currently having a long mouse press
                Singleton<ParallelRoadToolManager>.instance.IsMouseLongPress = e.type switch
                {
                    EventType.MouseDown => true,
                    EventType.MouseUp => false,
                    _ => Singleton<ParallelRoadToolManager>.instance.IsMouseLongPress
                };

                Log._Debug($"[{nameof(UIMainWindow)}.{nameof(OnGUI)}] Setting {nameof(Singleton<ParallelRoadToolManager>.instance.IsMouseLongPress)} to {Singleton<ParallelRoadToolManager>.instance.IsMouseLongPress}");
            }
            // Checking key presses
            if (ModSettings.KeyToggleTool.IsPressed(e)) ToggleModStatus();
            if (ModSettings.KeyIncreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(1f);
            if (ModSettings.KeyDecreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(-1f);
            if (ModSettings.KeyIncreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(1f, false);
            if (ModSettings.KeyDecreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(-1f, false);
        }

        #endregion

        #endregion

        #region Control

        #region Internals

        /// <summary>
        /// Toggles the current mod status on <see cref="ParallelRoadToolManager"/>.
        /// </summary>
        private static void ToggleModStatus()
        {
            Singleton<ParallelRoadToolManager>.instance.ToggleModActiveStatus();
        }

        private void AdjustNetOffset(float step, bool isHorizontal = true)
        {
            // Adjust all offsets on keypress
            if (isHorizontal)
                OnHorizontalOffsetKeypress?.Invoke(null, step);
            else
                OnVerticalOffsetKeypress?.Invoke(null, step);
        }

        /// <summary>
        /// Builds mod top-level UI components and attaches them to the current view.
        /// The components are:
        /// <list type="bullet">
        /// <item><see cref="UIMainWindow"/></item>
        /// <item><see cref="UIToolToggleButton"/></item>
        /// </list>
        /// </summary>
        private void BuildUI()
        {
            var view = UIView.GetAView();

            Log._Debug($"[{nameof(UIController)}.{nameof(BuildUI)}] Creating mod's main window...");

            _mainWindow ??= view.FindUIComponent<UIMainWindow>($"{Configuration.ResourcePrefix}MainWindow");
            if (_mainWindow != null)
                DestroyImmediate(_mainWindow);
            _mainWindow = view.AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;

            Log._Debug($"[{nameof(UIController)}.{nameof(BuildUI)}] Mod's main window created.");

            Log._Debug($"[{nameof(UIController)}.{nameof(BuildUI)}] Creating mod's main button...");

            var button = view.FindUIComponent<UIToolToggleButton>($"{Configuration.ResourcePrefix}Parallel");
            if (button != null) DestroyImmediate(button);
            _mainButton = view.AddUIComponent(typeof(UIToolToggleButton)) as UIToolToggleButton;

            Log._Debug($"[{nameof(UIController)}.{nameof(BuildUI)}] Mod's main button created.");
        }

        private void AttachToEvents()
        {
            _mainWindow.CloseButtonEventClicked += MainWindow_CloseButtonEventClicked;
            _mainWindow.AddNetworkButtonEventClicked += MainWindow_AddNetworkButtonEventClicked;
            _mainWindow.SavePresetButtonEventClicked += MainWindowOnSavePresetButtonEventClicked;
        }

        private void DetachFromEvents()
        {
            _mainWindow.CloseButtonEventClicked -= MainWindow_CloseButtonEventClicked;
            _mainWindow.AddNetworkButtonEventClicked -= MainWindow_AddNetworkButtonEventClicked;
            _mainWindow.SavePresetButtonEventClicked -= MainWindowOnSavePresetButtonEventClicked;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Builds the main UI for the mod.
        /// This acts as the <see cref="UIComponent.Start"/> method for components.
        /// </summary>
        public void Initialize()
        {
            try
            {
                BuildUI();
                AttachToEvents();
            }
            catch (Exception e)
            {
                Log._DebugOnlyError($"[{nameof(UIController)}.{nameof(Initialize)}] Loading failed.");
                Log.Exception(e);

                // Fully disable mod's GameComponent
                enabled = false;
            }
        }

        /// <summary>
        /// Destroys the current UI.
        /// This acts as the <see cref="UIComponent.OnDestroy"/> method for components.
        /// </summary>
        public void Cleanup()
        {
            DetachFromEvents();

            // Destroy UI
            Destroy(_mainWindow);
            _mainWindow = null;

            Destroy(_mainButton);
            _mainButton = null;
        }

        public void ResetToolWindowPosition()
        {
            _mainWindow.CenterToParent();
        }

        public void ResetToolButtonPosition()
        {
            _mainButton.ResetPosition();
        }

        /// <summary>
        /// Sets components' visibility based on the current flags that we have on <see cref="ParallelRoadToolManager.ModStatuses"/>.
        /// </summary>
        /// <param name="modStatuses"></param>
        public void UpdateVisibility(ModStatuses modStatuses)
        {
            Log.Info($"[{nameof(UIController)}.{nameof(UpdateVisibility)}] Updating visibility for status: {modStatuses:g}.");

            // Button is visible if mod is enabled
            _mainButton.isVisible = modStatuses.IsFlagSet(ModStatuses.Enabled);

            // Window is visible button is visible and mod is active
            _mainWindow.isVisible = _mainButton.isVisible && modStatuses.IsFlagSet(ModStatuses.Active);
            _mainButton.isChecked = modStatuses.IsFlagSet(ModStatuses.Active);

            Log.Info($"[{nameof(UIController)}.{nameof(UpdateVisibility)}] Visibility set for main components: [{nameof(_mainButton)} = {_mainButton.isVisible}, {nameof(_mainWindow)} = {_mainWindow.isVisible}].");
        }

        public void UpdateCurrentNetwork(NetInfo netInfo)
        {
            _mainWindow.UpdateCurrentNetwork(new NetInfoItem(netInfo));
        }

        public void AddNetwork(NetInfoItem netInfo)
        {
            _mainWindow.AddNetwork(netInfo);
        }

        public void RefreshNetworks(List<NetInfoItem> networks)
        {
            _mainWindow.RefreshNetworks(networks);
        }

        #endregion

        #endregion
    }
}
