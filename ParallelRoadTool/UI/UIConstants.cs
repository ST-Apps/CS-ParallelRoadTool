using UnityEngine;

namespace ParallelRoadTool.UI
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
        internal const int SmallSize = 32;

        /// <summary>
        /// Size for middle items (e.g. checkboxes).
        /// </summary>
        internal const int MiddleSize = 36;

        /// <summary>
        /// Size for large items (e.g. checkboxes).
        /// </summary>
        internal const int LargeSize = 48;

        /// <summary>
        /// Fixed size for a <see cref="NetInfo"/> thumbnail
        /// </summary>
        internal static readonly Vector2 ThumbnailSize = new Vector2(LargeSize, LargeSize);

        /// <summary>
        /// Fixed width for a generic <see cref="UINetInfoPanel"/>
        /// </summary>
        internal const int NetInfoPanelWidth = 400;

        /// <summary>
        /// Fixed height for a generic <see cref="UINetInfoPanel"/>
        /// </summary>
        internal const int NetInfoPanelHeight = LargeSize + Padding + Padding;

        /// <summary>
        /// Fixed size for a generic <see cref="UINetInfoPanel"/>
        /// </summary>
        internal static readonly Vector2 NetInfoPanelSize = new Vector2(NetInfoPanelWidth, NetInfoPanelHeight);

        // Taken from vanilla road building's overlay color
        internal static readonly Color ReadOnlyColor = new Color(0, 0.710f, 1, 0.5f);
    }
}
