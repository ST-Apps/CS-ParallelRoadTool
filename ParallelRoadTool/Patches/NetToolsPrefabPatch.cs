using System;
using ColossalFramework;
using CSUtil.Commons;
using HarmonyLib;
using ParallelRoadTool.Models;

// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Local

namespace ParallelRoadTool.Patches
{
    [HarmonyPatch(typeof(NetTool),
                  nameof(NetTool.Prefab),
                  MethodType.Setter
                 )]
    internal class NetToolsPrefabPatch
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
}
