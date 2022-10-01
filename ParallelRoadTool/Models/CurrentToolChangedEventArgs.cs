using System;

namespace ParallelRoadTool.Models
{
    /// <summary>
    ///     <see cref="EventArgs" /> used when the current <see cref="ToolBase" /> changes in-game (e.g. user switched from
    ///     roads to transport).
    /// </summary>
    public class CurrentToolChangedEventArgs : EventArgs
    {
        #region Properties

        public ToolBase Tool { get; }

        #endregion

        public CurrentToolChangedEventArgs(ToolBase tool)
        {
            Tool = tool;
        }
    }
}
