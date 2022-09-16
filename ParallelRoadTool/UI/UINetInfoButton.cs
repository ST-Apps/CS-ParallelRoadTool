using ColossalFramework.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    internal class UINetInfoButton : UIPanel
    {
        private UINetInfoPanel _netInfoPanel;
        private UIButton _toggleButton;

        public bool IsReadOnly
        {
            set
            {
                _toggleButton.isVisible = !value;
            }
        }

        public event MouseEventHandler DropdownToggleButtonEventClick
        {
            add { _toggleButton.eventClick += value; }
            remove { _toggleButton.eventClick -= value; }
        }

        public override void Awake()
        {
            clipChildren = true;
            size = new Vector2(400, 48 + UIConstants.Padding + UIConstants.Padding);
            relativePosition = Vector3.zero;

            _netInfoPanel = AddUIComponent<UINetInfoPanel>();
            _netInfoPanel.anchor = UIAnchorStyle.CenterVertical;

            _toggleButton = AddUIComponent<UIButton>();
            _toggleButton.size = _netInfoPanel.size;
            _toggleButton.relativePosition = Vector3.zero;
            _toggleButton.normalBgSprite = "OptionsDropbox";
            _toggleButton.hoveredBgSprite = "ButtonWhite";
            _toggleButton.focusedBgSprite = "OptionsDropboxHovered";
            _toggleButton.pressedBgSprite = "OptionsDropboxHovered";
            _toggleButton.opacity = 0.25f;

            _toggleButton.FitWidth(_netInfoPanel, UIConstants.Padding);
            _netInfoPanel.relativePosition = Vector3.zero;
        }

        public void Render(NetInfoItem netInfo, bool inList = false)
        {
            if (inList)
            {
                _toggleButton.normalBgSprite = "GenericPanel";
                _toggleButton.hoveredBgSprite = "ButtonWhite";
                _toggleButton.focusedBgSprite = "GenericPanel";
                _toggleButton.pressedBgSprite = "GenericPanel";
                _toggleButton.color = netInfo.Color;
                _toggleButton.opacity = 0.25f;
            }
            _netInfoPanel.Render(netInfo);
        }
    }
}
