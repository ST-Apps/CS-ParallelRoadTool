// <copyright file="ModStatuses.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Models;

using System;

/// <summary>
///     Possible statuses for the Mod.
///     <list type="bullet">
///         <item>Disabled - Mod is fully disabled, patches are not deployed and no UI items exist.</item>
///         <item>Deployed - Mod is enabled, patches are deployed and UI items exist.</item>
///         <item>Enabled - Mod can be used (e.g. we're in the right tool).</item>
///         <item>Active - Mod's is being executed (e.g. parallel segments are on).</item>
///     </list>
/// </summary>
[Flags]
public enum ModStatuses : byte
{
    Disabled = 1,
    Deployed = 2,
    Enabled = 4,
    Active = 8
}
