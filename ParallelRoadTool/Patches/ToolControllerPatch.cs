// <copyright file="ToolControllerPatch.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Patches;

using System;
using CSUtil.Commons;
using HarmonyLib;
using Models;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming
/// <summary>
///     Used to patch <see cref="ToolController" /> since there's no event raised when
///     <see cref="ToolController.CurrentTool" /> is changed.
/// </summary>
[HarmonyPatch(typeof(ToolController), nameof(ToolController.CurrentTool), MethodType.Setter)]
internal static class ToolControllerPatch
{
    /// <summary>
    ///     Event raised to report changes on <see cref="ToolController.CurrentTool" />.
    /// </summary>
    public static event EventHandler<CurrentToolChangedEventArgs> CurrentToolChanged;

    /// <summary>
    ///     Just retrieve <see cref="ToolController.CurrentTool" /> and raise <see cref="CurrentToolChanged" /> event.
    /// </summary>
    private static void Postfix()
    {
        Log._Debug($"[{nameof(ToolControllerPatch)}.{nameof(Postfix)}] Changed active tool to {ToolsModifierControl.toolController.CurrentTool}.");

        CurrentToolChanged?.Invoke(null, new CurrentToolChangedEventArgs(ToolsModifierControl.toolController?.CurrentTool));
    }
}
