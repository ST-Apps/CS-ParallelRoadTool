//using ColossalFramework;
//using ColossalFramework.UI;
//using CSUtil.Commons;
//using ParallelRoadTool.Utils;
//using UnityEngine;

//namespace ParallelRoadTool.UI.Base
//{
//    internal class UIModalWindow : UIPanel
//    {

//        public static UIModalWindow Instance { get; private set; }

//        #region Events

//        public event MouseEventHandler CloseButtonEventClicked
//        {
//            add { _closeButton.eventClicked += value; }
//            remove { _closeButton.eventClicked -= value; }
//        }

//        #endregion

//        #region Unity

//        #region Components

//        private UIButton _closeButton;
//        private UIDragHandle _dragHandle;
//        private UIComponent _modalEffect;

//        #endregion

//        #region Lifecycle

//        public override void Awake()
//        {
//            // Main
//            name = $"{Configuration.ResourcePrefix}_TMP";
//            atlas = UIUtil.DefaultAtlas;
//            backgroundSprite = "SubcategoriesPanel";
//            size = new Vector2(465, 180);
//            canFocus = true;

//            // Main/DragHandle
//            _dragHandle = AddUIComponent<UIDragHandle>();
//            _dragHandle.target = parent;
//            _dragHandle.relativePosition = Vector3.zero;

//            // Main/CloseButton
//            _closeButton = AddUIComponent<UIButton>();
//            _closeButton.size = new Vector2(30f, 30f);
//            _closeButton.text = "X";
//            _closeButton.textScale = 0.9f;
//            _closeButton.textColor = new Color32(118, 123, 123, 255);
//            _closeButton.focusedTextColor = new Color32(118, 123, 123, 255);
//            _closeButton.hoveredTextColor = new Color32(140, 142, 142, 255);
//            _closeButton.pressedTextColor = new Color32(99, 102, 102, 102);
//            _closeButton.textPadding = new RectOffset(8, 8, 8, 8);
//            _closeButton.canFocus = false;
//            _closeButton.playAudioEvents = true;
//            _closeButton.relativePosition = new Vector3(width - _closeButton.width, 0);

//            // Main/ModalEffect
//            _modalEffect = GetUIView().panelsLibraryModalEffect;
//            if (_modalEffect != null && !_modalEffect.isVisible)
//            {
//                _modalEffect.Show(false);
//                ValueAnimator.Animate(
//                    "ModalEffect",
//                    delegate (float val) {
//                        _modalEffect.opacity = val; 
//                    },
//                    new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut)
//                );
//            }

//            _dragHandle.size = size;

//            UIView.GetAView().AttachUIComponent(this.gameObject);
//        }

//        //protected override void OnPositionChanged()
//        //{
//        //    var resolution = GetUIView().GetScreenResolution();

//        //    if (absolutePosition.x == -1000)
//        //    {
//        //        absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);
//        //        MakePixelPerfect();
//        //    }

//        //    absolutePosition = new Vector2(
//        //                                   (int)Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
//        //                                   (int)Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

//        //    //LoadWindowX.value = (int)absolutePosition.x;
//        //    //LoadWindowY.value = (int)absolutePosition.y;

//        //    Log._Debug($"POPUP {absolutePosition}");

//        //    base.OnPositionChanged();
//        //}

//        #endregion

//        #endregion

//        #region Control

//        public void Open()
//        {
//            var resolution = GetUIView().GetScreenResolution();

//            Log._Debug($"RESOLUTION {resolution}");
//            Log._Debug($"NONSENSE: {new Vector2((resolution.x / 2f) - width, (resolution.y / 2f) - height)}");

//            //absolutePosition = new Vector2((resolution.x / 2f) - width, (resolution.y / 2f) - height);
//            absolutePosition = new Vector2(330, 300);
//            // absolutePosition = new Vector3(-1000, -1000); // LoadWindowX.value, LoadWindowY.value);
//            MakePixelPerfect();
//            // OnPositionChanged();

//            Log._Debug($"POPUP {absolutePosition}");

//            UIView.PushModal(this);

//            Show(true);
//            Focus();
//        }

//        public void Close()
//        {
//            UIView.PopModal();

//            var modalEffect = GetUIView().panelsLibraryModalEffect;
//            if (modalEffect != null && modalEffect.isVisible) modalEffect.Hide();
//            isVisible = false;            
//        }

//        #endregion
//    }
//}
