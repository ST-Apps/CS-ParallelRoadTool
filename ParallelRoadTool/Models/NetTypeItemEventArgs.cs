namespace ParallelRoadTool.Models
{
    public class NetTypeItemEventArgs
    {
        public NetTypeItemEventArgs(int itemIndex, float horizontalOffset, float verticalOffset,
            int selectedNetworkIndex, bool isReversedNetwork)
        {
            ItemIndex = itemIndex;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            SelectedNetworkIndex = selectedNetworkIndex;
            IsReversedNetwork = isReversedNetwork;
        }

        public int ItemIndex { get; }
        public float HorizontalOffset { get; }
        public float VerticalOffset { get; }
        public int SelectedNetworkIndex { get; }
        public bool IsReversedNetwork { get; }
    }
}