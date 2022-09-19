using ColossalFramework.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Shared;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Main
{
    /// <summary>
    /// This class is meant as a wrapper to <see cref="UINetInfoPanel"/> which adds an <see cref="UIButton"/> used to toggle the <see cref="UINetListPopup"/> instance.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class UINetInfoButton : UIPanel
    {
        #region Properties

        /// <summary>
        /// If this is true it means that the button must not be visible.
        /// </summary>
        public bool IsReadOnly
        {
            set => _toggleButton.isVisible = !value;
        }

        #endregion

        #region Events

        public event MouseEventHandler DropdownToggleButtonEventClick
        {
            add => _toggleButton.eventClick += value;
            remove => _toggleButton.eventClick -= value;
        }

        #endregion

        #region Unity

        #region Components

        private UINetInfoPanel _netInfoPanel;
        private UIButton _toggleButton;

        #endregion

        #region Lifecycle

        public override void Awake()
        {
            base.Awake();

            // Main
            clipChildren = true;
            size = new Vector2(400, 48 + UIConstants.Padding + UIConstants.Padding);
            relativePosition = Vector3.zero;

            // Main/NetInfo
            _netInfoPanel = AddUIComponent<UINetInfoPanel>();
            _netInfoPanel.anchor = UIAnchorStyle.CenterVertical;

            // Main/ToggleButton
            _toggleButton = AddUIComponent<UIButton>();
            _toggleButton.size = _netInfoPanel.size;
            _toggleButton.relativePosition = Vector3.zero;
            _toggleButton.normalBgSprite = "OptionsDropbox";
            _toggleButton.hoveredBgSprite = "ButtonWhite";
            _toggleButton.focusedBgSprite = "OptionsDropboxHovered";
            _toggleButton.pressedBgSprite = "OptionsDropboxHovered";
            _toggleButton.opacity = 0.25f;

            // Manually fix some sizing and positioning to have the button overlap the net info panel
            _toggleButton.FitWidth(_netInfoPanel, UIConstants.Padding);
            _netInfoPanel.relativePosition = Vector3.zero;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Forcefully destroy all children
            Destroy(_netInfoPanel);
            Destroy(_toggleButton);
        }

        #endregion

        #endregion

        #region Control

        #region Public API

        /// <summary>
        /// Renders the current component by performing the required render steps for the internal <see cref="UINetInfoPanel"/>.
        /// If this item is in a list we also change its sprites and color.
        /// </summary>
        /// <param name="netInfo"></param>
        /// <param name="inList"></param>
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

        #endregion

        #endregion
    }
}
