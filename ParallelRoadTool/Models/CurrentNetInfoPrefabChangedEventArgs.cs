using System;

namespace ParallelRoadTool.Models
{
    /// <summary>
    ///     <see cref="EventArgs" /> be used when the currently selected <see cref="NetInfo" /> changes in-game (e.g. user
    ///     clicked on a different network type in game's UI).
    /// </summary>
    internal class CurrentNetInfoPrefabChangedEventArgs : EventArgs
    {
        #region Properties

        public NetInfo Prefab { get; }

        #endregion

        public CurrentNetInfoPrefabChangedEventArgs(NetInfo prefab)
        {
            Prefab = prefab;
        }
    }
}
