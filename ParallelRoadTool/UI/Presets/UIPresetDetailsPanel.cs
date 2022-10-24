// <copyright file="UIPresetDetailsPanel.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Presets;

using System.Collections.Generic;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using Models;
using Shared;
using Utils;

/// <summary>
///     This <see cref="UIPanel" /> shows a <see cref="UIList" /> with all the <see cref="NetInfoItem" /> contained in a
///     given preset file.
/// </summary>
internal class UIPresetDetailsPanel : UIPanel
{
    private UIList _netItemsList;

    public UIPresetDetailsPanel()
    {
        autoLayoutDirection = LayoutDirection.Vertical;
        autoLayoutPadding = UIHelpers.RectOffsetFromPadding(0);
        autoLayout = true;
    }

    public void ClearPreset()
    {
        _netItemsList.Clear();
    }

    public void LoadPreset(IEnumerable<NetInfoItem> networks)
    {
        var items = new FastList<object>();
        foreach (var netItem in networks)
        {
            items.Add(netItem);
        }

        _netItemsList.Data = items;
    }

    public override void Start()
    {
        base.Start();

        // We need to create this here because this panel's size is set by its container and is not know during ctor
        _netItemsList = UIList.AddUIList<UINetItemMediumListRow>(this, 0, 0, width, height, UIConstants.MediumSize);

        // Force disable selection
        _netItemsList.EventSelectionChanged += (_, _) => { _netItemsList.SelectedIndex = -1; };
    }
}
