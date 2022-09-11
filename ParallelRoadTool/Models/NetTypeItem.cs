using ColossalFramework.UI;
using ParallelRoadTool.UI.Utils;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.Models
{
    public class NetInfoItem
    {

        #region Properties

        public NetInfo NetInfo { get; private set; }

        public string Name => NetInfo.name;

        public string BeautifiedName { get; private set; }

        public Color Color { get; private set; }

        public UITextureAtlas Atlas => NetInfo.m_Atlas;

        public string Thumbnail => NetInfo.m_Thumbnail;

        public float HorizontalOffset { get; set; }

        public float VerticalOffset { get; set; }

        public bool IsReversed { get; set; }

        #endregion

        public NetInfoItem(NetInfo netInfo)
        {
            NetInfo = netInfo;

            BeautifiedName = netInfo.GenerateBeautifiedNetName();
            Color = UIHelpers.ColorFromString(BeautifiedName);
        }

        public NetInfoItem(NetInfo netInfo, float horizontalOffset, float verticalOffset, bool isReversed) : this(netInfo)
        {
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            IsReversed = isReversed;
        }
    }
}
