using ParallelRoadTool.UI.Main;
using ParallelRoadTool.UI.Shared;
using UnityEngine;

namespace ParallelRoadTool.UI.Utils
{
    /// <summary>
    /// Some shared constants to better define UI's layout.
    /// </summary>
    internal static class UIConstants
    {
        /// <summary>
        /// Standard padding amount for all the components in this mod.
        /// </summary>
        internal const int Padding = 8;

        /// <summary>
        /// Size for small items (e.g. close button).
        /// </summary>
        internal const int TinySize = 28;

        /// <summary>
        /// Size for small items (e.g. close button).
        /// </summary>
        internal const int SmallSize = 32;

        /// <summary>
        /// Size for middle items (e.g. checkboxes).
        /// </summary>
        internal const int MediumSize = 36;

        /// <summary>
        /// Size for large items (e.g. textfields).
        /// </summary>
        internal const int LargeSize = 48;

        /// <summary>
        /// Fixed size for a tiny <see cref="NetInfo"/> thumbnail
        /// </summary>
        internal static readonly Vector2 ThumbnailTinySize = new(TinySize, TinySize);

        /// <summary>
        /// Fixed size for a large <see cref="NetInfo"/> thumbnail
        /// </summary>
        internal static readonly Vector2 ThumbnailLargeSize = new(LargeSize, LargeSize);

        /// <summary>
        /// Fixed width for a generic <see cref="UINetInfoPanel"/>
        /// </summary>
        internal const int NetInfoPanelLargeWidth = 400;

        /// <summary>
        /// Fixed height for a generic <see cref="UINetInfoPanel"/>
        /// </summary>
        internal const int NetInfoPanelLargeHeight = LargeSize + Padding + Padding;

        /// <summary>
        /// Fixed size for a generic <see cref="UINetInfoPanel"/>
        /// </summary>
        internal static readonly Vector2 NetInfoPanelLargeSize = new(NetInfoPanelLargeWidth, NetInfoPanelLargeHeight);

        /// <summary>
        /// Fixed width for a generic <see cref="UINetInfoPanel"/>
        /// </summary>
        internal const int NetInfoPanelTinyWidth = 220;

        /// <summary>
        /// Fixed height for a generic <see cref="UINetInfoPanel"/>
        /// </summary>
        internal const int NetInfoPanelTinyHeight = TinySize + Padding + Padding;

        /// <summary>
        /// Fixed size for a generic <see cref="UINetInfoPanel"/>
        /// </summary>
        internal static readonly Vector2 NetInfoPanelTinySize = new(NetInfoPanelTinyWidth, NetInfoPanelTinyHeight);

        // Taken from vanilla road building's overlay color
        internal static readonly Color ReadOnlyColor = new(0, 0.710f, 1, 0.5f);
    }
}
