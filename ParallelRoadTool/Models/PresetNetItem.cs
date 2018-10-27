using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelRoadTool.Models
{
    /// <summary>
    /// Helper class needed for serialization because NetInfo class can not be serialized
    /// </summary>    
    public class PresetNetItem
    {
        public float HorizontalOffset;
        public bool IsReversed;
        public string NetName;
        public float VerticalOffset;

        public PresetNetItem(string netName, float horizontalOffset, float verticalOffset, bool isReversed)
        {
            NetName = netName;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            IsReversed = isReversed;
        }

        public PresetNetItem() { }
        public void Add(NetTypeItem value) { }
    }
}
