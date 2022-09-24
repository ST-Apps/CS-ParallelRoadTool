using ColossalFramework;
using ColossalFramework.UI;

namespace ParallelRoadTool.UI.Shared
{
    /// <summary>
    ///     Base class for a specific modal window, containing the modal animation and the basic logic to push/pop a modal
    ///     window.
    /// </summary>
    public abstract class UIModalWindow : UIWindow
    {
        #region Properties

        public override float PanelWidth => 450;

        public override float PanelHeight => 180;

        #endregion

        #region Unity

        #region Lifecycle

        public UIModalWindow(string iconAtlasName) : base(iconAtlasName)
        {
            // Main/ModalEffect
            var modalEffect = GetUIView().panelsLibraryModalEffect;
            if (modalEffect != null && !modalEffect.isVisible)
            {
                modalEffect.Show(false);
                ValueAnimator.Animate("ModalEffect",
                                      delegate(float val) { modalEffect.opacity = val; },
                                      new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut)
                                     );
            }

            UIView.PushModal(this);
            Focus();
        }

        protected override bool PreClose()
        {
            UIView.PopModal();

            var modalEffect = GetUIView().panelsLibraryModalEffect;
            if (modalEffect != null && modalEffect.isVisible)
                ValueAnimator.Animate("ModalEffect", delegate(float val) { modalEffect.opacity = val; },
                                      new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), delegate { modalEffect.Hide(); });
            Unfocus();
            return base.PreClose();
        }

        #endregion

        #endregion
    }
}
