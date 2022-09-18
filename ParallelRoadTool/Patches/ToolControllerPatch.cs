using System;
using CSUtil.Commons;
using HarmonyLib;
using ParallelRoadTool.Models;

// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local

namespace ParallelRoadTool.Patches
{
    /// <summary>
    ///     Used to patch <see cref="ToolController" /> since there's no event raised when
    ///     <see cref="ToolController.CurrentTool" /> is changed.
    /// </summary>
    [HarmonyPatch(typeof(ToolController),
                  nameof(ToolController.CurrentTool),
                  MethodType.Setter
                 )]
    public class ToolControllerPatch
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

            CurrentToolChanged?.Invoke(null, new CurrentToolChangedEventArgs(ToolsModifierControl.toolController.CurrentTool));
        }
    }
}
