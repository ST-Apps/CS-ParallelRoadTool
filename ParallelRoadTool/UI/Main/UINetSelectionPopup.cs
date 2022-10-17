// <copyright file="UINetSelectionPopup.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Main;

using System.Collections.Generic;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using Models;
using Shared;
using UnityEngine;
using Utils;

// ReSharper disable ClassNeverInstantiated.Local
/// <summary>
///     A popup showing a list of <see cref="NetInfoItem" /> to be selected.
/// </summary>
internal class UINetSelectionPopup : UIPanel
{
    private readonly UITextField _searchTextField;

    /// <summary>
    ///     This is the <see cref="NetInfoItem" /> that was selected in popup's owner.
    ///     Used to pre-select an item in the list.
    /// </summary>
    private NetInfoItem _ownerItem;

    private UIComponent _owner;
    private UIList _netItemsList;
    private FastList<object> _netItems;

    public UINetSelectionPopup()
    {
        // Main
        backgroundSprite = "OptionsDropboxListbox";
        zOrder = int.MaxValue;
        clipChildren = true;
        autoLayout = true;
        autoLayoutPadding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);
        autoLayoutDirection = LayoutDirection.Vertical;

        // Main/SearchText
        _searchTextField = UITextFields.AddBigTextField(this, 0, 0);

        height = _searchTextField.height + (4 * UIConstants.LargeSize) + (4 * UIConstants.Padding);

        AttachToEvents();
    }

    public event PropertyChangedEventHandler<NetTypeItemEventArgs> OnPopupSelectionChanged;

    /// <summary>
    ///     Forces closing the popup if user click anywhere outside.
    /// </summary>
    public override void LateUpdate()
    {
        var ray = GetCamera().ScreenPointToRay(Input.mousePosition);

        if (!Input.GetMouseButtonDown(0) || Raycast(ray) || _owner.Raycast(ray) || _netItemsList.Raycast(ray))
        {
            return;
        }

        Close();
    }

    public override void OnDestroy()
    {
        DetachFromEvents();

        base.OnDestroy();
    }

    /// <summary>
    ///     Opens the popup and registers both the caller and its <see cref="NetInfoItem" />.
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="currentItem"></param>
    public void Open(UIComponent owner, NetInfoItem currentItem = null)
    {
        _owner = owner;

        // Move it to the correct position
        absolutePosition = owner.absolutePosition + new Vector3(0, owner.height);

        // Set width to match its owner
        width = owner.width - (2 * UIConstants.Padding);
        _searchTextField.FitWidth(this, UIConstants.Padding);

        // Make it appear above all the other components
        BringToFront();
        _searchTextField.Focus();

        _ownerItem = currentItem;
    }

    /// <summary>
    ///     Closed the popup and destroys all the resources
    /// </summary>
    public void Close()
    {
        isVisible = false;

        DestroyImmediate(_netItemsList);
        DestroyImmediate(this);
    }

    /// <summary>
    ///     Loads the provided collection of <see cref="NetInfo" /> into our <see cref="UIList" />.
    /// </summary>
    /// <param name="networks"></param>
    public void LoadNetworks(IEnumerable<NetInfo> networks)
    {
        var items = new FastList<object>();
        foreach (var netItem in networks)
        {
            items.Add(new NetInfoItem(netItem));
        }

        if (_netItemsList == null)
        {
            // We need to create this here because this panel's size is set by its container and is not know during ctor
            _netItemsList = UIList.AddUIList<UINetItemLargeListRow>(this, 0, 0, width - (2 * UIConstants.Padding),
                                                                    (4 * UIConstants.LargeSize) - UIConstants.Padding, UIConstants.LargeSize);
            _netItemsList.BackgroundSprite = null;

            // We also register the selection changed event here
            _netItemsList.EventSelectionChanged += NetItemsListOnEventSelectionChanged;
        }

        _netItemsList.Data = items;
        _netItems = items;

        // Pre-select
        if (_ownerItem != null)
        {
            _netItemsList.FindItem<NetInfoItem>(i => i.BeautifiedName == _ownerItem.BeautifiedName);
        }
    }

    /// <summary>
    ///     Forces closing the popup if user presses the <see cref="KeyCode.Escape" /> key.
    /// </summary>
    /// <param name="p"></param>
    protected override void OnKeyDown(UIKeyEventParameter p)
    {
        if (!Input.GetKey(KeyCode.Escape))
        {
            return;
        }

        p.Use();
        Close();
    }

    /// <summary>
    ///     Filters the current list using the provided query.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="searchQuery"></param>
    private void SearchTextFieldOnEventTextChanged(UIComponent component, string searchQuery)
    {
        if (string.IsNullOrEmpty(searchQuery))
        {
            // Empty query means we need to show everything we have
            _netItemsList.Data = _netItems;
        }
        else
        {
            var filteredData = new FastList<object>();

            // We iterate over the available items and populate a list with all the matching items
            foreach (var netObject in _netItems)
            {
                var netInfo = (NetInfoItem)netObject;
                if (netInfo.Name.ToLower().Contains(searchQuery) || netInfo.BeautifiedName.ToLower().Contains(searchQuery))
                {
                    filteredData.Add(netInfo);
                }
            }

            // We set filtered data as current data and reset our scroll position.
            _netItemsList.Data = filteredData;
            _netItemsList.CurrentPosition = 0;
        }
    }

    private void AttachToEvents()
    {
        _searchTextField.eventTextChanged += SearchTextFieldOnEventTextChanged;
    }

    private void DetachFromEvents()
    {
        _searchTextField.eventTextChanged -= SearchTextFieldOnEventTextChanged;
    }

    private void NetItemsListOnEventSelectionChanged(UIComponent component, object value)
    {
        var netInfoItem = (NetInfoItem)value;
        OnPopupSelectionChanged?.Invoke(null, new NetTypeItemEventArgs(-1, netInfoItem.Name));
    }
}
