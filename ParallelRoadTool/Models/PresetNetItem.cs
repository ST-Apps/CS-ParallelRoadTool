namespace ParallelRoadTool.Models
{
    /// <summary>
    ///     Helper class needed for serialization because NetInfo class can not be serialized
    /// </summary>
    public class PresetNetItem
    {
        public float HorizontalOffset;
        public bool IsReversed;
        public string NetName;
        public float VerticalOffset;
    }
}