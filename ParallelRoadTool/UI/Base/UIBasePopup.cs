using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Base
{
    public abstract class UIBasePopup : UIPanel
    {
        #region Unity

        public override void OnDestroy()
        {
            base.OnDestroy();
            UnsubscribeFromUiEvents();
        }

        public override void Start()
        {
            name = $"{Configuration.ResourcePrefix}{WindowName()}";
            atlas = UIUtil.DefaultAtlas;
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(465, 180);
            canFocus = true;

            _dragHandle = AddUIComponent<UIDragHandle>();
            _dragHandle.target = parent;
            _dragHandle.relativePosition = Vector3.zero;

            _closeButton = AddUIComponent<UIButton>();
            _closeButton.size = new Vector2(30f, 30f);
            _closeButton.text = "X";
            _closeButton.textScale = 0.9f;
            _closeButton.textColor = new Color32(118, 123, 123, 255);
            _closeButton.focusedTextColor = new Color32(118, 123, 123, 255);
            _closeButton.hoveredTextColor = new Color32(140, 142, 142, 255);
            _closeButton.pressedTextColor = new Color32(99, 102, 102, 102);
            _closeButton.textPadding = new RectOffset(8, 8, 8, 8);
            _closeButton.canFocus = false;
            _closeButton.playAudioEvents = true;
            _closeButton.relativePosition = new Vector3(width - _closeButton.width, 0);

            _titleLabel = AddUIComponent<UILabel>();
            _titleLabel.textScale = 0.9f;
            _titleLabel.text = WindowTitle();
            _titleLabel.relativePosition = new Vector2(8, 8);
            _titleLabel.SendToBack();

            InitComponent();

            _dragHandle.size = size;
            absolutePosition = new Vector3(WindowX.value, WindowY.value);
            MakePixelPerfect();

            _modalEffect = GetUIView().panelsLibraryModalEffect;
            if (_modalEffect != null && !_modalEffect.isVisible)
            {
                _modalEffect.Show(false);
                ValueAnimator.Animate("ModalEffect", delegate(float val) { _modalEffect.opacity = val; },
                    new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));
            }

            SubscribeToUiEvents();

            BringToFront();
            Focus();
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                p.Use();
                Close();
            }

            base.OnKeyDown(p);
        }

        protected override void OnPositionChanged()
        {
            var resolution = GetUIView().GetScreenResolution();

            if (absolutePosition.x == -1000)
            {
                absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);
                MakePixelPerfect();
            }

            absolutePosition = new Vector2(
                (int) Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                (int) Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            WindowX.value = (int) absolutePosition.x;
            WindowY.value = (int) absolutePosition.y;

            base.OnPositionChanged();
        }

        #endregion

        #region Properties        

        private static UIBasePopup Instance;

        /// <summary>
        ///     Stored X position for current window
        /// </summary>
        private static SavedInt WindowX;

        /// <summary>
        ///     Stored Y position for current window
        /// </summary>
        private static SavedInt WindowY;

        /// <summary>
        ///     Button used to close the window
        /// </summary>
        private UIButton _closeButton;

        /// <summary>
        ///     Button used to drag the window
        /// </summary>
        private UIDragHandle _dragHandle;

        /// <summary>
        ///     Window's title
        /// </summary>
        private UILabel _titleLabel;

        private UIComponent _modalEffect;

        /// <summary>
        /// Object that asked for the popup
        /// </summary>
        protected object _caller;

        #endregion

        #region Events

        protected virtual void SubscribeToUiEvents()
        {
            _closeButton.eventClick += CloseButtonOnEventClick;
        }

        protected virtual void UnsubscribeFromUiEvents()
        {
            _closeButton.eventClick -= CloseButtonOnEventClick;
        }

        private void CloseButtonOnEventClick(UIComponent component, UIMouseEventParameter eventparam)
        {
            // TODO: this event is not working!
            Close();
        }

        #endregion

        #region Handlers

        protected abstract string WindowTitle();
        protected abstract string WindowName();

        public void Open(object caller = null)
        {
            if (Instance != null) return;
            Instance = UIView.GetAView().AddUIComponent(GetType()) as UIBasePopup;
            UIView.PushModal(Instance);
            _caller = caller;
        }

        public void Close()
        {
            if (Instance == null) return;

            UIView.PopModal();
            var modalEffect = Instance.GetUIView().panelsLibraryModalEffect;
            if (modalEffect != null && modalEffect.isVisible) modalEffect.Hide();

            _caller = null;
            isVisible = false;
            Destroy(Instance.gameObject);
            Instance = null;
        }

        /// <summary>
        ///     Method used to setup additional components that must be shown inside the window
        /// </summary>
        protected abstract void InitComponent();

        #endregion
    }
}