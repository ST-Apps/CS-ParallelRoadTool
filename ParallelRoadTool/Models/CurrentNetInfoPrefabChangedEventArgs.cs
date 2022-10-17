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
    public CurrentNetInfoPrefabChangedEventArgs(NetInfo prefab)
    {
        Prefab = prefab;
    }

    public NetInfo Prefab { get; }
}
