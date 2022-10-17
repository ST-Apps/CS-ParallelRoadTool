// <copyright file="UINetInfoPanel.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Shared;

using ColossalFramework.UI;
using Models;
using UnityEngine;
using Utils;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
///     This is the main panel used to render a <see cref="NetInfoItem" /> as both its thumbnail and its name.
/// </summary>
public class UINetInfoPanel : UIPanel
{
    protected readonly UISprite Thumbnail;
    protected readonly UILabel Label;

    private NetInfoItem _netInfoItem;

    public UINetInfoPanel()
    {
        // Main
        name = $"{Constants.ResourcePrefix}NetInfo";
        size = UIConstants.NetInfoPanelLargeSize;
        autoLayout = true;
        autoLayoutDirection = LayoutDirection.Horizontal;
        autoLayoutPadding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);

        // We don't want padding on right side
        autoLayoutPadding.right = 0;

        // Main/Thumbnail
        Thumbnail = AddUIComponent<UISprite>();
        Thumbnail.size = UIConstants.ThumbnailLargeSize;

        // Main/Label
        Label = AddUIComponent<UILabel>();
        Label.textScale = .8f;
        Label.verticalAlignment = UIVerticalAlignment.Middle;
        Label.autoSize = false;
        Label.wordWrap = true;

        // Label should fill up the remaining space
        // x is 5 * padding because we have one at the beginning of the row, one between thumbnail and label, one at the end of the row, and we have also to consider that the entire panel is padded twice.
        Label.size = UIConstants.NetInfoPanelLargeSize - new Vector2(Thumbnail.width + (5 * UIConstants.Padding), 2 * UIConstants.Padding);
    }

    public NetInfoItem NetInfoItem
    {
        get => _netInfoItem;
        set
        {
            if (_netInfoItem != null && _netInfoItem.BeautifiedName == value.BeautifiedName)
            {
                return;
            }

            _netInfoItem = value;

            Thumbnail.atlas = _netInfoItem.Atlas;
            Thumbnail.spriteName = _netInfoItem.Thumbnail;
            Label.text = _netInfoItem.BeautifiedName;

            color = _netInfoItem.Color;
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        // Forcefully destroy all children
        Destroy(Thumbnail);
        Destroy(Label);
    }

    ///// <summary>
    ///// To render a <see cref="NetInfoItem"/> we just to set both atlas and spriteName for <see cref="Thumbnail"/>, as well as the provided network name.
    ///// </summary>
    ///// <param name="netInfo"></param>
    //public void Render(NetInfoItem netInfo)
    //{
    //    Log._Debug(@$"[{nameof(UINetInfoPanel)}.{nameof(Render)}] Received a new network ""{netInfo.Name}"".");

    //    Thumbnail.atlas = netInfo.Atlas;
    //    Thumbnail.spriteName = netInfo.Thumbnail;
    //    Label.text = netInfo.BeautifiedName;

    //    color = netInfo.Color;
    //}
}

/// <summary>
///     Tiny version of the <see cref="UINetInfoPanel" />.
/// </summary>
public class UINetInfoTinyPanel : UINetInfoPanel
{
    public UINetInfoTinyPanel()
    {
        size = UIConstants.NetInfoPanelTinySize;

        Thumbnail.size = UIConstants.ThumbnailTinySize;
        Label.size = UIConstants.NetInfoPanelTinySize - new Vector2(Thumbnail.width + (2 * UIConstants.Padding), 2 * UIConstants.Padding);
    }
}
