using ColossalFramework.UI;

namespace ParallelRoadTool.UI.Base
{
    /// <summary>
    ///     Utility class that allows right-click to drag, disabling left-click one.
    /// </summary>

    // ReSharper disable once ClassNeverInstantiated.Global
    public class UIRightDragHandle : UIDragHandle
    {
        public UIRightDragHandle()
        {
            eventDragStart += OnEventDragStart;
            eventDragEnd += OnEventDragEnd;
        }

        // Original events are not working outside of this class, so we need to redefine them
        public event DragEventHandler EventDragStart;
        public event DragEventHandler EventDragEnd;

        private void OnEventDragEnd(UIComponent component, UIDragEventParameter eventParam)
        {
            EventDragEnd?.Invoke(component, eventParam);
        }

        private void OnEventDragStart(UIComponent component, UIDragEventParameter eventParam)
        {
            EventDragStart?.Invoke(component, eventParam);
        }

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
    }
}
