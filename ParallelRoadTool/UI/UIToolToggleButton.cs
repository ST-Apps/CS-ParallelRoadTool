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

        #region Events

        private void ButtonDragHandle_eventDragEnd(UIComponent component, UIDragEventParameter eventParam)
        {
            UpdateSavedPosition();
        }

        #endregion

        #region Unity

        #region Components

        private UIRightDragHandle _buttonDragHandle;

        #endregion

        #region Lifecycle

        public override void Awake()
        {
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

            // HACK - [ISSUE-26] Tool's main button must be draggable to prevent overlapping other mods buttons.
            _buttonDragHandle = AddUIComponent<UIRightDragHandle>();
            _buttonDragHandle.size = size;
            _buttonDragHandle.relativePosition = Vector3.zero;
            _buttonDragHandle.target = this;
        }

        public override void Start()
        {
            AttachToEvents();

            if (SavedToggleX.value != -1000 && SavedToggleY.value != -1000)
                absolutePosition = new Vector3(SavedToggleX.value, SavedToggleY.value);
            else
                ResetPosition();
        }

        public override void OnDestroy()
        {
            DetachFromEvents();

            base.OnDestroy();
        }

        private void AttachToEvents()
        {
            _buttonDragHandle.eventDragEnd += ButtonDragHandle_eventDragEnd;
        }

        private void DetachFromEvents()
        {
            _buttonDragHandle.eventDragEnd -= ButtonDragHandle_eventDragEnd;
        }

        #endregion

        #endregion

        private void UpdateSavedPosition()
        {
            SavedToggleX.value = (int)absolutePosition.x;
            SavedToggleY.value = (int)absolutePosition.y;
        }

        public void ResetPosition()
        {
            var tsBar = UIUtil.FindComponent<UIComponent>("TSBar", null, UIUtil.FindOptions.NameContains);
            var toolModeBar = UIUtil.FindComponent<UITabstrip>("ToolMode", tsBar, UIUtil.FindOptions.NameContains);

            absolutePosition = new Vector3(toolModeBar.absolutePosition.x + toolModeBar.size.x + 1, toolModeBar.absolutePosition.y);
            UpdateSavedPosition();
        }

    }
}
