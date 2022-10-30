// <copyright file="ModifiersManager.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Managers;

/// <summary>
///     Handles key modifiers such as CTRL, ALT or SHIFT.
/// </summary>
internal static class ModifiersManager
{
    /// <summary>
    ///     Gets or sets a value indicating whether SHIFT button is pressed or not.
    /// </summary>
    public static bool IsShiftPressed { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether CTRL button is pressed or not.
    /// </summary>
    public static bool IsCtrlPressed { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether ALT button is pressed or not.
    /// </summary>
    public static bool IsAltPressed { get; set; }
}
