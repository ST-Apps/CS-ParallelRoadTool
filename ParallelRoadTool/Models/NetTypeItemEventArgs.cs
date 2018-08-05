using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParallelRoadTool.Models
{
    public class NetTypeItemEventArgs
    {

        public float HorizontalOffset { get; }
        public float VerticalOffset { get; }
        public int SelectedNetworkIndex { get; }
        public bool IsReversedNetwork { get; }

        public NetTypeItemEventArgs(float horizontalOffset, float verticalOffset, int selectedNetworkIndex, bool isReversedNetwork)
        {
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            SelectedNetworkIndex = selectedNetworkIndex;
            IsReversedNetwork = isReversedNetwork;
        }
    }
}
