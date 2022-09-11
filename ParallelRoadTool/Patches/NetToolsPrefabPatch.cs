using ColossalFramework;
using CSUtil.Commons;
using HarmonyLib;
using ParallelRoadTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ParallelRoadTool.Patches
{
    [HarmonyPatch(
        typeof(NetTool),
        nameof(NetTool.Prefab),
        MethodType.Setter
    )]
    internal class NetToolsPrefabPatch
    {
        /// <summary>
        /// Event raised to report changes on <see cref="NetTool.Prefab"/>.
        /// </summary>
        public static event EventHandler<CurrentNetInfoPrefabChangedEventArgs> CurrentNetInfoChanged;

        /// <summary>
        /// Just retrieve <see cref="NetTool.Prefab"/> and raise <see cref="CurrentNetInfoChanged"/> event.
        /// </summary>
        private static void Postfix()
        {
            var prefab = Singleton<NetTool>.instance.Prefab;
            Log._Debug($"[{nameof(NetToolsPrefabPatch)}.{nameof(Postfix)}] Changed active tool to {prefab.name}.");

            CurrentNetInfoChanged?.Invoke(null, new CurrentNetInfoPrefabChangedEventArgs(prefab));
        }
    }
}
