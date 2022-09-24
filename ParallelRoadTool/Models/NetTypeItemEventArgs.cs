using System;

namespace ParallelRoadTool.Models
{
    /// <summary>
    ///     <see cref="EventArgs" /> used when the setup for a specific parallel/stacked network changes.
    ///     This can contain item's index and network name if the network type changed, index and the other custom properties
    ///     otherwise (e.g. an offset changed).
    /// </summary>
    public class NetTypeItemEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        ///     Current index in its container
        /// </summary>
        public int ItemIndex { get; set; }

        /// <summary>
        ///     Horizontal offset, relative to the main network
        /// </summary>
        public float HorizontalOffset { get; }

        /// <summary>
        ///     Vertical offset, relative to the main network
        /// </summary>
        public float VerticalOffset { get; }

        /// <summary>
        ///     True if the direction is reversed
        /// </summary>
        public bool IsReversed { get; }

        /// <summary>
        ///     Name of the newly selected network. This is in-game's basic name, not the display one.
        /// </summary>
        public string SelectedNetworkName { get; }

        #endregion

        public NetTypeItemEventArgs(int itemIndex, float horizontalOffset, float verticalOffset, bool isReversed)
        {
            ItemIndex = itemIndex;
            HorizontalOffset = horizontalOffset;
            VerticalOffset = verticalOffset;
            IsReversed = isReversed;
        }

        public NetTypeItemEventArgs(int itemIndex, string selectedNetworkName)
        {
            ItemIndex = itemIndex;
            SelectedNetworkName = selectedNetworkName;
        }
    }
}
