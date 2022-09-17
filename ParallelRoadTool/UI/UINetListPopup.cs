//using ColossalFramework.UI;
//using ParallelRoadTool.Models;
//using ParallelRoadTool.UI.Utils;
//using CSUtil.Commons;
//using UnityEngine;

//namespace ParallelRoadTool.UI
//{
//    internal class UINetListPopup : UIPanel
//    {
//        #region Fields

//        private readonly RectOffset _layoutPadding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);

//        private readonly object _lock = new();

//        private UIComponent _currentCaller;

//        #endregion

//        // TODO: lost focus, click outside and ESC button should close it

//        #region Properties

//        /// <summary>
//        /// Number of <see cref="NetInfoItem"/> that are shown by the popup.
//        /// </summary>
//        public int MaxVisibleItems { get; set; } = 5;

//        #endregion

//        #region Unity

//        #region Components

//        private UITextField _searchTextField;
//        private UIPanel _netTypesContainer;

//        // TODO: replace with UIFastList?
//        private UIScrollablePanel _netTypesList;
//        private UIScrollbar _scrollbar;

//        #endregion

//        #region Lifecycle

//        public override void Awake()
//        {
//            // Main
//            name = $"{Configuration.ResourcePrefix}NetworksPopup";
//            canFocus = true;
//            backgroundSprite = "OptionsDropboxListbox";
//            autoFitChildrenVertically = true;
//            autoLayout = true;
//            autoLayoutPadding = _layoutPadding;
//            autoLayoutDirection = LayoutDirection.Vertical;

//            // Main/Spacer
//            AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2f);

//            // Main/SearchText
//            _searchTextField = UIHelpers.CreateTextField(this);
//            _searchTextField.numericalOnly =
//                _searchTextField.allowFloats =
//                _searchTextField.allowNegative = false;
//            _searchTextField.submitOnFocusLost = true;
//            _searchTextField.horizontalAlignment = UIHorizontalAlignment.Center;
//            _searchTextField.anchor = UIAnchorStyle.CenterHorizontal | UIAnchorStyle.CenterVertical;

//            // Main/Spacer
//            AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2f);

//            // Main/NetTypesContainer
//            _netTypesContainer = AddUIComponent<UIPanel>();
//            _netTypesContainer.autoFitChildrenHorizontally = true;
//            _netTypesContainer.autoLayout = true;
//            _netTypesContainer.autoLayoutDirection = LayoutDirection.Horizontal;

//            // Main/NetTypesContainer/NetTypes
//            _netTypesList = _netTypesContainer.AddUIComponent<UIScrollablePanel>();
//            _netTypesList.clipChildren = true;
//            _netTypesList.autoLayout = true;
//            _netTypesList.autoLayoutDirection = LayoutDirection.Vertical;
//            _netTypesList.scrollWheelDirection = UIOrientation.Vertical;
//            _netTypesList.autoLayoutPadding = _layoutPadding;

//            // Main/NetTypesContainer/Scrollbar
//            _scrollbar = _netTypesContainer.AddUIComponent<UIScrollbar>();
//            _scrollbar.orientation = UIOrientation.Vertical;
//            _scrollbar.pivot = UIPivotPoint.TopLeft;
//            _scrollbar.minValue = 0;
//            _scrollbar.value = 0;
//            _scrollbar.incrementAmount = 50;
//            _scrollbar.autoHide = true;
//            _scrollbar.width = 10;

//            // Main/NetTypesContainer/Scrollbar/TrackSprite
//            var trackSprite = _scrollbar.AddUIComponent<UISlicedSprite>();
//            trackSprite.relativePosition = Vector2.zero;
//            trackSprite.autoSize = true;
//            trackSprite.anchor = UIAnchorStyle.All;
//            trackSprite.size = trackSprite.parent.size;
//            trackSprite.fillDirection = UIFillDirection.Vertical;
//            trackSprite.spriteName = "ScrollbarTrack";

//            // Main/NetTypesContainer/Scrollbar/ThumbSprite
//            var thumbSprite = trackSprite.AddUIComponent<UISlicedSprite>();
//            thumbSprite.relativePosition = Vector2.zero;
//            thumbSprite.fillDirection = UIFillDirection.Vertical;
//            thumbSprite.autoSize = true;
//            thumbSprite.width = thumbSprite.parent.width;
//            thumbSprite.spriteName = "ScrollbarThumb";

//            // Link all the components needed for the scrollbar
//            // TODO: this only works if you scroll exactly over the bar itself, we need to be able to scroll on the panel too.
//            _scrollbar.trackObject = trackSprite;
//            _scrollbar.thumbObject = thumbSprite;
//            _netTypesList.verticalScrollbar = _scrollbar;

//            // Main/Spacer
//            AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2f);

//            // Force starting hidden
//            // TODO: aaaand it stopped working, now it starts as not enabled so nothing is shown ever
//            Toggle(null, true);
//            m_IsEnabled = true;
//            isEnabled = true;
//            enabled = true;
//        }

//        public override void Start()
//        {
//            base.Start();

//            _searchTextField.FitWidth(this, 0);
//            _netTypesContainer.FitWidth(this, 0);
//            _netTypesList.FitWidth(_netTypesContainer, (int)(UIConstants.Padding / 2f + _scrollbar.width));

//            AttachToEvents();

//            // TODO: data must be provided by UIController at startup
//            //foreach (var net in Singleton<ParallelRoadTool>.instance.AvailableRoadTypes)
//            //{
//            //    var netInfoItem = new NetInfoItem(net);
//            //    var netInfoButton = _netTypesList.AddUIComponent<UINetInfoButton>();
//            //    netInfoButton.Render(netInfoItem, true);
//            //}

//            FitHeightToVisibleComponents();
//        }

//        public override void OnDestroy()
//        {
//            base.OnDestroy();

//            DetachFromEvents();
//        }

//        #endregion

//        #endregion

//        #region Internal

//        private void AttachToEvents()
//        {
//            eventLostFocus += UINetListPopup_eventLostFocus;
//            eventMouseLeave += UINetListPopup_eventMouseLeave;
//        }

//        private void UINetListPopup_eventMouseLeave(UIComponent component, UIMouseEventParameter eventParam)
//        {
//            Toggle(_currentCaller, true);
//        }

//        private void UINetListPopup_eventLostFocus(UIComponent component, UIFocusEventParameter eventParam)
//        {
//            Toggle(_currentCaller, true);
//        }

//        private void DetachFromEvents()
//        {

//        }

//        /// <summary>
//        /// Fit height based on the number of visible components and their height.
//        /// </summary>
//        private void FitHeightToVisibleComponents()
//        {
//            _netTypesContainer.height = (UIConstants.NetInfoPanelHeight + _netTypesList.autoLayoutPadding.top + _netTypesList.autoLayoutPadding.bottom) * MaxVisibleItems;
//            _netTypesList.FitHeight(_netTypesContainer, 0);
//            _scrollbar.FitHeight(_netTypesContainer, 0);
//        }

//        #endregion

//        #region Control

//        private bool visible;

//        public void Toggle(UIComponent caller, bool forceClose = false)
//        {
//            lock (_lock)
//            {
//                if (forceClose || visible)
//                {
//                    Log._Debug($"[{nameof(UINetListPopup)}.{nameof(Toggle)}] Popup was already visible, closing...");

//                    // Popup is currently owned by another component, we need to hide it first
//                    // Since we don't want to rebuild it every-time we need it, we just hide it and move it away
//                    Unfocus();
//                    absolutePosition = new Vector2(-1000, -1000);
//                    SendToBack();
//                    visible = false;

//                    // Finally, we can release anything related to our current caller
//                    _currentCaller.eventPositionChanged -= Caller_eventPositionChanged;
//                    _currentCaller = null;
//                }
//                else
//                {
//                    Log._Debug($"[{nameof(UINetListPopup)}.{nameof(Toggle)}] Popup was not visible, opening...");

//                    // Change popup's ownership
//                    _currentCaller = caller;

//                    // We can now set the current position based on caller and restore visibility
//                    this.FitWidth(_currentCaller, 0);
//                    absolutePosition = _currentCaller.absolutePosition + new Vector3(0, _currentCaller.height);
//                    FitHeightToVisibleComponents();
//                    visible = true;
//                    BringToFront();
//                    Focus();

//                    Log._Debug($"[{nameof(UINetListPopup)}.{nameof(Toggle)}] Popup's new position is {absolutePosition}");
//                    Log._Debug($"[{nameof(UINetListPopup)}.{nameof(Toggle)}] Popup's new size is {size}");

//                    // Finally, we need to keep track of parent's position changes
//                    _currentCaller.eventPositionChanged += Caller_eventPositionChanged;
//                }
//            }
//        }

//        private void Caller_eventPositionChanged(UIComponent component, Vector2 value)
//        {
//            absolutePosition = _currentCaller.absolutePosition + new Vector3(0, _currentCaller.height);
//        }

//        #endregion
//    }
//}
