using ColossalFramework.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Shared;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Main
{
    /// <summary>
    ///     This class is meant as a wrapper to <see cref="UINetInfoPanel" /> which adds an <see cref="UIButton" /> used to
    ///     toggle the <see cref="UINetListPopup" /> instance.
    /// </summary>

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class UINetInfoButton : UIPanel
    {
        #region Fields

        private static readonly object Lock = new();

        #endregion

        #region Properties

        /// <summary>
        ///     If this is true it means that the button must not be visible.
        /// </summary>
        public bool IsReadOnly
        {
            set => _toggleButton.isVisible = !value;
        }

        #endregion

        #region Callbacks

        private void ToggleButtonOnEventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            lock (Lock)
            {
                if (_netSelectionPopup != null)
                {
                    _netSelectionPopup.Close();
                    _netSelectionPopup = null;
                    return;
                }

                _netSelectionPopup = UIView.GetAView().AddUIComponent(typeof(UINetSelectionPopup)) as UINetSelectionPopup;
                if (_netSelectionPopup == null) return;

                // Register events
                _netSelectionPopup.OnPopupSelectionChanged += (_, value) =>
                                                              {
                                                                  OnPopupSelectionChanged?.Invoke(_netSelectionPopup, value);
                                                                  _netSelectionPopup.Close();
                                                              };

                // Open the popup
                _netSelectionPopup.Open(this, _netInfoPanel.NetInfoItem);
                OnPopupOpened?.Invoke(this, _netSelectionPopup);
            }
        }

        #endregion

        #region Events

        public event ChildComponentEventHandler OnPopupOpened;

        public event PropertyChangedEventHandler<NetTypeItemEventArgs> OnPopupSelectionChanged;

        //{
        //    add => _netSelectionPopup.OnPopupSelectionChanged += value;
        //    remove => _netSelectionPopup.OnPopupSelectionChanged -= value;
        //}

        #endregion

        #region Unity

        #region Components

        private UINetInfoPanel _netInfoPanel;
        private UIButton _toggleButton;
        private UINetSelectionPopup _netSelectionPopup;

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

        public override void Start()
        {
            base.Start();

            AttachToEvents();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            DetachFromEvents();

            // Forcefully destroy all children
            Destroy(_netInfoPanel);
            Destroy(_toggleButton);
        }

        #endregion

        #endregion

        #region Control

        #region Internals

        private void AttachToEvents()
        {
            _toggleButton.eventClicked += ToggleButtonOnEventClicked;
        }

        private void DetachFromEvents()
        {
            _toggleButton.eventClicked -= ToggleButtonOnEventClicked;
        }

        #endregion

        #region Public API

        /// <summary>
        ///     Renders the current component by performing the required render steps for the internal
        ///     <see cref="UINetInfoPanel" />.
        ///     If this item is in a list we also change its sprites and color.
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

            _netInfoPanel.NetInfoItem = netInfo;
        }

        #endregion

        #endregion
    }
}
