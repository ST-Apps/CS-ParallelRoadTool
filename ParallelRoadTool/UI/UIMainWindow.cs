using ColossalFramework;
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
        #region Unity

        // We don't use padding for top side
        private readonly RectOffset LayoutPadding = new RectOffset(UIConstants.Padding, UIConstants.Padding, 0, UIConstants.Padding);

        public override void Start()
        {
            // Main
            name = $"{Configuration.ResourcePrefix}/MainWindow";
            isVisible = true;
            backgroundSprite = "SubcategoriesPanel";
            size = new Vector2(512, 256);
            maximumSize = size;
            absolutePosition = new Vector2(100, 100);
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding = LayoutPadding;

            // Main/Header
            var headerPanel = AddUIComponent<UIPanel>();
            headerPanel.name = $"{Configuration.ResourcePrefix}/MainWindow/Header";
            headerPanel.padding = padding;
            headerPanel.FitWidth(this, UIConstants.Padding);
            headerPanel.height = UIConstants.SmallBarHeight;

            // Main/Header/TitleLabel
            var titleLabel = headerPanel.AddUIComponent<UILabel>();
            titleLabel.name = $"{Configuration.ResourcePrefix}/MainWindow/Header/TitleLabel";
            titleLabel.text = ModInfo.ModName;
            titleLabel.AlignTo(headerPanel, UIAlignAnchor.TopLeft);
            titleLabel.anchor = UIAnchorStyle.CenterVertical;
            titleLabel.SendToBack();

            // Main/Header/CloseButton
            var closeButton = headerPanel.AddUIComponent<UIButton>();
            closeButton.name = $"{Configuration.ResourcePrefix}/MainWindow/Header/CloseButton";
            closeButton.text = "";
            closeButton.normalBgSprite = "buttonclose";
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.pressedBgSprite = "buttonclosepressed";
            closeButton.size = new Vector2(UIConstants.SmallButtonSize, UIConstants.SmallButtonSize);
            closeButton.anchor = UIAnchorStyle.CenterVertical;
            closeButton.AlignTo(headerPanel, UIAlignAnchor.TopRight);

            // Main/Header/DragHandle
            var dragHandle = headerPanel.AddUIComponent<UIDragHandle>();
            dragHandle.target = this;
            dragHandle.FitWidth(this, 0);
            dragHandle.height = headerPanel.height;

            // Main/Toolbar
            var toolbarPanel = AddUIComponent<UIPanel>();
            toolbarPanel.name = $"{Configuration.ResourcePrefix}/MainWindow/Toolbar";
            toolbarPanel.FitWidth(this, UIConstants.Padding);
            toolbarPanel.height = UIConstants.MiddleBarHeight;

            // Main/Toolbar/Tools
            var toolsPanel = toolbarPanel.AddUIComponent<UIPanel>();
            toolsPanel.name = $"{Configuration.ResourcePrefix}/MainWindow/Toolbar/Tools";
            toolsPanel.backgroundSprite = "GenericPanel";
            toolsPanel.color = new Color32(206, 206, 206, 255);
            toolsPanel.size = toolbarPanel.size;
            toolsPanel.width = (toolsPanel.width / 2) - (UIConstants.Padding / 2);
            toolsPanel.AlignTo(toolbarPanel, UIAlignAnchor.TopLeft);

            // Main/Toolbar/Options
            var optionsPanel = toolbarPanel.AddUIComponent<UIPanel>();
            optionsPanel.name = $"{Configuration.ResourcePrefix}/MainWindow/Toolbar/Options";
            optionsPanel.backgroundSprite = "GenericPanel";
            optionsPanel.color = new Color32(206, 206, 206, 255);
            optionsPanel.size = toolbarPanel.size;
            optionsPanel.width = (optionsPanel.width / 2) - (UIConstants.Padding / 2);
            optionsPanel.AlignTo(toolbarPanel, UIAlignAnchor.TopRight);
        }

        #endregion
    }
}
