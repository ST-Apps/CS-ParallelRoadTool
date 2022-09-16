using ColossalFramework;
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
    internal class UINetListPopup : UIPanel
    {
        // We only have padding for bottom side to separate multiple elements
        private readonly RectOffset LayoutPadding = new RectOffset(UIConstants.Padding, UIConstants.Padding, 0, UIConstants.Padding);

        #region Properties

        /// <summary>
        /// Number of <see cref="NetInfoItem"/> that are shown by the popup.
        /// </summary>
        public int MaxVisibleItems { get; set; } = 5;

        #endregion

        #region Fields

        private float _itemHeight;

        #endregion

        #region Unity

        #region Components

        private UITextField _searchTextField;
        private UIPanel _netTypesContainer;
        private UIScrollablePanel _netTypesList;
        private UIScrollbar _scrollbar;

        #endregion

        #region Lifecycle

        public override void Awake()
        {
            // Main
            canFocus = true;
            backgroundSprite = "OptionsDropboxListbox";
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutPadding = LayoutPadding;
            autoLayoutDirection = LayoutDirection.Vertical;

            // Main/Spacer
            AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2);

            // Main/SearchText
            _searchTextField = UIHelpers.CreateTextField(this);
            _searchTextField.numericalOnly =
                _searchTextField.allowFloats =
                _searchTextField.allowNegative = false;
            _searchTextField.submitOnFocusLost = true;
            _searchTextField.horizontalAlignment = UIHorizontalAlignment.Center;
            _searchTextField.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;

            // Main/Spacer
            AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2);

            // Main/NetTypesContainer
            _netTypesContainer = AddUIComponent<UIPanel>();
            _netTypesContainer.autoFitChildrenHorizontally = true;
            //_netTypesContainer.autoFitChildrenVertically = true;
            _netTypesContainer.autoLayout = true;
            _netTypesContainer.autoLayoutDirection = LayoutDirection.Horizontal;

            // Main/NetTypesContainer/NetTypes
            _netTypesList = _netTypesContainer.AddUIComponent<UIScrollablePanel>();
            _netTypesList.clipChildren = true;
            //_netTypesList.height = 300;
            _netTypesList.autoLayout = true;
            _netTypesList.autoLayoutDirection = LayoutDirection.Vertical;
            _netTypesList.scrollWheelDirection = UIOrientation.Vertical;
            _netTypesList.autoLayoutPadding = LayoutPadding;

            // Main/NetTypesContainer/Scrollbar
            _scrollbar = _netTypesContainer.AddUIComponent<UIScrollbar>();
            //_scrollbar.height = 300; // TODO: height based on component's sizes and max amount of displayed rows
            _scrollbar.orientation = UIOrientation.Vertical;
            _scrollbar.pivot = UIPivotPoint.TopLeft;
            _scrollbar.minValue = 0;
            _scrollbar.value = 0;
            _scrollbar.incrementAmount = 50;
            _scrollbar.autoHide = true;
            _scrollbar.width = 10;

            // Main/NetTypesContainer/Scrollbar/TrackSprite
            var trackSprite = _scrollbar.AddUIComponent<UISlicedSprite>();
            trackSprite.relativePosition = Vector2.zero;
            trackSprite.autoSize = true;
            trackSprite.anchor = UIAnchorStyle.All;
            trackSprite.size = trackSprite.parent.size;
            trackSprite.fillDirection = UIFillDirection.Vertical;
            trackSprite.spriteName = "ScrollbarTrack";
            _scrollbar.trackObject = trackSprite;

            // Main/NetTypesContainer/Scrollbar/ThumbSprite
            var thumbSprite = trackSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = thumbSprite.parent.width;
            thumbSprite.spriteName = "ScrollbarThumb";
            _scrollbar.thumbObject = thumbSprite;

            _netTypesList.verticalScrollbar = _scrollbar;

            // Main/Spacer
            AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2);
        }

        public override void Start()
        {
            base.Start();

            _searchTextField.FitWidth(this, 0);
            _netTypesContainer.FitWidth(this, 0);
            _netTypesList.FitWidth(_netTypesContainer, (int)(UIConstants.Padding / 2 + _scrollbar.width));

            foreach (var net in Singleton<ParallelRoadTool>.instance.AvailableRoadTypes)
            {
                var netInfoItem = new NetInfoItem(net);
                var netInfoButton = _netTypesList.AddUIComponent<UINetInfoButton>();
                netInfoButton.Render(netInfoItem, true);

                // TODO: make it constant
                _itemHeight = netInfoButton.height;
            }
            // _netTypesList.FitToContents();

            UpdateHeight();
        }

        private void UpdateHeight()
        {
            _netTypesContainer.height = (_itemHeight + _netTypesList.autoLayoutPadding.top + _netTypesList.autoLayoutPadding.bottom) * MaxVisibleItems;
            _netTypesList.FitHeight(_netTypesContainer, 0);
            _scrollbar.FitHeight(_netTypesContainer, 0);
        }

        #endregion

        #endregion

        #region Control
        #endregion
    }
}
