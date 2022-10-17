// <copyright file="UIMainWindow.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Main;

using System.Collections.Generic;
using System.Linq;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework;
using ColossalFramework.UI;
using Models;
using UnityEngine;
using Utils;

/// <summary>
///     Main UI for PRT.
///     This window contains:
///     - an header with the closing button
///     - a row with all the tools and features
///     - a list of panels used to display selected NetInfo instances
/// </summary>
public class UIMainWindow : UIPanel
{
    /// <summary>
    ///     Saved X position for the current components. Defaults at -1000 and it's saved in
    ///     <see cref="Constants.SettingsFileName" />
    /// </summary>
    private static readonly SavedInt SavedWindowX = new("windowX", Constants.SettingsFileName, -1000, true);

    /// <summary>
    ///     Saved Y position for the current components. Defaults at -1000 and it's saved in
    ///     <see cref="Constants.SettingsFileName" />
    /// </summary>
    private static readonly SavedInt SavedWindowY = new("windowY", Constants.SettingsFileName, -1000, true);

    private UIButton _closeButton;
    private UIDragHandle _dragHandle;
    private UIButton _savePresetButton;
    private UIButton _loadPresetButton;
    private UINetListPanel _networkListPanel;
    private UICheckBox _toggleSnappingButton;
    private UICheckBox _toggleAngleCompensationButton;
    private UICheckBox _toggleAutoWidthButton;
    private UIButton _addNetworkButton;
    private UIButton _sortNetworksButton;
    private UINetSetupPanel _currentNetworkSetupPanel;

    public event PropertyChangedEventHandler<NetTypeItemEventArgs> NetTypeEventChanged
    {
        add => _networkListPanel.NetTypeEventChanged += value;
        remove => _networkListPanel.NetTypeEventChanged -= value;
    }

    public event MouseEventHandler CloseButtonEventClicked
    {
        add => _closeButton.eventClicked += value;
        remove => _closeButton.eventClicked -= value;
    }

    public event MouseEventHandler AddNetworkButtonEventClicked
    {
        add => _addNetworkButton.eventClicked += value;
        remove => _addNetworkButton.eventClicked -= value;
    }

    public event MouseEventHandler SortNetworksButtonEventClicked
    {
        add => _sortNetworksButton.eventClicked += value;
        remove => _sortNetworksButton.eventClicked -= value;
    }

    public event PropertyChangedEventHandler<int> DeleteNetworkButtonEventClicked
    {
        add => _networkListPanel.DeleteNetworkButtonEventClicked += value;
        remove => _networkListPanel.DeleteNetworkButtonEventClicked += value;
    }

    public event PropertyChangedEventHandler<bool> ToggleSnappingButtonEventCheckChanged
    {
        add => _toggleSnappingButton.eventCheckChanged += value;
        remove => _toggleSnappingButton.eventCheckChanged -= value;
    }

    public event PropertyChangedEventHandler<bool> ToggleAngleCompensationButtonEventCheckChanged
    {
        add => _toggleAngleCompensationButton.eventCheckChanged += value;
        remove => _toggleAngleCompensationButton.eventCheckChanged -= value;
    }

    public event PropertyChangedEventHandler<bool> ToggleAutoWidthButtonEventCheckChanged
    {
        add => _toggleAutoWidthButton.eventCheckChanged += value;
        remove => _toggleAutoWidthButton.eventCheckChanged -= value;
    }

    public event MouseEventHandler SavePresetButtonEventClicked
    {
        add => _savePresetButton.eventClicked += value;
        remove => _savePresetButton.eventClicked -= value;
    }

    public event MouseEventHandler LoadPresetButtonEventClicked
    {
        add => _loadPresetButton.eventClicked += value;
        remove => _loadPresetButton.eventClicked -= value;
    }

    public event ChildComponentEventHandler OnPopupOpened
    {
        add => _networkListPanel.OnPopupOpened += value;
        remove => _networkListPanel.OnPopupOpened -= value;
    }

    public override void Awake()
    {
        base.Awake();

        // Main
        name = $"{Constants.ResourcePrefix}MainWindow";
        backgroundSprite = "UnlockingPanel2";
        size = new Vector2(512, 256);
        autoFitChildrenVertically = true;
        autoLayout = true;
        autoLayoutDirection = LayoutDirection.Vertical;
        autoLayoutPadding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);
        autoLayoutPadding.top = 0;

        // Build contents for this window
        BuildHeader();
        BuildToolbar();

        // Main/CurrentNetwork
        _currentNetworkSetupPanel = AddUIComponent<UINetSetupPanel>();
        _currentNetworkSetupPanel.relativePosition = Vector2.zero;
        _currentNetworkSetupPanel.padding = padding;
        _currentNetworkSetupPanel.FitWidth(this, UIConstants.Padding);
        _currentNetworkSetupPanel.IsReadOnly = true;

        // Main/NetworkList
        _networkListPanel = AddUIComponent<UINetListPanel>();
        _networkListPanel.padding = padding;
        _networkListPanel.FitWidth(this, UIConstants.Padding);

        // Main/Spacer
        AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2f);
    }

    public override void Start()
    {
        base.Start();

        AttachToEvents();

        // Restore saved position, if any, otherwise reset it to default
        if (SavedWindowX.value != -1000 && SavedWindowY.value != -1000)
        {
            absolutePosition = new Vector3(SavedWindowX.value, SavedWindowY.value);
        }
        else
        {
            absolutePosition = new Vector3(300, 300);
        }

        // Since our layout is now complete, we can disable autoLayout for all the panels to avoid wasting CPU cycle
        autoLayout = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        DetachFromEvents();

        // Forcefully destroy all children
        Destroy(_closeButton);
        Destroy(_networkListPanel);
        Destroy(_toggleSnappingButton);
        Destroy(_addNetworkButton);
        Destroy(_currentNetworkSetupPanel);
    }

    /// <summary>
    ///     To refresh networks we just pass the new list to the <see cref="UINetListPanel" /> which will do the rendering.
    ///     This is called ONLY on deletions as adding a new network will trigger <see cref="AddNetwork" />.
    /// </summary>
    /// <param name="networks"></param>
    public void RefreshNetworks(List<NetInfoItem> networks)
    {
        // Before refreshing networks we restore auto-layout to make the panel react to the new element
        autoLayout = true;

        _networkListPanel.RefreshNetworks(networks);

        // Enable the save button only if we have at least one network
        _savePresetButton.isEnabled = networks.Any();

        // Now that networks have been refreshed we can disable auto-layout again
        autoLayout = false;
    }

    /// <summary>
    ///     Re-renders the current network panel with the newly provided <see cref="NetInfoItem" />.
    /// </summary>
    /// <param name="netInfo"></param>
    public void UpdateCurrentNetwork(NetInfoItem netInfo)
    {
        _currentNetworkSetupPanel.Render(netInfo);
    }

    /// <summary>
    ///     Adds the provided <see cref="NetInfoItem" /> to <see cref="UINetListPanel" /> and renders it.
    /// </summary>
    /// <param name="netInfo"></param>
    public void AddNetwork(NetInfoItem netInfo)
    {
        // Before adding a new network we restore auto-layout to make the panel react to the new element
        autoLayout = true;

        _networkListPanel.AddNetwork(netInfo);

        // We have at least one network so we can enable the save button
        _savePresetButton.isEnabled = true;

        // Now that the item has been added we can disable auto-layout again
        autoLayout = false;
    }

    /// <summary>
    ///     Stores current position in mod's preferences
    /// </summary>
    /// <param name="component"></param>
    /// <param name="value"></param>
    private void UIMainWindow_eventPositionChanged(UIComponent component, Vector2 value)
    {
        UpdateSavedPosition();
    }

    private void AttachToEvents()
    {
        eventPositionChanged += UIMainWindow_eventPositionChanged;
    }

    private void DetachFromEvents()
    {
        eventPositionChanged += UIMainWindow_eventPositionChanged;
    }

    private void BuildHeader()
    {
        // Main/Header
        var headerPanel = AddUIComponent<UIPanel>();
        headerPanel.name = $"{name}_Header";
        headerPanel.padding = padding;
        headerPanel.FitWidth(this, UIConstants.Padding);
        headerPanel.height = UIConstants.LargeSize;

        // Main/Header/TitleLabel
        var titleLabel = UILabels.AddLabel(headerPanel, 0f, 13f, Mod.Instance.Name, width, alignment: UIHorizontalAlignment.Center);
        titleLabel.name = $"{headerPanel.name}_TitleLabel";
        titleLabel.height = UIConstants.SmallSize;
        titleLabel.anchor = UIAnchorStyle.CenterVertical | UIAnchorStyle.CenterHorizontal;

        // Main/Header/DragHandle
        _dragHandle = headerPanel.AddUIComponent<UIDragHandle>();
        _dragHandle.name = $"{headerPanel.name}_DragHandle";
        _dragHandle.target = this;
        _dragHandle.FitWidth(this, UIConstants.Padding);
        _dragHandle.height = headerPanel.height;
        _dragHandle.AlignTo(headerPanel, UIAlignAnchor.TopLeft);

        // Main/Header/CloseButton
        _closeButton = headerPanel.AddUIComponent<UIButton>();
        _closeButton.name = $"{headerPanel.name}_CloseButton";
        _closeButton.text = "";
        _closeButton.normalBgSprite = "buttonclose";
        _closeButton.hoveredBgSprite = "buttonclosehover";
        _closeButton.pressedBgSprite = "buttonclosepressed";
        _closeButton.size = new Vector2(UIConstants.SmallSize, UIConstants.SmallSize);
        _closeButton.anchor = UIAnchorStyle.CenterVertical;
        _closeButton.AlignTo(headerPanel, UIAlignAnchor.TopRight);
        _closeButton.relativePosition = new Vector2(_closeButton.relativePosition.x, 4);

        // Main/Header/Icon
        var iconSprite = AddUIComponent<UISprite>();
        iconSprite.height = UIConstants.SmallSize;
        iconSprite.width = UIConstants.SmallSize;
        iconSprite.atlas = UITextures.LoadSingleSpriteAtlas("PRT-Logo-Small");
        iconSprite.spriteName = "normal";
        iconSprite.anchor = UIAnchorStyle.CenterVertical;
        iconSprite.AlignTo(headerPanel, UIAlignAnchor.TopLeft);
        iconSprite.relativePosition = new Vector2(0, 8);
    }

    private void BuildToolbar()
    {
        // Main/Toolbar
        var toolbarPanel = AddUIComponent<UIPanel>();
        toolbarPanel.name = $"{name}_Toolbar";
        toolbarPanel.FitWidth(this, UIConstants.Padding);
        toolbarPanel.height = UIConstants.MediumSize;

        // Main/Toolbar/Options
        var optionsPanel = toolbarPanel.AddUIComponent<UIPanel>();
        optionsPanel.name = $"{toolbarPanel.name}_Options";
        optionsPanel.backgroundSprite = "GenericPanel";
        optionsPanel.color = new Color32(206, 206, 206, 255);
        optionsPanel.size = toolbarPanel.size;
        optionsPanel.width = (optionsPanel.width / 2) - (UIConstants.Padding / 2f);
        optionsPanel.AlignTo(toolbarPanel, UIAlignAnchor.TopLeft);
        optionsPanel.autoLayout = true;
        optionsPanel.autoLayoutDirection = LayoutDirection.Horizontal;

        // Main/Toolbar/Options/SavePresetButton
        _savePresetButton = UIButtons.AddIconButton(optionsPanel, 0, 0, UIConstants.MediumSize, UITextures.LoadQuadSpriteAtlas("PRT-Save"),
                                                    Translations.Translate("TOOLTIP_SAVE_BUTTON"));

        _savePresetButton.name = $"{optionsPanel.name}_SavePreset";
        _savePresetButton.isEnabled = false;

        // Main/Toolbar/Options/LoadPresetButton
        _loadPresetButton = UIButtons.AddIconButton(optionsPanel, 0, 0, UIConstants.MediumSize, UITextures.LoadQuadSpriteAtlas("PRT-Load"),
                                                    Translations.Translate("TOOLTIP_LOAD_BUTTON"));
        _loadPresetButton.name = $"{optionsPanel.name}_LoadPreset";

        // Main/Toolbar/Tools
        var toolsPanel = toolbarPanel.AddUIComponent<UIPanel>();
        toolsPanel.name = $"{toolbarPanel.name}_Tools";
        toolsPanel.backgroundSprite = "GenericPanel";
        toolsPanel.color = new Color32(206, 206, 206, 255);
        toolsPanel.size = toolbarPanel.size;
        toolsPanel.width = (toolsPanel.width / 2) - (UIConstants.Padding / 2f);
        toolsPanel.AlignTo(toolbarPanel, UIAlignAnchor.TopRight);
        toolsPanel.autoLayout = true;
        toolsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
        toolsPanel.autoLayoutStart = LayoutStart.TopRight;

        // Main/Toolbar/Tools/SortNetworksButton
        _sortNetworksButton = UIButtons.AddIconButton(toolsPanel, 0, 0, UIConstants.MediumSize, UITextures.LoadQuadSpriteAtlas("PRT-Sort"),
                                                      Translations.Translate("TOOLTIP_SORT_NETWORKS_BUTTON"));
        _sortNetworksButton.name = $"{toolsPanel.name}_SortNetworks";

        // Main/Toolbar/Tools/ToggleAutoWidthButton
        var autoWidthIcon = UITextures.LoadSpriteAtlas("PRT-AutoWidth", new[] { "AutoWidth", "AutoWidthPressed" });
        _toggleAutoWidthButton = UICheckBoxes.AddIconToggle(toolsPanel, 0, 0, autoWidthIcon.name, "AutoWidthPressed", "AutoWidth",
                                                            backgroundSprite: "OptionBase",
                                                            tooltip: Translations.Translate("TOOLTIP_AUTO_WIDTH_TOGGLE_BUTTON"),
                                                            height: UIConstants.MediumSize, width: UIConstants.MediumSize);
        _toggleAutoWidthButton.name = $"{toolsPanel.name}_ToggleAutoWidth";

        // Main/Toolbar/Tools/ToggleNodeModeButton
        var nodeModeIcon = UITextures.LoadSpriteAtlas("PRT-NodeMove", new[] { "NodeMove", "NodeMovePressed" });
        _toggleAngleCompensationButton = UICheckBoxes.AddIconToggle(toolsPanel, 0, 0, nodeModeIcon.name, "NodeMovePressed", "NodeMove",
                                                                    backgroundSprite: "OptionBase",
                                                                    tooltip: Translations.Translate("TOOLTIP_ANGLE_COMPENSATION_TOGGLE_BUTTON"),
                                                                    height: UIConstants.MediumSize, width: UIConstants.MediumSize);
        _toggleAngleCompensationButton.name = $"{toolsPanel.name}_ToggleNodeMode";

        // Main/Toolbar/Tools/ToggleSnappingButton
        _toggleSnappingButton = UICheckBoxes.AddIconToggle(toolsPanel, 0, 0, UITextures.InGameAtlas.name, "SnappingPressed", "Snapping",
                                                           backgroundSprite: "OptionBase",
                                                           tooltip: Translations.Translate("TOOLTIP_SNAPPING_TOGGLE_BUTTON"),
                                                           height: UIConstants.MediumSize, width: UIConstants.MediumSize);
        _toggleSnappingButton.name = $"{toolsPanel.name}_ToggleSnapping";

        // Main/Toolbar/Tools/AddNetworkButton
        _addNetworkButton = UIButtons.AddIconButton(toolsPanel, 0, 0, UIConstants.MediumSize, UITextures.LoadQuadSpriteAtlas("PRT-Add"),
                                                    Translations.Translate("TOOLTIP_ADD_NETWORK_BUTTON"));
        _addNetworkButton.name = $"{toolsPanel.name}_AddNetwork";

        // Since our layout is now complete, we can disable autoLayout for all the panels to avoid wasting CPU cycle
        optionsPanel.autoLayout = false;
        toolsPanel.autoLayout = false;
    }

    private void UpdateSavedPosition()
    {
        SavedWindowX.value = (int)absolutePosition.x;
        SavedWindowY.value = (int)absolutePosition.y;
    }
}
