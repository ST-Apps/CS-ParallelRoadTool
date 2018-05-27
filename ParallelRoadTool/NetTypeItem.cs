using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelRoadTool
{
    public class NetTypeItem
    {
        public NetInfo NetInfo;
        public float HorizontalOffset;
        public float VerticalOffset;
        public bool IsReversed;
        
        public NetTypeItem(NetInfo netInfo, float horizontalOffset, float verticalOffset, bool isReversed)
        {
            NetInfo = netInfo;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            IsReversed = isReversed;
        }
    }
}
