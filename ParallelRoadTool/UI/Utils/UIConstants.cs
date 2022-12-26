// <copyright file="UIConstants.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Utils;

using Shared;
using UnityEngine;

/// <summary>
///     Some shared constants to better define UI's layout.
/// </summary>
internal static class UIConstants
{
    /// <summary>
    ///     Standard padding amount for all the components in this mod.
    /// </summary>
    internal const int Padding = 8;

    /// <summary>
    ///     Size for small items (e.g. close button).
    /// </summary>
    internal const int TinySize = 28;

    /// <summary>
    ///     Size for small items (e.g. close button).
    /// </summary>
    internal const int SmallSize = 32;

    /// <summary>
    ///     Size for middle items (e.g. checkboxes).
    /// </summary>
    internal const int MediumSize = 36;

    /// <summary>
    ///     Size for large items (e.g. text-fields).
    /// </summary>
    internal const int LargeSize = 48;

    /// <summary>
    ///     Size for very large items (e.g. text-fields).
    /// </summary>
    internal const int HugeSize = 64;

    /// <summary>
    ///     Fixed width for a generic <see cref="UINetInfoPanel" />.
    /// </summary>
    internal const int NetInfoPanelLargeWidth = 380;

    /// <summary>
    ///     Fixed height for a generic <see cref="UINetInfoPanel" />.
    /// </summary>
    internal const int NetInfoPanelLargeHeight = LargeSize + Padding + Padding;

    /// <summary>
    ///     Fixed width for a generic <see cref="UINetInfoPanel" />.
    /// </summary>
    internal const int NetInfoPanelTinyWidth = 220;

    /// <summary>
    ///     Fixed height for a generic <see cref="UINetInfoPanel" />.
    /// </summary>
    internal const int NetInfoPanelTinyHeight = TinySize + Padding + Padding;

    /// <summary>
    ///     Fixed size for a tiny <see cref="NetInfo" /> thumbnail.
    /// </summary>
    internal static readonly Vector2 ThumbnailTinySize = new (TinySize, TinySize);

    /// <summary>
    ///     Fixed size for a large <see cref="NetInfo" /> thumbnail.
    /// </summary>
    internal static readonly Vector2 ThumbnailLargeSize = new (LargeSize, LargeSize);

    /// <summary>
    ///     Fixed size for a generic <see cref="UINetInfoPanel" />.
    /// </summary>
    internal static readonly Vector2 NetInfoPanelLargeSize = new (NetInfoPanelLargeWidth, NetInfoPanelLargeHeight);

    /// <summary>
    ///     Fixed size for a generic <see cref="UINetInfoPanel" />.
    /// </summary>
    internal static readonly Vector2 NetInfoPanelTinySize = new (NetInfoPanelTinyWidth, NetInfoPanelTinyHeight);

    // Taken from vanilla road building's overlay color
    internal static readonly Color ReadOnlyColor = new (0, 0.710f, 1, 0.5f);
}
