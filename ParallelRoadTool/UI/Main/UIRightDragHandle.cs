using ColossalFramework.UI;

// ReSharper disable once ClassNeverInstantiated.Global

namespace ParallelRoadTool.UI.Main
{
    /// <summary>
    ///     Utility class that allows right-click to drag, disabling left-click one.
    /// </summary>
    public class UIRightDragHandle : UIDragHandle
    {
        #region Events

        public event DragEventHandler EventDragStart
        {
            add => eventDragStart += value;
            remove => eventDragStart -= value;
        }

        public event DragEventHandler EventDragEnd
        {
            add => eventDragEnd += value;
            remove => eventDragEnd -= value;
        }

        #endregion

        #region Callbacks

        protected override void OnMouseDown(UIMouseEventParameter p)
        {
            if (p.buttons != UIMouseButton.Right) return;

            p = new UIMouseEventParameter(p.source, UIMouseButton.Left, p.clicks, p.ray, p.position, p.moveDelta,
                                          p.wheelDelta);

            base.OnMouseDown(p);
        }

        protected override void OnMouseMove(UIMouseEventParameter p)
        {
            if (p.buttons != UIMouseButton.Right) return;

            p = new UIMouseEventParameter(p.source, UIMouseButton.Left, p.clicks, p.ray, p.position, p.moveDelta,
                                          p.wheelDelta);

            base.OnMouseMove(p);
        }

        #endregion
    }
}
