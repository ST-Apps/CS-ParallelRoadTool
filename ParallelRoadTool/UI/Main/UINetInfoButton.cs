// <copyright file="UINetInfoButton.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Main;

using ColossalFramework.UI;
using Models;
using Shared;
using UnityEngine;
using Utils;

/// <summary>
///     This class is meant as a wrapper to <see cref="UINetInfoPanel" /> which adds an <see cref="UIButton" /> used to
///     toggle the <see cref="UINetSelectionPopup" /> instance.
/// </summary>

// ReSharper disable once ClassNeverInstantiated.Global
internal class UINetInfoButton : UIPanel
{
    /// <summary>
    ///     Used to prevent concurrent executions on <see cref="ToggleButtonOnEventClicked" />.
    /// </summary>
    private static readonly object Lock = new ();

    private UINetInfoPanel _netInfoPanel;
    private UIButton _toggleButton;
    private UINetSelectionPopup _netSelectionPopup;

    public event ChildComponentEventHandler OnPopupOpened;

    public event PropertyChangedEventHandler<NetTypeItemEventArgs> OnPopupSelectionChanged;

    /// <summary>
    ///     If this is true it means that the button must not be visible.
    /// </summary>
    public bool IsReadOnly
    {
        set => _toggleButton.isVisible = !value;
    }

    public override void Awake()
    {
        base.Awake();

        // Main
        clipChildren = true;
        size = new Vector2(400, 48 + UIConstants.Padding + UIConstants.Padding);
        relativePosition = Vector3.zero;

        // Main/NetInfo
        _netInfoPanel = AddUIComponent<UINetInfoPanel>();
        _netInfoPanel.anchor = UIAnchorStyle.CenterVertical;

        // Main/ToggleButton
        _toggleButton = AddUIComponent<UIButton>();
        _toggleButton.size = _netInfoPanel.size;
        _toggleButton.relativePosition = Vector3.zero;
        _toggleButton.normalBgSprite = "OptionsDropbox";
        _toggleButton.hoveredBgSprite = "ButtonWhite";
        _toggleButton.focusedBgSprite = "OptionsDropboxHovered";
        _toggleButton.pressedBgSprite = "OptionsDropboxHovered";
        _toggleButton.opacity = 0.25f;
    }

    public override void Start()
    {
        base.Start();

        // Manually fix some sizing and positioning to have the button overlap the net info panel
        _toggleButton.FitWidth(_netInfoPanel, UIConstants.Padding);
        _netInfoPanel.relativePosition = Vector3.zero;

        AttachToEvents();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        DetachFromEvents();

        // Forcefully destroy all children
        Destroy(_netInfoPanel);
        Destroy(_toggleButton);
    }

    /// <summary>
    ///     Renders the current component by performing the required render steps for the internal
    ///     <see cref="UINetInfoPanel" />.
    ///     If this item is in a list we also change its sprites and color.
    /// </summary>
    /// <param name="netInfo"></param>
    /// <param name="inList"></param>
    public void Render(NetInfoItem netInfo, bool inList = false)
    {
        if (inList)
        {
            _toggleButton.normalBgSprite = "GenericPanel";
            _toggleButton.hoveredBgSprite = "ButtonWhite";
            _toggleButton.focusedBgSprite = "GenericPanel";
            _toggleButton.pressedBgSprite = "GenericPanel";
            _toggleButton.color = netInfo.Color;
            _toggleButton.opacity = 0.25f;
        }

        _netInfoPanel.NetInfoItem = netInfo;
    }

    private void ToggleButtonOnEventClicked(UIComponent component, UIMouseEventParameter eventParam)
    {
        lock (Lock)
        {
            // Close the selection popup if any
            if (_netSelectionPopup != null)
            {
                _netSelectionPopup.Close();
                _netSelectionPopup = null;
                return;
            }

            // Get the popup
            _netSelectionPopup = UIView.GetAView().AddUIComponent(typeof(UINetSelectionPopup)) as UINetSelectionPopup;
            if (_netSelectionPopup == null)
            {
                return;
            }

            // Register events
            _netSelectionPopup.OnPopupSelectionChanged += (_, value) =>
                                                          {
                                                              if (value.SelectedNetworkName == _netInfoPanel.NetInfoItem.Name)
                                                              {
                                                                  return;
                                                              }

                                                              OnPopupSelectionChanged?.Invoke(_netSelectionPopup, value);
                                                              _netSelectionPopup.Close();
                                                          };

            // Open the popup
            _netSelectionPopup.Open(this, _netInfoPanel.NetInfoItem);
            OnPopupOpened?.Invoke(this, _netSelectionPopup);
        }
    }

    private void AttachToEvents()
    {
        _toggleButton.eventClicked += ToggleButtonOnEventClicked;
    }

    private void DetachFromEvents()
    {
        _toggleButton.eventClicked -= ToggleButtonOnEventClicked;
    }
}
