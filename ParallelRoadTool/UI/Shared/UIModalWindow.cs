using AlgernonCommons.UI;
using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Shared
{
    public abstract class UIModalWindow : UIWindow
    {
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
        }

        protected override bool PreClose()
        {
            UIView.PopModal();

            var modalEffect = GetUIView().panelsLibraryModalEffect;
            if (modalEffect != null && modalEffect.isVisible)
                ValueAnimator.Animate("ModalEffect", delegate(float val) { modalEffect.opacity = val; },
                                      new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut), delegate { modalEffect.Hide(); });

            return base.PreClose();
        }

        #endregion

        #endregion

        #region Properties

        public override float PanelWidth => 450;

        public override float PanelHeight => 180;

        #endregion
    }
}
