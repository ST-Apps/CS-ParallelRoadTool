// <copyright file="NetToolsPrefabPatch.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Patches;

using System;
using ColossalFramework;
using CSUtil.Commons;
using HarmonyLib;
using Models;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Global
// ReSharper disable InconsistentNaming
[HarmonyPatch(typeof(NetTool), nameof(NetTool.Prefab), MethodType.Setter)]
internal static class NetToolsPrefabPatch
{
    /// <summary>
    ///     Event raised to report changes on <see cref="NetTool.Prefab" />.
    /// </summary>
    public static event EventHandler<CurrentNetInfoPrefabChangedEventArgs> CurrentNetInfoChanged;

    /// <summary>
    ///     Just retrieve <see cref="NetTool.Prefab" /> and raise <see cref="CurrentNetInfoChanged" /> event.
    /// </summary>
    private static void Postfix()
    {
        var prefab = Singleton<NetTool>.instance.Prefab;
        Log._Debug($"[{nameof(NetToolsPrefabPatch)}.{nameof(Postfix)}] Changed active tool to {prefab.name}.");

        CurrentNetInfoChanged?.Invoke(null, new CurrentNetInfoPrefabChangedEventArgs(prefab));
    }
}
