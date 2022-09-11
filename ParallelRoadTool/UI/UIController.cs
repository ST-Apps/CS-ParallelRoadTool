using ColossalFramework;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.Utils;
using System;
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

        #endregion

        #region Events

        public event PropertyChangedEventHandler<bool> ToolToggleButtonEventCheckChanged
        {
            add { _toolToggleButton.eventCheckChanged += value; }
            remove { _toolToggleButton.eventCheckChanged -= value; }
        }

        #endregion

        #region Lifecycle

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

        }

        private void DetachFromEvents()
        {

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

            Log.Info($"[{nameof(UIController)}.{nameof(UpdateVisibility)}] Visibility set for main components: [{nameof(_toolToggleButton)} = {_toolToggleButton.isVisible}, {nameof(_mainWindow)} = {_mainWindow.isVisible}].");
        }

        #endregion
    }
}
