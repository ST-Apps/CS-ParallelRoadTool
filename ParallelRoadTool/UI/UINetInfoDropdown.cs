using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Utils;
using System.Collections.Generic;
using TMP.UI;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UINetInfoDropdown : UIPanel
    {

        //private UINetInfoPanel _netInfoPanel;
        //private UIButton _dropdownToggleButton;
        private UINetInfoButton _netInfoButton;
        private UINetListPopup _popup;

        public bool IsReadOnly { set { _netInfoButton.IsReadOnly = value; } }

        public override void Awake()
        {
            clipChildren = true;
            size = new Vector2(400, 48 + UIConstants.Padding + UIConstants.Padding);

            _netInfoButton = AddUIComponent<UINetInfoButton>();
            _netInfoButton.FitWidth(this, 0);

            //_netInfoPanel = AddUIComponent<UINetInfoPanel>();
            //_netInfoPanel.anchor = UIAnchorStyle.CenterVertical;

            //_dropdownToggleButton = AddUIComponent<UIButton>();
            //_dropdownToggleButton.size = _netInfoPanel.size;
            //_dropdownToggleButton.relativePosition = Vector3.zero;
            //_dropdownToggleButton.enabled = true;
            //_dropdownToggleButton.normalBgSprite = "OptionsDropbox";
            //_dropdownToggleButton.hoveredBgSprite = "ButtonWhite";
            //_dropdownToggleButton.focusedBgSprite = "OptionsDropboxHovered";
            //_dropdownToggleButton.pressedBgSprite = "OptionsDropboxHovered";
            //_dropdownToggleButton.opacity = 0.25f;

            //_dropdownToggleButton.FitWidth(_netInfoPanel, UIConstants.Padding);
            //_netInfoPanel.relativePosition = Vector3.zero;
        }

        public override void Start()
        {
            base.Start();

            AttachToEvents();
        }

        public void Render(NetInfoItem netInfo)
        {
            _netInfoButton.Render(netInfo);
        }

        private void AttachToEvents()
        {
            _netInfoButton.DropdownToggleButtonEventClick += DropdownToggleButton_eventClick;
        }

        private void DropdownToggleButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (_popup == null)
                OpenPopup();
            else
                ClosePopup();
        }

        private void DetachFromEvents()
        {

        }

        protected void OpenPopup()
        {
            // _dropdownToggleButton.isInteractive = false;
            var view = UIView.GetAView();
            //_netPopup = view.AddUIComponent(typeof(UINetPopup)) as UINetPopup;
            //_netPopup.absolutePosition = new Vector2(-1000, -1000);
            _popup = view.AddUIComponent(typeof(UINetListPopup)) as UINetListPopup;
            //var root = GetRootContainer();
            //_popup = root.AddUIComponent<UIPanel>();
            _popup.canFocus = true;
            // _popup.atlas = UIHelpers.Atlas;//CommonTextures.Atlas;
            //_popup.backgroundSprite = "OptionsDropboxListbox"; // CommonTextures.FieldHovered;
            // _popup.opacity = 0.5f;
            //_popup.ItemHover = ""; //CommonTextures.FieldNormal;
            //_popup.ItemSelected = ""; //CommonTextures.FieldFocused;
            //_popup.EntityHeight = 50f;
            //_popup.MaxVisibleItems = 3;
            _popup.FitWidth(this, UIConstants.Padding / 2);
            _popup.height = 150f;
            //_popup.maximumSize = new Vector2(_dropdownToggleButton.size.x, 500f);
            //_popup.zOrder = int.MaxValue;
            //_popup.UpdateItems(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes);
            _popup.Focus();
            //_popup.SelectedObject = Prefab;

            //_popup.eventKeyDown += OnPopupKeyDown;
            //_popup.OnSelectedChanged += OnSelectedChanged;

            //SetPopupPosition();
            //_popup.parent.eventPositionChanged += SetPopupPosition;

            _popup.absolutePosition = _netInfoButton.absolutePosition + new Vector3(0, _netInfoButton.height);

            //_popup.UpdateItems(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes);
        }

        public virtual void ClosePopup()
        {
            // _dropdownToggleButton.isInteractive = true;

            if (_popup != null)
            {
                //_popup.eventLeaveFocus -= OnPopupLeaveFocus;
                //_popup.eventKeyDown -= OnPopupKeyDown;

                //ComponentPool.Free(_popup);
                DestroyImmediate(_popup);
                _popup = null;
            }
        }

        //private void SetPopupPosition(UIComponent component = null, Vector2 value = default)
        //{
        //    if (_popup != null)
        //    {
        //        _popup.absolutePosition = _dropdownToggleButton.absolutePosition + new Vector3(0, _dropdownToggleButton.height);

        //        //UIView uiView = _popup.GetUIView();
        //        //var screen = uiView.GetScreenResolution();
        //        //var position = _dropdownToggleButton.relativePosition + new Vector3(0, _dropdownToggleButton.height);
        //        ////position.x = MathPos(position.x, _popup.width, screen.x);
        //        ////position.y = MathPos(position.y, _popup.height, screen.y);

        //        //_popup.relativePosition = position - _popup.parent.absolutePosition;
        //    }

        //    static float MathPos(float pos, float size, float screen) => pos + size > screen ? (screen - size < 0 ? 0 : screen - size) : Mathf.Max(pos, 0);
        //}

    }
}
