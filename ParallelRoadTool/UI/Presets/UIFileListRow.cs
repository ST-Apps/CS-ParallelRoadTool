// <copyright file="UIFileListRow.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Presets;

using AlgernonCommons.UI;
using ColossalFramework.UI;
using Utils;

// ReSharper disable ClassNeverInstantiated.Global
/// <summary>
///     Row for a <see cref="UIList" /> item containing a file name.
/// </summary>
internal sealed class UIFileListRow : UIListRow
{
    private UILabel _fileName;

    /// <summary>
    ///     To display the row we simply render a <see cref="UILabel" /> containing our data.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="rowIndex"></param>
    public override void Display(object data, int rowIndex)
    {
        if (_fileName == null)
        {
            // Init our row
            width = parent.width;
            height = RowHeight;

            // Set the label
            _fileName = AddLabel(UIConstants.Padding, width - (2 * UIConstants.Padding), wordWrap: true);
        }

        _fileName.text = (string)data;
        Deselect(rowIndex);
    }
}
