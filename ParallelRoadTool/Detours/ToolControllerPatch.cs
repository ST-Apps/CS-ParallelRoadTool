using CSUtil.Commons;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ParallelRoadTool.Detours
{
    [HarmonyPatch(
        typeof(ToolController),
        nameof(ToolController.CurrentTool),
        MethodType.Setter
    )]
    internal class ToolControllerPatch
    {

        private static void Postfix()
        {
            Log._Debug($"[{nameof(ToolControllerPatch)}.{nameof(Postfix)}] Changed active tool to {ToolsModifierControl.toolController.CurrentTool}.");
        }

    }
}
