using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.UI;
using ParallelRoadTool.UI.Utils;
using ParallelRoadTool.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace ParallelRoadTool.UI_NEW
{
    public class UIMainWindow : UIPanel
    {
        #region Constants

        // We don't use padding for top side
        private readonly RectOffset LayoutPadding = new RectOffset(UIConstants.Padding, UIConstants.Padding, 0, UIConstants.Padding);

        #endregion

        #region Unity

        #region Components

        private UINetworkListPanel networkListPanel;

        #endregion

        public override void Awake()
        {
            // Main
            name = $"{Configuration.ResourcePrefix}MainWindow";
            isVisible = true;
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(512, 256);
            absolutePosition = new Vector2(100, 100);
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding = LayoutPadding;

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
            var closeButton = headerPanel.AddUIComponent<UIButton>();
            closeButton.name = $"{headerPanel.name}_CloseButton";
            closeButton.text = "";
            closeButton.normalBgSprite = "buttonclose";
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.pressedBgSprite = "buttonclosepressed";
            closeButton.size = new Vector2(UIConstants.SmallButtonSize, UIConstants.SmallButtonSize);
            closeButton.anchor = UIAnchorStyle.CenterVertical;
            closeButton.AlignTo(headerPanel, UIAlignAnchor.TopRight);

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
            var toggleSnappingButton = UIHelpers.CreateCheckBox(
                toolsPanel,
                "Snapping",
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "SnappingToggleButton"),
                new Vector2(UIConstants.MiddleButtonSize, UIConstants.MiddleButtonSize),
                false
            );
            toggleSnappingButton.name = $"{toolsPanel.name}_ToggleSnapping";

            // Main/Toolbar/Tools/AddNetworkButton
            var addNetworkButton = UIHelpers.CreateUiButton(
                toolsPanel,
                new Vector2(UIConstants.MiddleButtonSize, UIConstants.MiddleButtonSize),
                string.Empty,
                Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "AddNetworkButton"),
                "Add"
            );
            addNetworkButton.name = $"{toolsPanel.name}_AddNetwork";

            // Main/NetworkList
            networkListPanel = AddUIComponent<UINetworkListPanel>();
            networkListPanel.padding = padding;
            networkListPanel.FitWidth(this, UIConstants.Padding);

            // Main/Spacer
            AddUIComponent<UIPanel>().size = new Vector2(1, UIConstants.Padding / 2);
        }

        public override void Start()
        {
            // TMP
            networkListPanel.AddNetwork(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(4).First(), true);
            networkListPanel.AddNetwork(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(2).First());
            networkListPanel.AddNetwork(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(22).First());
            networkListPanel.AddNetwork(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(11).First());
            networkListPanel.AddNetwork(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(83).First());
            networkListPanel.AddNetwork(Singleton<ParallelRoadTool>.instance.AvailableRoadTypes.Skip(42).First());

            //// Before returning we can fit based on children's height and add back some padding
            //FitChildrenVertically();
            //height += UIConstants.Padding;
        }

        #endregion
    }
}
