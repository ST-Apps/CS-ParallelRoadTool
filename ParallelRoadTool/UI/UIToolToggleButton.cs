using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using ParallelRoadTool.UI.Utils;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UIToolToggleButton : UICheckBox
    {

        #region Settings

        private static readonly SavedInt SavedToggleX =
            new SavedInt("toggleX", Configuration.SettingsFileName, -1000, true);

        private static readonly SavedInt SavedToggleY =
            new SavedInt("toggleY", Configuration.SettingsFileName, -1000, true);

        #endregion

        #region Unity

        #region Components

        private UIRightDragHandle _buttonDragHandle;

        #endregion

        #region Lifecycle

        public override void Awake()
        {
            var tsBar = UIUtil.FindComponent<UIComponent>("TSBar", null, UIUtil.FindOptions.NameContains);
            if (tsBar == null || !tsBar.gameObject.activeInHierarchy) return;

            var toolModeBar = UIUtil.FindComponent<UITabstrip>("ToolMode", tsBar, UIUtil.FindOptions.NameContains);
            if (toolModeBar == null) return;

            var spriteName = "Parallel";
            var toolTip = Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ToolToggleButton");
            size = new Vector2(36, 36);

            var button = AddUIComponent<UIButton>();
            button.name = $"{Configuration.ResourcePrefix}{spriteName}";
            button.atlas = UIHelpers.Atlas;
            button.tooltip = toolTip;
            button.relativePosition = new Vector2(0, 0);

            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.pressedBgSprite = "OptionBasePressed";
            button.disabledBgSprite = "OptionBaseDisabled";

            button.normalFgSprite = spriteName;
            button.hoveredFgSprite = spriteName + "Hovered";
            button.pressedFgSprite = spriteName + "Pressed";
            button.disabledFgSprite = spriteName + "Disabled";

            isChecked = false;
            eventCheckChanged += (c, s) =>
            {
                if (s)
                {
                    button.normalBgSprite = "OptionBaseFocused";
                    button.normalFgSprite = spriteName + "Focused";
                }
                else
                {
                    button.normalBgSprite = "OptionBase";
                    button.normalFgSprite = spriteName;
                }
            };

            if (SavedToggleX.value != -1000 && SavedToggleY.value != -1000)
                absolutePosition = new Vector3(SavedToggleX.value, SavedToggleY.value);
            else
                absolutePosition =
                    new Vector3(toolModeBar.absolutePosition.x + toolModeBar.size.x + 1,
                                toolModeBar.absolutePosition.y);

            // HACK - [ISSUE-26] Tool's main button must be draggable to prevent overlapping other mods buttons.
            _buttonDragHandle = AddUIComponent<UIRightDragHandle>();
            _buttonDragHandle.size = size;
            _buttonDragHandle.relativePosition = Vector3.zero;
            _buttonDragHandle.target = this;
        }

        #endregion

        #endregion

        public void ResetPosition()
        {
            var tsBar = UIUtil.FindComponent<UIComponent>("TSBar", null, UIUtil.FindOptions.NameContains);
            var toolModeBar = UIUtil.FindComponent<UITabstrip>("ToolMode", tsBar, UIUtil.FindOptions.NameContains);

            absolutePosition = new Vector3(toolModeBar.absolutePosition.x + toolModeBar.size.x + 1, toolModeBar.absolutePosition.y);
        }

    }
}
