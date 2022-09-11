using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelRoadTool.Models
{
    internal class CurrentNetInfoPrefabChangedEventArgs : EventArgs
    {
        public NetInfo Prefab { get; private set; }

        public CurrentNetInfoPrefabChangedEventArgs(NetInfo prefab)
        {
            Prefab = prefab;
        }
    }
}
