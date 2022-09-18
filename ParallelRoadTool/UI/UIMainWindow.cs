using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Utils;
using System.Collections.Generic;
using AlgernonCommons.Translation;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    /// <summary>
    /// Main UI for PRT.
    /// This window contains:
    /// - an header with the closing button
    /// - a row with all the tools and features
    /// - a list of panels used to display selected NetInfo instances
    /// </summary>
    public class UIMainWindow : UIPanel
    {

        #region Fields



        #endregion

        #region Properties



        #endregion

        #region Events

        public event PropertyChangedEventHandler<NetTypeItemEventArgs> NetTypeEventChanged
        {
            add => _networkListPanel.NetTypeEventChanged += value;
            remove => _networkListPanel.NetTypeEventChanged -= value;
        }

        public event MouseEventHandler CloseButtonEventClicked
        {
            add => _closeButton.eventClicked += value;
            remove => _closeButton.eventClicked -= value;
        }

        public event MouseEventHandler AddNetworkButtonEventClicked
        {
            add => _addNetworkButton.eventClicked += value;
            remove => _addNetworkButton.eventClicked -= value;
        }

        public event PropertyChangedEventHandler<int> DeleteNetworkButtonEventClicked
        {
            add => _networkListPanel.DeleteNetworkButtonEventClicked += value;
            remove => _networkListPanel.DeleteNetworkButtonEventClicked += value;
        }

        public event PropertyChangedEventHandler<bool> ToggleSnappingButtonEventCheckChanged
        {
            add => _toggleSnappingButton.eventCheckChanged += value;
            remove => _toggleSnappingButton.eventCheckChanged -= value;
        }

        public event MouseEventHandler ToggleDropdownButtonEventClick
        {
            add => _networkListPanel.ToggleDropdownButtonEventClick += value;
            remove => _networkListPanel.ToggleDropdownButtonEventClick -= value;
        }

        #endregion

        #region Callbacks



        #endregion

        #region Unity

        #region Controls

        private UIButton _closeButton;
        private UINetListPanel _networkListPanel;
        private UICheckBox _toggleSnappingButton;
        private UIButton _addNetworkButton;
        private UINetSetupPanel _currentNetworkSetupPanel;

        #endregion

        #region Lifecycle

        public override void Awake()
        {
            base.Awake();

            // Main
            name = $"{Configuration.ResourcePrefix}MainWindow";
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(512, 256);
            absolutePosition = new Vector2(100, 100);
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);
            autoLayoutPadding.top = 0;

            BuildHeader();
            BuildToolbar();

            // Main/CurrentNetwork
            _currentNetworkSetupPanel = AddUIComponent<UINetSetupPanel>();
            _currentNetworkSetupPanel.relativePosition = Vector2.zero;
            _currentNetworkSetupPanel.padding = padding;
            _currentNetworkSetupPanel.FitWidth(this, UIConstants.Padding);
            _currentNetworkSetupPanel.IsReadOnly = true;

            // Main/NetworkList
            _networkListPanel = AddUIComponent<UINetListPanel>();
            _networkListPanel.padding = padding;
            _networkListPanel.FitWidth(this, UIConstants.Padding);

            // Main/Spacer
            AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2f);
        }

        public override void Start()
        {
            base.Start();

            // Since our layout is now complete, we can disable autoLayout for all the panels to avoid wasting CPU cycle
            autoLayout = false;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();


            // Forcefully destroy all children
            Destroy(_closeButton);
            Destroy(_networkListPanel);
            Destroy(_toggleSnappingButton);
            Destroy(_addNetworkButton);
            Destroy(_currentNetworkSetupPanel);
        }

        #endregion

        #endregion

        #region Control

        #region Internals

        private void BuildHeader()
        {
            // Main/Header
            var headerPanel = AddUIComponent<UIPanel>();
            headerPanel.name = $"{name}_Header";
            headerPanel.padding = padding;
            headerPanel.FitWidth(this, UIConstants.Padding);
            headerPanel.height = UIConstants.SmallSize;

            // Main/Header/TitleLabel
            var titleLabel = headerPanel.AddUIComponent<UILabel>();
            titleLabel.name = $"{headerPanel.name}_TitleLabel";
            titleLabel.text = Mod.Instance.Name;
            titleLabel.AlignTo(headerPanel, UIAlignAnchor.TopLeft);
            titleLabel.anchor = UIAnchorStyle.CenterVertical;

            // Main/Header/DragHandle
            var dragHandle = headerPanel.AddUIComponent<UIDragHandle>();
            dragHandle.name = $"{headerPanel.name}_DragHandle";
            dragHandle.target = this;
            dragHandle.FitWidth(this, UIConstants.Padding);
            dragHandle.height = headerPanel.height;
            dragHandle.AlignTo(headerPanel, UIAlignAnchor.TopLeft);

            // Main/Header/CloseButton
            _closeButton = headerPanel.AddUIComponent<UIButton>();
            _closeButton.name = $"{headerPanel.name}_CloseButton";
            _closeButton.text = "";
            _closeButton.normalBgSprite = "buttonclose";
            _closeButton.hoveredBgSprite = "buttonclosehover";
            _closeButton.pressedBgSprite = "buttonclosepressed";
            _closeButton.size = new Vector2(UIConstants.SmallSize, UIConstants.SmallSize);
            _closeButton.anchor = UIAnchorStyle.CenterVertical;
            _closeButton.AlignTo(headerPanel, UIAlignAnchor.TopRight);
        }

        private void BuildToolbar()
        {
            // Main/Toolbar
            var toolbarPanel = AddUIComponent<UIPanel>();
            toolbarPanel.name = $"{name}_Toolbar";
            toolbarPanel.FitWidth(this, UIConstants.Padding);
            toolbarPanel.height = UIConstants.MiddleSize;

            // Main/Toolbar/Options
            var optionsPanel = toolbarPanel.AddUIComponent<UIPanel>();
            optionsPanel.name = $"{toolbarPanel.name}_Options";
            optionsPanel.backgroundSprite = "GenericPanel";
            optionsPanel.color = new Color32(206, 206, 206, 255);
            optionsPanel.size = toolbarPanel.size;
            optionsPanel.width = optionsPanel.width / 2 - UIConstants.Padding / 2f;
            optionsPanel.AlignTo(toolbarPanel, UIAlignAnchor.TopLeft);
            optionsPanel.autoLayout = true;
            optionsPanel.autoLayoutDirection = LayoutDirection.Horizontal;

            // Main/Toolbar/Options/SavePresetButton
            var savePresetButton = UIHelpers.CreateUiButton(
                optionsPanel,
                new Vector2(UIConstants.MiddleSize, UIConstants.MiddleSize),
                string.Empty,
                Translations.Translate("TOOLTIP_SAVE_BUTTON"),
                "Save");
            savePresetButton.name = $"{optionsPanel.name}_SavePreset";

            // Main/Toolbar/Options/LoadPresetButton
            var loadPresetButton = UIHelpers.CreateUiButton(
                optionsPanel,
                new Vector2(UIConstants.MiddleSize, UIConstants.MiddleSize),
                string.Empty,
                Translations.Translate("TOOLTIP_LOAD_BUTTON"),
                "Load");
            loadPresetButton.name = $"{optionsPanel.name}_LoadPreset";

            // Main/Toolbar/Tools
            var toolsPanel = toolbarPanel.AddUIComponent<UIPanel>();
            toolsPanel.name = $"{toolbarPanel.name}_Tools";
            toolsPanel.backgroundSprite = "GenericPanel";
            toolsPanel.color = new Color32(206, 206, 206, 255);
            toolsPanel.size = toolbarPanel.size;
            toolsPanel.width = toolsPanel.width / 2 - UIConstants.Padding / 2f;
            toolsPanel.AlignTo(toolbarPanel, UIAlignAnchor.TopRight);
            toolsPanel.autoLayout = true;
            toolsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            toolsPanel.autoLayoutStart = LayoutStart.TopRight;

            // Main/Toolbar/Tools/ToggleSnappingButton
            _toggleSnappingButton = UIHelpers.CreateCheckBox(
                toolsPanel,
                "Snapping",
                Translations.Translate("TOOLTIP_SNAPPING_TOGGLE_BUTTON"),
                new Vector2(UIConstants.MiddleSize, UIConstants.MiddleSize),
                false
            );
            _toggleSnappingButton.name = $"{toolsPanel.name}_ToggleSnapping";

            // Main/Toolbar/Tools/AddNetworkButton
            _addNetworkButton = UIHelpers.CreateUiButton(
                toolsPanel,
                new Vector2(UIConstants.MiddleSize, UIConstants.MiddleSize),
                string.Empty,
                Translations.Translate("TOOLTIP_ADD_NETWORK_BUTTON"),
                "Add"
            );
            _addNetworkButton.name = $"{toolsPanel.name}_AddNetwork";

            // Since our layout is now complete, we can disable autoLayout for all the panels to avoid wasting CPU cycle
            optionsPanel.autoLayout = false;
            toolsPanel.autoLayout = false;
        }

        #endregion

        #region Public API

        /// <summary>
        /// To refresh networks we just pass the new list to the <see cref="UINetListPanel"/> which will do the rendering.
        /// This is called ONLY on deletions as adding a new network will trigger <see cref="AddNetwork"/>.
        /// </summary>
        /// <param name="networks"></param>
        public void RefreshNetworks(List<NetInfoItem> networks)
        {
            // Before refreshing networks we restore autolayout to make the panel react to the new element
            autoLayout = true;

            _networkListPanel.RefreshNetworks(networks);

            // Now that networks have been refreshed we can disable autolayout again
            autoLayout = false;
        }

        /// <summary>
        /// Re-renders the current network panel with the newly provided <see cref="NetInfoItem"/>.
        /// </summary>
        /// <param name="netInfo"></param>
        public void UpdateCurrentNetwork(NetInfoItem netInfo)
        {
            _currentNetworkSetupPanel.Render(netInfo);
        }

        /// <summary>
        /// Adds the provided <see cref="NetInfoItem"/> to <see cref="UINetListPanel"/> and renders it.
        /// </summary>
        /// <param name="netInfo"></param>
        public void AddNetwork(NetInfoItem netInfo)
        {
            // Before adding a new network we restore autolayout to make the panel react to the new element
            autoLayout = true;

            _networkListPanel.AddNetwork(netInfo);

            // Now that the item has been added we can disable autolayout again
            autoLayout = false;
        }

        #endregion

        #endregion

    }
}
