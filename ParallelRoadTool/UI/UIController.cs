using ColossalFramework;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Utils;
using ParallelRoadTool.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UIController : MonoBehaviour
    {

        #region Components

        /// <summary>
        ///     Main UI panel.
        /// </summary>
        private UIMainWindow _mainWindow;

        /// <summary>
        /// Main UI button.
        /// </summary>
        private UIToolToggleButton _toolToggleButton;

        //private UINetListPopup _netPopup;

        #endregion

        #region Events/Callbacks

        public event EventHandler CloseButtonEventClicked;
        public event EventHandler AddNetworkButtonEventClicked;

        public event PropertyChangedEventHandler<NetTypeItemEventArgs> NetTypeEventChanged
        {
            add { _mainWindow.NetTypeEventChanged += value; }
            remove { _mainWindow.NetTypeEventChanged -= value; }
        }

        public event PropertyChangedEventHandler<bool> ToolToggleButtonEventCheckChanged
        {
            add { _toolToggleButton.eventCheckChanged += value; }
            remove { _toolToggleButton.eventCheckChanged -= value; }
        }

        public event PropertyChangedEventHandler<bool> ToggleSnappingButtonEventCheckChanged
        {
            add { _mainWindow.ToggleSnappingButtonEventCheckChanged += value; }
            remove { _mainWindow.ToggleSnappingButtonEventCheckChanged -= value; }
        }

        public event PropertyChangedEventHandler<int> DeleteNetworkButtonEventClicked
        {
            add { _mainWindow.DeleteNetworkButtonEventClicked += value; }
            remove { _mainWindow.DeleteNetworkButtonEventClicked += value; }
        }

        public event PropertyChangedEventHandler<float> OnHorizontalOffsetKeypress;
        public event PropertyChangedEventHandler<float> OnVerticalOffsetKeypress;

        private void MainWindow_CloseButtonEventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            CloseButtonEventClicked?.Invoke(null, null);
        }

        private void MainWindow_AddNetworkButtonEventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            AddNetworkButtonEventClicked?.Invoke(null, null);
        }

        private void MainWindowOnToggleDropdownButtonEventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            //_netPopup.Toggle(component);
        }

        #endregion

        #region Lifecycle

        public void OnGUI()
        {
            if (UIView.HasModalInput()
                || UIView.HasInputFocus()
                || !Singleton<ParallelRoadTool>.exists
                || !(ToolsModifierControl.toolController.CurrentTool is NetTool))
                return;

            var e = Event.current;

            if (e.isMouse)
            {
                // HACK - [ISSUE-84] Report if we're currently having a long mouse press
                Singleton<ParallelRoadTool>.instance.IsMouseLongPress = e.type switch
                {
                    EventType.MouseDown => true,
                    EventType.MouseUp => false,
                    _ => Singleton<ParallelRoadTool>.instance.IsMouseLongPress
                };

                Log._Debug($"[{nameof(UIMainWindow)}.{nameof(OnGUI)}] Setting {nameof(Singleton<ParallelRoadTool>.instance.IsMouseLongPress)} to {Singleton<ParallelRoadTool>.instance.IsMouseLongPress}");
            }

            // Checking key presses
            if (OptionsKeymapping.ToggleParallelRoads.IsPressed(e)) ToggleModStatus();

            if (OptionsKeymapping.DecreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(-1f);

            if (OptionsKeymapping.IncreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(1f);

            if (OptionsKeymapping.DecreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(-1f, false);

            if (OptionsKeymapping.IncreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(1f, false);
        }

        private void ToggleModStatus()
        {
            Singleton<ParallelRoadTool>.instance.ToggleModStatus();
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
        /// Builds the main UI for the mod.
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

                enabled = false;
            }
        }

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

            var button = UIUtil.FindComponent<UICheckBox>($"{Configuration.ResourcePrefix}Parallel");
            if (button != null) DestroyImmediate(button);
            _toolToggleButton = view.AddUIComponent(typeof(UIToolToggleButton)) as UIToolToggleButton;

            Log._Debug($"[{nameof(UIController)}.{nameof(BuildUI)}] Mod's main button created.");

            Log._Debug($"[{nameof(UIController)}.{nameof(BuildUI)}] Creating mod's dropdown popup...");

            //_netPopup ??= view.FindUIComponent<UINetListPopup>($"{Configuration.ResourcePrefix}NetworksPopup");
            //if (_netPopup != null)
            //    DestroyImmediate(_netPopup);
            //_netPopup = view.AddUIComponent(typeof(UINetListPopup)) as UINetListPopup;

            Log._Debug($"[{nameof(UIController)}.{nameof(BuildUI)}] Mod's dropdown popup.");
        }

        public void Cleanup()
        {
            // Destroy UI
            Destroy(_mainWindow);
            _mainWindow = null;

            Destroy(_toolToggleButton);
            _toolToggleButton = null;
        }

        private void AttachToEvents()
        {
            _mainWindow.CloseButtonEventClicked += MainWindow_CloseButtonEventClicked;
            _mainWindow.AddNetworkButtonEventClicked += MainWindow_AddNetworkButtonEventClicked;
            _mainWindow.ToggleDropdownButtonEventClick += MainWindowOnToggleDropdownButtonEventClick;
        }

        private void DetachFromEvents()
        {
            _mainWindow.CloseButtonEventClicked -= MainWindow_CloseButtonEventClicked;
            _mainWindow.AddNetworkButtonEventClicked -= MainWindow_AddNetworkButtonEventClicked;
            _mainWindow.ToggleDropdownButtonEventClick -= MainWindowOnToggleDropdownButtonEventClick;
        }

        #endregion

        #region Control

        public void ResetToolWindowPosition()
        {
            _mainWindow.absolutePosition = new Vector3(100, 100);
        }

        public void ResetToolButtonPosition()
        {
            _toolToggleButton.ResetPosition();
        }

        public void UpdateVisibility(ModStatuses modStatuses)
        {
            Log.Info($"[{nameof(UIController)}.{nameof(UpdateVisibility)}] Updating visibility for status: {modStatuses:g}.");

            // Button is visible if mod is enabled
            _toolToggleButton.isVisible = modStatuses.IsFlagSet(ModStatuses.Enabled);

            // Window is visible button is visible and mod is active
            _mainWindow.isVisible = _toolToggleButton.isVisible && modStatuses.IsFlagSet(ModStatuses.Active);
            _toolToggleButton.isChecked = modStatuses.IsFlagSet(ModStatuses.Active);

            Log.Info($"[{nameof(UIController)}.{nameof(UpdateVisibility)}] Visibility set for main components: [{nameof(_toolToggleButton)} = {_toolToggleButton.isVisible}, {nameof(_mainWindow)} = {_mainWindow.isVisible}].");
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
    }
}
