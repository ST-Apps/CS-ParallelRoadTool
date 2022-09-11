using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Utils;
using System.Linq;
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
        #region Constants

        // We don't use padding for top side
        private readonly RectOffset LayoutPadding = new RectOffset(UIConstants.Padding, UIConstants.Padding, 0, UIConstants.Padding);

        #endregion

        #region Unity

        #region Components

        private UIButton _closeButton;
        private UINetworkListPanel _networkListPanel;
        private UICheckBox _toggleSnappingButton;
        private UIButton _addNetworkButton;
        private UINetInfoPanel _currentNetworkInfoPanel;

        #endregion

        #region Events

        public event MouseEventHandler CloseButtonEventClicked
        {
            add { _closeButton.eventClicked += value; }
            remove { _closeButton.eventClicked -= value; }
        }

        public event MouseEventHandler AddNetworkButtonEventClicked
        {
            add { _addNetworkButton.eventClicked += value; }
            remove { _addNetworkButton.eventClicked -= value; }
        }

        public event PropertyChangedEventHandler<bool> ToggleSnappingButtonEventCheckChanged
        {
            add { _toggleSnappingButton.eventCheckChanged += value; }
            remove { _toggleSnappingButton.eventCheckChanged -= value; }
        }

        #endregion

        private void BuildHeader()
        {
            // Main/Header
            var headerPanel = AddUIComponent<UIPanel>();
            headerPanel.name = $"{name}_Header";
            headerPanel.padding = padding;
            headerPanel.FitWidth(this, UIConstants.Padding);
            headerPanel.height = UIConstants.SmallBarHeight;

            // Main/Header/TitleLabel
            var titleLabel = headerPanel.AddUIComponent<UILabel>();
            titleLabel.name = $"{headerPanel.name}_TitleLabel";
            titleLabel.text = ModInfo.ModName;
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
            _closeButton.size = new Vector2(UIConstants.SmallButtonSize, UIConstants.SmallButtonSize);
            _closeButton.anchor = UIAnchorStyle.CenterVertical;
            _closeButton.AlignTo(headerPanel, UIAlignAnchor.TopRight);
        }

        private void BuildToolbar()
        {
            // Main/Toolbar
            var toolbarPanel = AddUIComponent<UIPanel>();
            toolbarPanel.name = $"{name}_Toolbar";
            toolbarPanel.FitWidth(this, UIConstants.Padding);
            toolbarPanel.height = UIConstants.MiddleBarHeight;

            // Main/Toolbar/Options
            var optionsPanel = toolbarPanel.AddUIComponent<UIPanel>();
            optionsPanel.name = $"{toolbarPanel.name}_Options";
            optionsPanel.backgroundSprite = "GenericPanel";
            optionsPanel.color = new Color32(206, 206, 206, 255);
            optionsPanel.size = toolbarPanel.size;
            optionsPanel.width = (optionsPanel.width / 2) - (UIConstants.Padding / 2);
            optionsPanel.AlignTo(toolbarPanel, UIAlignAnchor.TopLeft);
            optionsPanel.autoLayout = true;
            optionsPanel.autoLayoutDirection = LayoutDirection.Horizontal;

            // Main/Toolbar/Options/SavePresetButton
            var savePresetButton = UIHelpers.CreateUiButton(
                optionsPanel,
                new Vector2(UIConstants.MiddleButtonSize, UIConstants.MiddleButtonSize),
                string.Empty,
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "SaveButton"),
                "Save");
            savePresetButton.name = $"{optionsPanel.name}_SavePreset";

            // Main/Toolbar/Options/LoadPresetButton
            var loadPresetButton = UIHelpers.CreateUiButton(
                optionsPanel,
                new Vector2(UIConstants.MiddleButtonSize, UIConstants.MiddleButtonSize),
                string.Empty,
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "LoadButton"),
                "Load");
            loadPresetButton.name = $"{optionsPanel.name}_LoadPreset";

            // Main/Toolbar/Tools
            var toolsPanel = toolbarPanel.AddUIComponent<UIPanel>();
            toolsPanel.name = $"{toolbarPanel.name}_Tools";
            toolsPanel.backgroundSprite = "GenericPanel";
            toolsPanel.color = new Color32(206, 206, 206, 255);
            toolsPanel.size = toolbarPanel.size;
            toolsPanel.width = (toolsPanel.width / 2) - (UIConstants.Padding / 2);
            toolsPanel.AlignTo(toolbarPanel, UIAlignAnchor.TopRight);
            toolsPanel.autoLayout = true;
            toolsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            toolsPanel.autoLayoutStart = LayoutStart.TopRight;

            // Main/Toolbar/Tools/ToggleSnappingButton
            _toggleSnappingButton = UIHelpers.CreateCheckBox(
                toolsPanel,
                "Snapping",
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "SnappingToggleButton"),
                new Vector2(UIConstants.MiddleButtonSize, UIConstants.MiddleButtonSize),
                false
            );
            _toggleSnappingButton.name = $"{toolsPanel.name}_ToggleSnapping";

            // Main/Toolbar/Tools/AddNetworkButton
            _addNetworkButton = UIHelpers.CreateUiButton(
                toolsPanel,
                new Vector2(UIConstants.MiddleButtonSize, UIConstants.MiddleButtonSize),
                string.Empty,
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "AddNetworkButton"),
                "Add"
            );
            _addNetworkButton.name = $"{toolsPanel.name}_AddNetwork";
        }

        #region Lifecycle

        private void AttachUIEvents()
        {
        }

        private void DetachUIEvents()
        {
        }

        public override void Awake()
        {
            // Main
            name = $"{Configuration.ResourcePrefix}MainWindow";
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(512, 256);
            absolutePosition = new Vector2(100, 100);
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding = LayoutPadding;

            BuildHeader();
            BuildToolbar();

            // Main/CurrentNetwork
            _currentNetworkInfoPanel = AddUIComponent<UINetInfoPanel>();
            _currentNetworkInfoPanel.padding = padding;
            _currentNetworkInfoPanel.FitWidth(this, UIConstants.Padding);
            _currentNetworkInfoPanel.MakeReadOnly();

            // Main/NetworkList
            _networkListPanel = AddUIComponent<UINetworkListPanel>();
            _networkListPanel.padding = padding;
            _networkListPanel.FitWidth(this, UIConstants.Padding);

            // Main/Spacer
            AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2);
        }

        public override void Start()
        {
            AttachUIEvents();
            //// TODO: remove
            //_networkListPanel.AddNetwork(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(4).First(), true);
            _networkListPanel.AddNetwork(new ExtendedNetInfo(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(2).First()));
            _networkListPanel.AddNetwork(new ExtendedNetInfo(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(22).First()));
            _networkListPanel.AddNetwork(new ExtendedNetInfo(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(42).First()));
            _networkListPanel.AddNetwork(new ExtendedNetInfo(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(44).First()));
            _networkListPanel.AddNetwork(new ExtendedNetInfo(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(76).First()));

            ////networkListPanel.UINetSetupPanelClicked += (s) =>
            ////{
            ////    var uiObject = new GameObject();
            ////    uiObject.transform.parent = UIView.GetAView().transform;
            ////    var messageBox = uiObject.AddComponent<UIPanel>();
            ////    messageBox.size = new Vector2(300, 300);
            ////    messageBox.color = s.color;
            ////    messageBox.absolutePosition = new Vector2(100, 100);

            ////    UIView.PushModal(messageBox);
            ////    messageBox.Show(true);
            ////    messageBox.Focus();

            ////    // TODO: not working
            ////    // TODO: modal with search box and a scrollable panel of UINetInfoPanel

            ////    Log._Debug($"[{nameof(UIMainWindow)}.{nameof(Start)}] Pushed modal to position {messageBox.absolutePosition}");
            ////};
        }

        public override void OnDestroy()
        {
            DetachUIEvents();
        }

        #endregion

        #endregion
    }
}
