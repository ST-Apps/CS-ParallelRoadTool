// <copyright file="UINetItemListRow.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Shared;

using AlgernonCommons.UI;
using Models;
using UnityEngine;
using Utils;

/// <summary>
///     Row component for a <see cref="UIList" /> displaying <see cref="NetInfoItem" /> wrapped in a
///     <see cref="UINetInfoTinyPanel" />.
/// </summary>
internal class UINetItemListRow : UIListRow
{
    private UINetInfoTinyPanel _netInfoRow;

    /// <summary>
    ///     Generates and displays contents for the provided data object.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="rowIndex"></param>
    public override void Display(object data, int rowIndex)
    {
        if (_netInfoRow == null)
        {
            // Init our row
            width = parent.width;
            height = RowHeight;
            isInteractive = false;

            // Set the item
            _netInfoRow = AddUIComponent<UINetInfoTinyPanel>();
            _netInfoRow.relativePosition = Vector2.zero;
            _netInfoRow.isInteractive = false;
        }

        // Set the current item
        _netInfoRow.NetInfoItem = data as NetInfoItem;

        // Deselect to reset its style in case it was selected before
        Deselect(rowIndex);
    }

    /// <summary>
    ///     Sets the row display to the selected state (highlighted).
    /// </summary>
    public override void Select()
    {
        BackgroundOpacity = 1f;
    }

    /// <summary>
    ///     Sets the row display to the deselected state.
    /// </summary>
    /// <param name="rowIndex">Row index number (for background banding).</param>
    public override void Deselect(int rowIndex)
    {
        BackgroundSpriteName = "GenericPanel";
        BackgroundColor = _netInfoRow.color;
        BackgroundOpacity = 0.25f;
    }
}

/// <summary>
///     Medium-sized height <see cref="UINetItemListRow" />.
/// </summary>
internal class UINetItemMediumListRow : UINetItemListRow
{
    public override float RowHeight => UIConstants.MediumSize;
}

/// <summary>
///     Large-sized height <see cref="UINetItemListRow" />.
/// </summary>
internal class UINetItemLargeListRow : UINetItemListRow
{
    public override float RowHeight => UIConstants.LargeSize;
}
