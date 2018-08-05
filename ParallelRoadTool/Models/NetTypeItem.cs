namespace ParallelRoadTool.Models
{
    public class NetTypeItem
    {
        public float HorizontalOffset;
        public float VerticalOffset;
        public bool IsReversed;
        public NetInfo NetInfo;        

        public NetTypeItem(NetInfo netInfo, float horizontalOffset, float verticalOffset, bool isReversed)
        {
            NetInfo = netInfo;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            IsReversed = isReversed;
        }
    }
}