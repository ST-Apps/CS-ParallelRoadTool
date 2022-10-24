// <copyright file="CurrentToolChangedEventArgs.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Models;

using System;

/// <summary>
///     <see cref="EventArgs" /> used when the current <see cref="ToolBase" /> changes in-game (e.g. user switched from
///     roads to transport).
/// </summary>
public class CurrentToolChangedEventArgs : EventArgs
{
    public CurrentToolChangedEventArgs(ToolBase tool)
    {
        Tool = tool;
    }

    public ToolBase Tool { get; }
}
