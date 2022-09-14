using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Utils;
using System.Collections.Generic;
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

        public void RefreshNetworks(List<NetInfoItem> networks)
        {
            _networkListPanel.RefreshNetworks(networks);
        }

        #region Constants

        // We don't use padding for top side
        private readonly RectOffset LayoutPadding = new RectOffset(UIConstants.Padding, UIConstants.Padding, 0, UIConstants.Padding);

        #endregion

        #region Unity

        #region Components

        private UIButton _closeButton;
        private UINetListPanel _networkListPanel;
        private UICheckBox _toggleSnappingButton;
        private UIButton _addNetworkButton;
        private UINetSetupPanel _currentNetworkSetupPanel;

        #endregion

        #region Events

        public event PropertyChangedEventHandler<float> OnHorizontalOffsetKeypress;
        public event PropertyChangedEventHandler<float> OnVerticalOffsetKeypress;

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

        public event PropertyChangedEventHandler<int> DeleteNetworkButtonEventClicked
        {
            add { _networkListPanel.DeleteNetworkButtonEventClicked += value; }
            remove { _networkListPanel.DeleteNetworkButtonEventClicked += value; }
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
            AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2);
        }

        public override void Start()
        {
            AttachUIEvents();
            //// TODO: remove
            //_networkListPanel.AddNetwork(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(4).First(), true);
            //_networkListPanel.AddNetwork(new ExtendedNetInfo(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(13).First()));
            //_networkListPanel.AddNetwork(new ExtendedNetInfo(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(25).First()));
            //_networkListPanel.AddNetwork(new ExtendedNetInfo(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(72).First()));
            //_networkListPanel.AddNetwork(new ExtendedNetInfo(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(155).First()));
            //_networkListPanel.AddNetwork(new ExtendedNetInfo(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(200).First()));

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

        public void OnGUI()
        {
            if (UIView.HasModalInput()
                || UIView.HasInputFocus()
                || !Singleton<ParallelRoadTool>.exists
                || !(ToolsModifierControl.toolController.CurrentTool is NetTool))
                return;

            var e = Event.current;

            if (e.isMouse)
            {
                // HACK - [ISSUE-84] Report if we're currently having a long mouse press
                Singleton<ParallelRoadTool>.instance.IsMouseLongPress = e.type switch
                {
                    EventType.MouseDown => true,
                    EventType.MouseUp => false,
                    _ => Singleton<ParallelRoadTool>.instance.IsMouseLongPress
                };

                Log._Debug($"[{nameof(UIMainWindow)}.{nameof(OnGUI)}] Setting {nameof(Singleton<ParallelRoadTool>.instance.IsMouseLongPress)} to {Singleton<ParallelRoadTool>.instance.IsMouseLongPress}");
            }

            // Checking key presses
            // if (OptionsKeymapping.ToggleParallelRoads.IsPressed(e)) ToggleToolCheckbox();

            if (OptionsKeymapping.DecreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(-1f);

            if (OptionsKeymapping.IncreaseHorizontalOffset.IsPressed(e)) AdjustNetOffset(1f);

            if (OptionsKeymapping.DecreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(-1f, false);

            if (OptionsKeymapping.IncreaseVerticalOffset.IsPressed(e)) AdjustNetOffset(1f, false);
        }

        public override void OnDestroy()
        {
            DetachUIEvents();
        }

        //private void ToggleToolCheckbox(bool forceClose = false)
        //{
        //    if (forceClose)
        //    {
        //        _toolToggleButton.isChecked = false;
        //        OnParallelToolToggled?.Invoke(_toolToggleButton, _toolToggleButton.isChecked);
        //    }
        //    else
        //    {
        //        _toolToggleButton.isChecked = !_toolToggleButton.isChecked;
        //        OnParallelToolToggled?.Invoke(_toolToggleButton, _toolToggleButton.isChecked);
        //    }
        //}

        private void AdjustNetOffset(float step, bool isHorizontal = true)
        {
            // Adjust all offsets on keypress
            if (isHorizontal)
                OnHorizontalOffsetKeypress?.Invoke(this, step);
            else
                OnVerticalOffsetKeypress?.Invoke(this, step);
        }

        #endregion

        #endregion

        #region Control

        public void UpdateCurrentNetwork(NetInfo netInfo)
        {
            _currentNetworkSetupPanel.Render(new NetInfoItem(netInfo));
        }

        public void AddNetwork(NetInfoItem netInfo)
        {
            _networkListPanel.AddNetwork(netInfo);
        }

        #endregion
    }
}
