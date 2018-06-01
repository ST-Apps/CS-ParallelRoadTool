namespace ParallelRoadTool
{
    public class NetTypeItem
    {
        public float HorizontalOffset;
        public bool IsReversed;
        public NetInfo NetInfo;
        public float VerticalOffset;

        public NetTypeItem(NetInfo netInfo, float horizontalOffset, float verticalOffset, bool isReversed)
        {
            NetInfo = netInfo;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            IsReversed = isReversed;
        }
    }
}