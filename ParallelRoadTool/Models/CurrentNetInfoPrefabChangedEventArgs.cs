// <copyright file="CurrentNetInfoPrefabChangedEventArgs.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Models;

using System;

/// <summary>
///     <see cref="EventArgs" /> be used when the currently selected <see cref="NetInfo" /> changes in-game (e.g. user
///     clicked on a different network type in game's UI).
/// </summary>
internal class CurrentNetInfoPrefabChangedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CurrentNetInfoPrefabChangedEventArgs"/> class.
    /// </summary>
    /// <param name="prefab"><see cref="NetInfo"/> item that is passed as event argument.</param>
    public CurrentNetInfoPrefabChangedEventArgs(NetInfo prefab)
    {
        Prefab = prefab;
    }

    /// <summary>
    ///     Gets the <see cref="NetInfo"/> item that has been passed as event argument.
    /// </summary>
    public NetInfo Prefab { get; }
}
