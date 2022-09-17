﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.UI.Base;
using ParallelRoadTool.UI.Utils;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    /// <summary>
    /// This is the main toggle button for the mod.
    /// Its default position is just besides the upgrade road tool.
    /// It's also draggable using the right mouse button.
    /// </summary>
    public class UIToolToggleButton : UICheckBox
    {

        #region Fields

        /// <summary>
        /// Base name for all the sprites showing the PRT icon.
        /// </summary>
        private const string SpriteName = "Parallel";

        #endregion

        #region Settings

        private static readonly SavedInt SavedToggleX = new("toggleX", Configuration.SettingsFileName, -1000, true);

        private static readonly SavedInt SavedToggleY = new("toggleY", Configuration.SettingsFileName, -1000, true);

        #endregion

        #region Callbacks

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
            base.Awake();

            size = new Vector2(UIConstants.MiddleSize, UIConstants.MiddleSize);

            var toolTip = Locale.Get($"{Configuration.ResourcePrefix}TOOLTIPS", "ToolToggleButton");

            var button = AddUIComponent<UIButton>();
            button.name = $"{Configuration.ResourcePrefix}{SpriteName}";
            button.atlas = UIHelpers.Atlas;
            button.tooltip = toolTip;
            button.relativePosition = new Vector2(0, 0);

            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.pressedBgSprite = "OptionBasePressed";
            button.disabledBgSprite = "OptionBaseDisabled";

            button.normalFgSprite = SpriteName;
            button.hoveredFgSprite = SpriteName + "Hovered";
            button.pressedFgSprite = SpriteName + "Pressed";
            button.disabledFgSprite = SpriteName + "Disabled";

            isChecked = false;
            eventCheckChanged += (c, s) =>
            {
                if (s)
                {
                    button.normalBgSprite = "OptionBaseFocused";
                    button.normalFgSprite = SpriteName + "Focused";
                }
                else
                {
                    button.normalBgSprite = "OptionBase";
                    button.normalFgSprite = SpriteName;
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
            base.Start();

            AttachToEvents();

            // Restore saved position, if any, otherwise reset it to default
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

        #endregion

        #endregion

        #region Control

        #region Internals

        private void AttachToEvents()
        {
            _buttonDragHandle.eventDragEnd += ButtonDragHandle_eventDragEnd;
        }

        private void DetachFromEvents()
        {
            _buttonDragHandle.eventDragEnd -= ButtonDragHandle_eventDragEnd;
        }

        private void UpdateSavedPosition()
        {
            SavedToggleX.value = (int)absolutePosition.x;
            SavedToggleY.value = (int)absolutePosition.y;
        }

        #endregion

        #region Public APi

        /// <summary>
        /// Reset position by setting it as the latest button in tool mode <see cref="UITabstrip"/>, right after the upgrade tool.
        /// </summary>
        public void ResetPosition()
        {
            // We need to have both the following components to set our position
            var tsBar = UIUtil.FindComponent<UIComponent>("TSBar", null, UIUtil.FindOptions.NameContains);
            if (tsBar == null)
            {
                Log.Info(@$"[{nameof(UIToolToggleButton)}.{nameof(ResetPosition)}] Couldn't find ""TSBar"", aborting.");
                return;
            }

            var toolModeBar = UIUtil.FindComponent<UITabstrip>("ToolMode", tsBar, UIUtil.FindOptions.NameContains);
            if (toolModeBar == null)
            {
                Log.Info(@$"[{nameof(UIToolToggleButton)}.{nameof(ResetPosition)}] Couldn't find ""ToolMode"", aborting.");
                return;
            }

            // We can now set the absolute position at the right of the toolbar
            absolutePosition = new Vector3(toolModeBar.absolutePosition.x + toolModeBar.size.x + 1, toolModeBar.absolutePosition.y);

            // We also update the saved position
            UpdateSavedPosition();
        }

        #endregion

        #endregion
    }
}
