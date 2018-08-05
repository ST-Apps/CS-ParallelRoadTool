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

    //helper class needed for serialization because NetInfo class can not be serialized
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