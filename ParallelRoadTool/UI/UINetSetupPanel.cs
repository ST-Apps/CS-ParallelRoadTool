﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Utils;
using ParallelRoadTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    internal class UINetSetupPanel : UIPanel
    {
        #region Unity

        #region Components

        private UINetInfoPanel _netInfoPanel;

        private UIPanel _offsetsPanel;
        private UITextField _horizontalOffsetField;
        private UITextField _verticalOffsetField;

        private UIPanel _buttonsPanel;
        private UICheckBox _reverseCheckbox;
        private UIButton _deleteButton;

        #endregion

        public override void Awake()
        {
            // NetSetup
            name = $"{Configuration.ResourcePrefix}NetSetup";
            backgroundSprite = "GenericPanel";
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Horizontal;

            // NetSetup/NetInfo
            _netInfoPanel = AddUIComponent<UINetInfoPanel>();
            _netInfoPanel.anchor = UIAnchorStyle.CenterVertical;

            // NetSetup/Offsets
            _offsetsPanel = AddUIComponent<UIPanel>();
            _offsetsPanel.name = $"{name}_Offsets";
            _offsetsPanel.autoLayout = true;
            _offsetsPanel.autoLayoutDirection = LayoutDirection.Vertical;
            _offsetsPanel.anchor = UIAnchorStyle.CenterVertical;
            _offsetsPanel.autoFitChildrenHorizontally = true;
            _offsetsPanel.autoFitChildrenVertically = true;

            // NetSetup/Offsets/Spacer
            _offsetsPanel.AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2);

            // NetSetup/Offsets/Horizontal
            var horizontalOffsetPanel = _offsetsPanel.AddUIComponent<UIPanel>();
            horizontalOffsetPanel.autoFitChildrenHorizontally = true;
            horizontalOffsetPanel.autoFitChildrenVertically = true;
            horizontalOffsetPanel.autoLayout = true;
            horizontalOffsetPanel.autoLayoutDirection = LayoutDirection.Horizontal;

            // NetSetup/Offsets/Horizontal/Icon
            UIHelpers.CreateUISprite(horizontalOffsetPanel, "HorizontalOffset");

            // NetSetup/Offsets/Horizontal/Text
            _horizontalOffsetField = UIHelpers.CreateTextField(horizontalOffsetPanel);
            _horizontalOffsetField.numericalOnly =
                _horizontalOffsetField.allowFloats =
                _horizontalOffsetField.allowNegative =
                _horizontalOffsetField.submitOnFocusLost = true;
            _horizontalOffsetField.tooltip = Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "HorizontalOffset");

            // NetSetup/Offsets/Vertical
            var verticalOffsetPanel = _offsetsPanel.AddUIComponent<UIPanel>();
            verticalOffsetPanel.autoFitChildrenHorizontally = true;
            verticalOffsetPanel.autoFitChildrenVertically = true;
            verticalOffsetPanel.autoLayout = true;
            verticalOffsetPanel.autoLayoutDirection = LayoutDirection.Horizontal;

            // NetSetup/Offsets/Vertical/Icon
            UIHelpers.CreateUISprite(verticalOffsetPanel, "VerticalOffset");

            // NetSetup/Offsets/Vertical/Text
            _verticalOffsetField = UIHelpers.CreateTextField(verticalOffsetPanel);
            _verticalOffsetField.numericalOnly =
                _verticalOffsetField.allowFloats =
                    _verticalOffsetField.allowNegative =
                        _verticalOffsetField.submitOnFocusLost = true;
            _verticalOffsetField.tooltip = Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "VerticalOffset");

            // NetSetup/Offsets/Spacer
            // _offsetsPanel.AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding);

            // NetSetup/Buttons
            _buttonsPanel = AddUIComponent<UIPanel>();
            _buttonsPanel.name = $"{name}_Buttons";
            _buttonsPanel.autoLayout = true;
            _buttonsPanel.autoLayoutDirection = LayoutDirection.Vertical;
            _buttonsPanel.anchor = UIAnchorStyle.CenterVertical;
            _buttonsPanel.autoFitChildrenHorizontally = true;
            _buttonsPanel.autoFitChildrenVertically = true;

            // NetSetup/Buttons/Spacer
            // _buttonsPanel.AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding);

            // NetSetup/Buttons/Delete
            _deleteButton = UIHelpers.CreateUiButton(
                _buttonsPanel,
                new Vector2(UIConstants.SmallButtonSize, UIConstants.SmallButtonSize),
                string.Empty,
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "RemoveNetworkButton"),
                "Remove"
            );

            // NetSetup/Buttons/Reverse
            _reverseCheckbox = UIHelpers.CreateCheckBox(
                _buttonsPanel,
                new Vector2(UIConstants.SmallButtonSize, UIConstants.SmallButtonSize),
                "Reverse",
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ReverseToggleButton"),
                false
            );

            // NetSetup/Buttons/Spacer
            // _buttonsPanel.AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding);
        }

        public override void Start()
        {
            // Make NetInfoPanel wide enough to fill the empty space
            _netInfoPanel.width = width - _offsetsPanel.width -_buttonsPanel.width;
        }

        #endregion

        #region Control

        internal void Refresh(NetInfo netInfo)
        {
            _netInfoPanel.Refresh(netInfo);
        }

        #endregion
    }
}