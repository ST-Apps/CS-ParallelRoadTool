namespace ParallelRoadTool.Models
{
    public class NetTypeItemEventArgs
    {
        public NetTypeItemEventArgs(int itemIndex, float horizontalOffset, float verticalOffset, bool isReversedNetwork)
        {
            ItemIndex = itemIndex;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            IsReversedNetwork = isReversedNetwork;
        }

        public NetTypeItemEventArgs(int itemIndex, string selectedNetworkName)
        {
            ItemIndex = itemIndex;
            SelectedNetworkName = selectedNetworkName;
        }

        public int ItemIndex { get; set; }
        public float HorizontalOffset { get; }
        public float VerticalOffset { get; }
        public bool IsReversedNetwork { get; }
        public string SelectedNetworkName { get; }
    }
}
