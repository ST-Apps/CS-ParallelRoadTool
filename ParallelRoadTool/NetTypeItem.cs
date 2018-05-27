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

        // TODO: remove default verticalOffset once implemented
        public NetTypeItem(NetInfo netInfo, float horizontalOffset, float verticalOffset = 0)
        {
            NetInfo = netInfo;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
        }
    }
}
