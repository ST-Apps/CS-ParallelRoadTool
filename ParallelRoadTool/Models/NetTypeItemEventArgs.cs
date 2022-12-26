// <copyright file="NetTypeItemEventArgs.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Models;

using System;

/// <summary>
///     <see cref="EventArgs" /> used when the setup for a specific parallel/stacked network changes.
///     This can contain item's index and network name if the network type changed, index and the other custom properties
///     otherwise (e.g. an offset changed).
/// </summary>
public class NetTypeItemEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NetTypeItemEventArgs"/> class.
    /// </summary>
    /// <param name="itemIndex">Index of the object that triggered the event.</param>
    /// <param name="horizontalOffset">Value for the new horizontal offset.</param>
    /// <param name="verticalOffset">Value for the new vertical offset.</param>
    /// <param name="isReversed">Value for the new reverse property.</param>
    public NetTypeItemEventArgs(int itemIndex, float horizontalOffset, float verticalOffset, bool isReversed)
    {
        ItemIndex = itemIndex;
        HorizontalOffset = horizontalOffset;
        VerticalOffset = verticalOffset;
        IsReversed = isReversed;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NetTypeItemEventArgs"/> class.
    /// </summary>
    /// <param name="itemIndex">Index of the object that triggered the event.</param>
    /// <param name="selectedNetworkName">Name of the <see cref="NetInfoItem"/> object that was selected.</param>
    public NetTypeItemEventArgs(int itemIndex, string selectedNetworkName)
    {
        ItemIndex = itemIndex;
        SelectedNetworkName = selectedNetworkName;
    }

    /// <summary>
    ///     Gets the horizontal offset relative to the main network.
    /// </summary>
    public float HorizontalOffset { get; }

    /// <summary>
    ///     Gets the vertical offset relative to the main network.
    /// </summary>
    public float VerticalOffset { get; }

    /// <summary>
    ///     Gets a value indicating whether the direction is reversed.
    /// </summary>
    public bool IsReversed { get; }

    /// <summary>
    ///     Gets the name of the newly selected network.
    ///     This is in-game's basic name, not the display one.
    /// </summary>
    public string SelectedNetworkName { get; }

    /// <summary>
    ///     Gets or sets the current index in its container.
    ///     This is needed to detect the correct event source on property changes/item deletion.
    /// </summary>
    public int ItemIndex { get; set; }
}
