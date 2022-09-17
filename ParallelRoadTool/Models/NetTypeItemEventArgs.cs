namespace ParallelRoadTool.Models
{
    public class NetTypeItemEventArgs
    {
        public NetTypeItemEventArgs(int itemIndex,
                                    float horizontalOffset,
                                    float verticalOffset,
                                    bool isReversedNetwork)
                                    //int selectedNetworkIndex,
                                    //bool isFiltered,
                                    //string selectedNetworkName)
        {
            ItemIndex = itemIndex;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            IsReversedNetwork = isReversedNetwork;
            //SelectedNetworkIndex = selectedNetworkIndex;
            //IsFiltered = isFiltered;
            //SelectedNetworkName = selectedNetworkName;
        }

        public int ItemIndex { get; }
        public float HorizontalOffset { get; }
        public float VerticalOffset { get; }
        //public int SelectedNetworkIndex { get; }
        public bool IsReversedNetwork { get; }
        //public bool IsFiltered { get; }
        //public string SelectedNetworkName { get; }
    }
}
