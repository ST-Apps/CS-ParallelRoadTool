// <copyright file="NetInfoItem.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Models;

using AlgernonCommons.Utils;
using ColossalFramework.UI;
using UI.Utils;
using UnityEngine;

/// <summary>
///     Main model for the mod.
///     It contains the original <see cref="NetInfo" /> item alongside all of its customizable properties and some
///     utilities such as its color and a readable name.
/// </summary>
public class NetInfoItem
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NetInfoItem"/> class.
    ///     Special case in which we don't have any customizable property, used to render the currently selected network.
    /// </summary>
    /// <param name="netInfo"><see cref="NetInfo"/> item that is being wrapped.</param>
    public NetInfoItem(NetInfo netInfo)
    {
        NetInfo = netInfo;

        BeautifiedName = PrefabUtils.GetDisplayName(netInfo);
        Color = UIHelpers.ColorFromString(Name);
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NetInfoItem"/> class.
    ///     Sets all the customizable properties alongside the wrapped object's ones.
    /// </summary>
    /// <param name="netInfo"></param>
    /// <param name="horizontalOffset"></param>
    /// <param name="verticalOffset"></param>
    /// <param name="isReversed"></param>
    public NetInfoItem(NetInfo netInfo, float horizontalOffset, float verticalOffset, bool isReversed)
        : this(netInfo)
    {
        HorizontalOffset = horizontalOffset;
        VerticalOffset = verticalOffset;
        IsReversed = isReversed;
    }

    /// <summary>
    ///     Gets the wrapped object with in-game properties.
    /// </summary>
    public NetInfo NetInfo { get; }

    /// <summary>
    ///     Gets network's name, used also as a unique id.
    /// </summary>
    public string Name => NetInfo?.name ?? "null";

    /// <summary>
    ///     Gets a generated name used for display purposes.
    ///     This might be translated or changed in future so it can't be used to identify the network.
    /// </summary>
    public string BeautifiedName { get; }

    /// <summary>
    ///     Gets a <see cref="Color"/> mapped from network's name.
    /// </summary>
    public Color Color { get; }

    /// <summary>
    ///     Gets the <see cref="UITextureAtlas"/> containing network's thumbnail.
    /// </summary>
    public UITextureAtlas Atlas => NetInfo.m_Atlas;

    /// <summary>
    ///     Gets network's thumbnail name in the provided <see cref="Atlas" />.
    /// </summary>
    public string Thumbnail => NetInfo.m_Thumbnail;

    /// <summary>
    ///     Gets or sets the horizontal offset for current <see cref="NetInfo"/> ite.
    ///     Horizontal offset measures the horizontal distance between current network and the previous one.
    /// </summary>
    public float HorizontalOffset { get; set; }

    /// <summary>
    ///     Gets or sets the vertical offset for current <see cref="NetInfo"/> ite.
    ///     Vertical offset measures the vertical distance between current network and the previous one.
    /// </summary>
    public float VerticalOffset { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the current network must be created going
    ///     in the opposite direction as the one selected in <see cref="NetTool" />.
    /// </summary>
    public bool IsReversed { get; set; }
}
