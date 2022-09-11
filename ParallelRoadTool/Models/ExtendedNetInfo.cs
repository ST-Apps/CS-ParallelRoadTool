using ColossalFramework.UI;
using ParallelRoadTool.UI.Utils;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.Models
{
    public class ExtendedNetInfo
    {
        #region Properties

        public NetInfo NetInfo { get; private set; }

        public string Name => NetInfo.name;

        public string BeautifiedName { get; private set; }

        public Color Color { get; private set; }

        public UITextureAtlas Atlas => NetInfo.m_Atlas;

        public string Thumbnail => NetInfo.m_Thumbnail;

        #endregion

        public ExtendedNetInfo(NetInfo netInfo)
        {
            NetInfo = netInfo;
            BeautifiedName = netInfo.GenerateBeautifiedNetName();
            Color = UIHelpers.ColorFromString(BeautifiedName);
        }
    }
}
