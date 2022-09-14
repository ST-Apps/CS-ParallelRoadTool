using ColossalFramework.Globalization;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Interfaces;
using ParallelRoadTool.UI.Utils;
using System;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    internal class UINetSetupPanel : UIPanel, IUIListabeItem<NetInfoItem>
    {

        #region Properties/Fields
        
        // Taken from vanilla road building's overlay color
        private readonly Color ReadOnlyColor = new Color(0, 0.710f, 1, 0.5f);

        #endregion

        #region Events/Callbacks

        public event PropertyChangedEventHandler<int> DeleteNetworkButtonEventClicked;

        public event PropertyChangedEventHandler<bool> ReverseDirectionButtonEventCheckChanged
        {
            add { _reverseCheckbox.eventCheckChanged += value; }
            remove { _reverseCheckbox.eventCheckChanged -= value; }
        }

        private void DeleteButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            DeleteNetworkButtonEventClicked?.Invoke(this, CurrentIndex);
        }

        #endregion

        #region Unity

        #region Components

        private UINetInfoPanel _netInfoPanel;

        private UIPanel _offsetsPanel;
        private UITextField _horizontalOffsetField;
        private UITextField _verticalOffsetField;

        private UIPanel _buttonsPanel;
        private UICheckBox _reverseCheckbox;
        private UIButton _deleteButton;

        private bool _isReadOnly;

        public int CurrentIndex { get; set; }

        public string Id { get; set; }
        public bool IsReadOnly { set { _isReadOnly = value; color = ReadOnlyColor; HideTools(); } }

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

            // NetSetup/Buttons
            _buttonsPanel = AddUIComponent<UIPanel>();
            _buttonsPanel.name = $"{name}_Buttons";
            _buttonsPanel.autoLayout = true;
            _buttonsPanel.autoLayoutDirection = LayoutDirection.Vertical;
            _buttonsPanel.anchor = UIAnchorStyle.CenterVertical;
            _buttonsPanel.autoFitChildrenHorizontally = true;
            _buttonsPanel.autoFitChildrenVertically = true;

            // NetSetup/Buttons/Delete
            _deleteButton = UIHelpers.CreateUiButton(
                _buttonsPanel,
                new Vector2(UIConstants.SmallButtonSize, UIConstants.SmallButtonSize),
                string.Empty,
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "RemoveNetworkButton"),
                "Remove"
            );

            // NetSetup/Buttons/Reverse
            _reverseCheckbox = UIHelpers.CreateCheckbox(
                _buttonsPanel,
                new Vector2(UIConstants.SmallButtonSize, UIConstants.SmallButtonSize),
                "Reverse",
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ReverseToggleButton")
            );
        }

        public override void Start()
        {
            // Make NetInfoPanel wide enough to fill the empty space
            _netInfoPanel.width = width - _offsetsPanel.width - _buttonsPanel.width;

            AttachToEvents();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            DetachFromEvents();
        }

        private void AttachToEvents()
        {
            _deleteButton.eventClicked += DeleteButton_eventClicked;
        }

        private void DetachFromEvents()
        {
            _deleteButton.eventClicked -= DeleteButton_eventClicked;
        }

        #endregion

        #region Control

        internal void HideTools()
        {
            _offsetsPanel.isVisible = false;
            _buttonsPanel.isVisible = false;
        }

        public void Render(NetInfoItem netInfo)
        {            
            color = _isReadOnly ? ReadOnlyColor : netInfo.Color;
            _horizontalOffsetField.text = $"{netInfo.HorizontalOffset}";
            _verticalOffsetField.text = $"{netInfo.VerticalOffset}";

            _netInfoPanel.Render(netInfo);
        }

        #endregion
    }
}
