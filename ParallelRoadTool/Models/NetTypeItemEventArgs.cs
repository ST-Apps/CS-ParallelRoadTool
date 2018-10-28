namespace ParallelRoadTool.Models
{
    public class NetTypeItemEventArgs
    {
        public NetTypeItemEventArgs(int itemIndex, float horizontalOffset, float verticalOffset,
            int selectedNetworkIndex, bool isReversedNetwork, bool isFiltered, string selectedNetworkName)
        {
            ItemIndex = itemIndex;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            SelectedNetworkIndex = selectedNetworkIndex;
            IsReversedNetwork = isReversedNetwork;
            IsFiltered = isFiltered;
            SelectedNetworkName = selectedNetworkName;
        }

        public int ItemIndex { get; }
        public float HorizontalOffset { get; }
        public float VerticalOffset { get; }
        public int SelectedNetworkIndex { get; }
        public bool IsReversedNetwork { get; }
        public bool IsFiltered { get; }
        public string SelectedNetworkName { get; }
    }
}