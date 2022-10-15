using System;
using System.Collections.Generic;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Models;
using ParallelRoadTool.Settings;
using ParallelRoadTool.UI.Main;
using ParallelRoadTool.UI.Presets;
using UnityEngine;

// ReSharper disable once ClassNeverInstantiated.Global

namespace ParallelRoadTool.UI;

/// <summary>
///     Main controller for the UI-related components of the mod.
/// </summary>
public class UIController : MonoBehaviour
{
    #region Events

    public event EventHandler CloseButtonEventClicked;
    public event EventHandler AddNetworkButtonEventClicked;
    public event EventHandler SortNetworksButtonEventClicked;

    public event PropertyChangedEventHandler<float> OnHorizontalOffsetKeypress;
    public event PropertyChangedEventHandler<float> OnVerticalOffsetKeypress;

    public event PropertyChangedEventHandler<NetTypeItemEventArgs> NetTypeEventChanged
    {
        add => _mainWindow.NetTypeEventChanged += value;
        remove => _mainWindow.NetTypeEventChanged -= value;
    }

    public event PropertyChangedEventHandler<bool> ToolToggleButtonEventCheckChanged
    {
        add => _mainButton.EventCheckChanged += value;
        remove => _mainButton.EventCheckChanged -= value;
    }

    public event PropertyChangedEventHandler<bool> ToggleSnappingButtonEventCheckChanged
    {
        add => _mainWindow.ToggleSnappingButtonEventCheckChanged += value;
        remove => _mainWindow.ToggleSnappingButtonEventCheckChanged -= value;
    }

    public event PropertyChangedEventHandler<bool> ToggleAngleCompensationButtonEventCheckChanged
    {
        add => _mainWindow.ToggleAngleCompensationButtonEventCheckChanged += value;
        remove => _mainWindow.ToggleAngleCompensationButtonEventCheckChanged -= value;
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

    private void MainWindowOnSortNetworksButtonEventClicked(UIComponent component, UIMouseEventParameter eventParam)
    {
        SortNetworksButtonEventClicked?.Invoke(null, null);
    }

    private void MainWindowOnSavePresetButtonEventClicked(UIComponent component, UIMouseEventParameter eventParam)
    {
        // Prevent focus for main window
        _mainWindow.canFocus = false;

        StandalonePanelManager<UISavePresetWindow>.Create();

        // Subscribe to events for the modal popup. We don't need to unsubscribe because this one will be destroyed on close
        StandalonePanelManager<UISavePresetWindow>.Panel.SaveButtonEventClicked += PanelOnSaveButtonEventClicked;
        StandalonePanelManager<UISavePresetWindow>.Panel.EventClose += () =>
                                                                       {
                                                                           // Restore focus for main window on modal close
                                                                           _mainWindow.canFocus = true;
                                                                           _mainWindow.Focus();
                                                                       };

        // Load data
        StandalonePanelManager<UISavePresetWindow>.Panel.RefreshItems(PresetsManager.ListSavedFiles());
    }

    private void PanelOnSaveButtonEventClicked(UIComponent component, string fileName)
    {
        // Save the current preset using PresetsManager
        try
        {
            Singleton<ParallelRoadToolManager>.instance.SavePreset(fileName);

            // We can now close the save modal
            StandalonePanelManager<UISavePresetWindow>.Panel.Close();
        }
        catch (Exception e)
        {
            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(Translations.Translate("LABEL_SAVE_PRESET_FAILED_TITLE"),
                                                                                  string
                                                                                      .Format(Translations.Translate("LABEL_SAVE_PRESET_FAILED_MESSAGE"),
                                                                                              fileName, e), true);
        }
    }

    private void MainWindowOnLoadPresetButtonEventClicked(UIComponent component, UIMouseEventParameter eventParam)
    {
        // Prevent focus for main window
        _mainWindow.canFocus = false;

        StandalonePanelManager<UILoadPresetWindow>.Create();

        // Subscribe to events for the modal popup. We don't need to unsubscribe because this one will be destroyed on close
        StandalonePanelManager<UILoadPresetWindow>.Panel.LoadButtonEventClicked += PanelOnLoadButtonEventClicked;
        StandalonePanelManager<UILoadPresetWindow>.Panel.EventClose += () =>
                                                                       {
                                                                           // Restore focus for main window on modal close
                                                                           _mainWindow.canFocus = true;
                                                                           _mainWindow.Focus();
                                                                       };

        // Load data
        StandalonePanelManager<UILoadPresetWindow>.Panel.RefreshItems(PresetsManager.ListSavedFiles());
    }

    private void PanelOnLoadButtonEventClicked(UIComponent component, string fileName)
    {
        try
        {
            Singleton<ParallelRoadToolManager>.instance.LoadPreset(fileName);

            // We can now close the load modal
            StandalonePanelManager<UILoadPresetWindow>.Panel.Close();
        }
        catch (Exception e)
        {
            UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(Translations.Translate("LABEL_LOAD_PRESET_FAILED_TITLE"),
                                                                                  string
                                                                                      .Format(Translations.Translate("LABEL_LOAD_PRESET_FAILED_MESSAGE"),
                                                                                              fileName, e), true);
        }
    }

    private void MainWindowOnOnPopupOpened(UIComponent container, UIComponent child)
    {
        // Load data into the provided child popup
        var popup = (UINetSelectionPopup)child;
        popup.LoadNetworks(Singleton<ParallelRoadToolManager>.instance.AvailableRoadTypes);
    }

    #endregion

    #region Unity

    #region Components

    /// <summary>
    ///     Main UI panel.
    /// </summary>
    private static UIMainWindow _mainWindow;

    /// <summary>
    ///     Main UI button.
    /// </summary>
    private static UIToolToggleButton _mainButton;

    #endregion

    #region Lifecycle

    /// <summary>
    ///     Check for key-bindings and activate their corresponding action.
    /// </summary>
    public void OnGUI()
    {
        if (UIView.HasModalInput() || UIView.HasInputFocus() || !Singleton<ParallelRoadToolManager>.exists ||
            !(ToolsModifierControl.toolController.CurrentTool is NetTool))
            return;

        var e = Event.current;

        if (e.isMouse)
        {
            // HACK - [ISSUE-84] Report if we're currently having a long mouse press
            Singleton<ParallelRoadToolManager>.instance.IsMouseLongPress = e.type switch
            {
                EventType.MouseDown => true,
                EventType.MouseUp   => false,
                _                   => Singleton<ParallelRoadToolManager>.instance.IsMouseLongPress
            };

            Log._Debug($"[{nameof(UIMainWindow)}.{nameof(OnGUI)}] Setting {nameof(Singleton<ParallelRoadToolManager>.instance.IsMouseLongPress)} to {Singleton<ParallelRoadToolManager>.instance.IsMouseLongPress}");
        }

        // Checking key presses
        if (ModSettings.KeyToggleTool.IsPressed(e)) ToggleModStatus();
        if (ModSettings.KeyIncreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(1f);
        if (ModSettings.KeyDecreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(-1f);
        if (ModSettings.KeyIncreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(1f,  false);
        if (ModSettings.KeyDecreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(-1f, false);

        // Check for modifiers too
        ModifiersManager.IsShiftPressed = e.shift;
        ModifiersManager.IsCtrlPressed  = e.control;
        ModifiersManager.IsAltPressed   = e.alt;
    }

    #endregion

    #endregion

    #region Control

    #region Internals

    /// <summary>
    ///     Toggles the current mod status on <see cref="ParallelRoadToolManager" />.
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
    ///     Builds mod top-level UI components and attaches them to the current view.
    ///     The components are:
    ///     <list type="bullet">
    ///         <item>
    ///             <see cref="UIMainWindow" />
    ///         </item>
    ///         <item>
    ///             <see cref="UIToolToggleButton" />
    ///         </item>
    ///     </list>
    /// </summary>
    private void BuildUI()
    {
        var view = UIView.GetAView();

        Log._Debug($"[{nameof(UIController)}.{nameof(BuildUI)}] Creating mod's main window...");

        _mainWindow ??= view.FindUIComponent<UIMainWindow>($"{Constants.ResourcePrefix}MainWindow");
        if (_mainWindow != null)
            DestroyImmediate(_mainWindow);
        _mainWindow = view.AddUIComponent(typeof(UIMainWindow)) as UIMainWindow;

        Log._Debug($"[{nameof(UIController)}.{nameof(BuildUI)}] Mod's main window created.");

        Log._Debug($"[{nameof(UIController)}.{nameof(BuildUI)}] Creating mod's main button...");

        var button = view.FindUIComponent<UIToolToggleButton>($"{Constants.ResourcePrefix}Parallel");
        if (button != null) DestroyImmediate(button);
        _mainButton = view.AddUIComponent(typeof(UIToolToggleButton)) as UIToolToggleButton;

        Log._Debug($"[{nameof(UIController)}.{nameof(BuildUI)}] Mod's main button created.");
    }

    private void AttachToEvents()
    {
        _mainWindow.CloseButtonEventClicked        += MainWindow_CloseButtonEventClicked;
        _mainWindow.AddNetworkButtonEventClicked   += MainWindow_AddNetworkButtonEventClicked;
        _mainWindow.SortNetworksButtonEventClicked += MainWindowOnSortNetworksButtonEventClicked;
        _mainWindow.SavePresetButtonEventClicked   += MainWindowOnSavePresetButtonEventClicked;
        _mainWindow.LoadPresetButtonEventClicked   += MainWindowOnLoadPresetButtonEventClicked;
        _mainWindow.OnPopupOpened                  += MainWindowOnOnPopupOpened;
    }

    private void DetachFromEvents()
    {
        _mainWindow.CloseButtonEventClicked        -= MainWindow_CloseButtonEventClicked;
        _mainWindow.AddNetworkButtonEventClicked   -= MainWindow_AddNetworkButtonEventClicked;
        _mainWindow.SortNetworksButtonEventClicked -= MainWindowOnSortNetworksButtonEventClicked;
        _mainWindow.SavePresetButtonEventClicked   -= MainWindowOnSavePresetButtonEventClicked;
        _mainWindow.LoadPresetButtonEventClicked   -= MainWindowOnLoadPresetButtonEventClicked;
        _mainWindow.OnPopupOpened                  -= MainWindowOnOnPopupOpened;
    }

    #endregion

    #region Public API

    /// <summary>
    ///     Builds the main UI for the mod.
    ///     This acts as the <see cref="UIComponent.Start" /> method for components.
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
    ///     Destroys the current UI.
    ///     This acts as the <see cref="UIComponent.OnDestroy" /> method for components.
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
    ///     Sets components' visibility based on the current flags that we have on
    ///     <see cref="ParallelRoadToolManager.ModStatuses" />.
    /// </summary>
    /// <param name="modStatuses"></param>
    public void UpdateVisibility(ModStatuses modStatuses)
    {
        Log.Info($"[{nameof(UIController)}.{nameof(UpdateVisibility)}] Updating visibility for status: {modStatuses:g}.");

        // Button is visible if mod is enabled
        _mainButton.isVisible = modStatuses.IsFlagSet(ModStatuses.Enabled);

        // Window is visible button is visible and mod is active
        _mainWindow.isVisible = _mainButton.isVisible && modStatuses.IsFlagSet(ModStatuses.Active);
        _mainButton.IsChecked = modStatuses.IsFlagSet(ModStatuses.Active);

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
