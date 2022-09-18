using ColossalFramework.UI;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.Models
{
    /// <summary>
    ///     Main model for the mod.
    ///     It contains the original <see cref="NetInfo" /> item alongside all of its customizable properties and some
    ///     utilities such as its color and a readable name.
    /// </summary>
    public class NetInfoItem
    {
        /// <summary>
        ///     Special case in which we don't have any customizable property, used to render the currently selected network
        /// </summary>
        /// <param name="netInfo"></param>
        public NetInfoItem(NetInfo netInfo)
        {
            NetInfo = netInfo;

            BeautifiedName = netInfo.GenerateBeautifiedNetName();
            Color = UIHelpers.ColorFromString(Name);
        }

        /// <summary>
        ///     Set all the customizable properties alongside the wrapped object's ones
        /// </summary>
        /// <param name="netInfo"></param>
        /// <param name="horizontalOffset"></param>
        /// <param name="verticalOffset"></param>
        /// <param name="isReversed"></param>
        public NetInfoItem(NetInfo netInfo, float horizontalOffset, float verticalOffset, bool isReversed) : this(netInfo)
        {
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            IsReversed = isReversed;
        }

        #region Properties

        /// <summary>
        ///     Wrapped object with in-game properties
        /// </summary>
        public NetInfo NetInfo { get; }

        /// <summary>
        ///     Network's name, used also as a unique id
        /// </summary>
        public string Name => NetInfo.name;

        /// <summary>
        ///     Name used for display purposes.
        ///     This might be translated or changed in future so it can't be used to identify the network
        /// </summary>
        public string BeautifiedName { get; }

        /// <summary>
        ///     Color mapped from network's name
        /// </summary>
        public Color Color { get; }

        /// <summary>
        ///     Atlas containing network's thumbnail
        /// </summary>
        public UITextureAtlas Atlas => NetInfo.m_Atlas;

        /// <summary>
        ///     Network's thumbnail name in the provided <see cref="Atlas" />
        /// </summary>
        public string Thumbnail => NetInfo.m_Thumbnail;

        /// <summary>
        ///     Horizontal offset measures the horizontal distance between current network and the previous one
        /// </summary>
        public float HorizontalOffset { get; set; }

        /// <summary>
        ///     Vertical offset measures the vertical distance between current network and the previous one
        /// </summary>
        public float VerticalOffset { get; set; }

        /// <summary>
        ///     If true, the current network must be created going in the opposite direction as the one selected in
        ///     <see cref="NetTool" />
        /// </summary>
        public bool IsReversed { get; set; }

        #endregion
    }
}
