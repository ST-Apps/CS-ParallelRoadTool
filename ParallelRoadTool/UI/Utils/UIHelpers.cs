using ColossalFramework.UI;
using ParallelRoadTool.Utils;
using System.Linq;
using UnityEngine;

namespace ParallelRoadTool.UI.Utils
{
    /// <summary>
    /// TODO: review!
    /// </summary>
    internal static class UIHelpers
    {

        #region Resources

        public static readonly UITextureAtlas Atlas = LoadResources();

        #endregion

        #region Helpers

        /// <summary>
        /// Fits component's width to its parent while keeping margin on both left and right size
        /// </summary>
        /// <param name="component"></param>
        /// <param name="parent"></param>
        /// <param name="margin"></param>
        internal static void FitWidth(this UIComponent component, UIComponent parent, int margin)
        {
            component.size = new Vector2(parent.size.x - 2 * margin, component.size.y);
        }

        /// <summary>
        /// Fits component's height to its parent while keeping margin on both left and right size
        /// </summary>
        /// <param name="component"></param>
        /// <param name="parent"></param>
        /// <param name="margin"></param>
        internal static void FitHeight(this UIComponent component, UIComponent parent, int margin)
        {
            component.size = new Vector2(component.size.x, parent.size.y - 2 * margin);
        }

        #endregion

        #region Components

        /// <summary>
        /// Creates an <see cref="UIButton"/> as child of parent <see cref="UIComponent"/>.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="text"></param>
        /// <param name="tooltip"></param>
        /// <param name="size"></param>
        /// <param name="sprite"></param>
        /// <param name="isTextButton"></param>
        /// <param name="atlas"></param>
        /// <returns></returns>
        internal static UIButton CreateUiButton(UIComponent parent,
                                              Vector2 size,
                                              string text,
                                              string tooltip,
                                              string sprite,
                                              bool isTextButton = false)
        {
            var uiButton = parent.AddUIComponent<UIButton>();
            uiButton.atlas = Atlas;
            uiButton.tooltip = tooltip;
            if (size == null)
            {
                size = new Vector2(UIConstants.MiddleButtonSize, UIConstants.MiddleButtonSize);
            }
            uiButton.size = size;

            if (!isTextButton)
            {
                uiButton.textHorizontalAlignment = UIHorizontalAlignment.Left;
                uiButton.normalFgSprite = sprite;
                uiButton.normalBgSprite = "OptionBase";
                uiButton.hoveredBgSprite = "OptionBaseHovered";
                uiButton.pressedBgSprite = "OptionBasePressed";
                uiButton.focusedBgSprite = "OptionBaseFocused";
                uiButton.disabledBgSprite = "OptionBaseDisabled";
            }
            else
            {
                uiButton.text = text;
                uiButton.atlas = Atlas;
                uiButton.textHorizontalAlignment = UIHorizontalAlignment.Center;
                if (sprite == string.Empty)
                    uiButton.normalFgSprite = "ButtonMenu";
                else
                    uiButton.normalFgSprite = sprite;
                uiButton.hoveredBgSprite = "ButtonMenuHovered";
                uiButton.pressedBgSprite = "ButtonMenuPressed";
                uiButton.focusedBgSprite = "ButtonMenuFocused";
                uiButton.disabledBgSprite = "ButtonMenuDisabled";
            }

            uiButton.textVerticalAlignment = UIVerticalAlignment.Middle;
            uiButton.horizontalAlignment = UIHorizontalAlignment.Right;
            uiButton.verticalAlignment = UIVerticalAlignment.Middle;

            return uiButton;
        }

        public static UICheckBox CreateCheckBox(UIComponent parent,
                                                string spriteName,
                                                string toolTip,
                                                Vector2 size,
                                                bool value,
                                                bool isStatic = false)
        {
            var checkBox = parent.AddUIComponent<UICheckBox>();
            if (size == null)
            {
                size = new Vector2(UIConstants.MiddleButtonSize, UIConstants.MiddleButtonSize);
            }
            checkBox.size = size;

            var button = checkBox.AddUIComponent<UIButton>();
            button.name = $"{Configuration.ResourcePrefix}{spriteName}";
            button.atlas = Atlas;
            button.tooltip = toolTip;
            button.relativePosition = new Vector2(0, 0);

            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.pressedBgSprite = "OptionBasePressed";
            button.disabledBgSprite = "OptionBaseDisabled";

            button.normalFgSprite = spriteName;
            if (!isStatic)
            {
                button.hoveredFgSprite = spriteName + "Hovered";
                button.pressedFgSprite = spriteName + "Pressed";
                button.disabledFgSprite = spriteName + "Disabled";
            }

            checkBox.isChecked = value;
            if (value)
            {
                button.normalBgSprite = "OptionBaseFocused";
                if (!isStatic)
                    button.normalFgSprite = spriteName + "Focused";
            }

            checkBox.eventCheckChanged += (c, s) =>
            {
                if (s)
                {
                    button.normalBgSprite = "OptionBaseFocused";
                    if (!isStatic)
                        button.normalFgSprite = spriteName + "Focused";
                }
                else
                {
                    button.normalBgSprite = "OptionBase";
                    if (!isStatic)
                        button.normalFgSprite = spriteName;
                }
            };

            return checkBox;
        }

        public static UITextField CreateTextField(UIComponent parent)
        {
            var textField = parent.AddUIComponent<UITextField>();

            textField.size = new Vector2(48f, 28f);
            textField.padding = new RectOffset(6, 6, 6, 6);
            textField.maxLength = 3;
            textField.builtinKeyNavigation = true;
            textField.isInteractive = true;
            textField.readOnly = false;
            textField.horizontalAlignment = UIHorizontalAlignment.Center;
            textField.selectionSprite = "EmptySprite";
            textField.selectionBackgroundColor = new Color32(0, 172, 234, 255);
            textField.normalBgSprite = "TextFieldPanel";
            textField.hoveredBgSprite = "TextFieldPanelHovered";
            textField.focusedBgSprite = "TextFieldPanelHovered";
            textField.textColor = new Color32(0, 0, 0, 255);
            textField.disabledTextColor = new Color32(0, 0, 0, 128);
            textField.color = new Color32(255, 255, 255, 255);
            textField.eventGotFocus += (component, param) => component.color = new Color32(253, 227, 144, 255);
            textField.eventLostFocus += (component, param) => component.color = new Color32(255, 255, 255, 255);
            return textField;
        }

        public static UISprite CreateUISprite(UIComponent parent, string spriteName)
        {
            var uiSprite = parent.AddUIComponent<UISprite>();
            uiSprite.size = new Vector2(24, 24);
            uiSprite.maximumSize = uiSprite.size;
            uiSprite.atlas = Atlas;
            uiSprite.spriteName = spriteName;

            return uiSprite;
        }

        public static UICheckBox CreateCheckbox(
            UIComponent parent,
            Vector2 size,
            string spriteName,
            string tooltip
        )
        {
            // Main container
            var checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.size = size;

            // Button
            var button = checkBox.AddUIComponent<UIButton>();            
            button.size = size;
            button.atlas = Atlas;
            button.tooltip = tooltip;
            button.relativePosition = new Vector2(0, 0);

            // Background sprites
            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.pressedBgSprite = "OptionBasePressed";
            button.disabledBgSprite = "OptionBaseDisabled";

            // Foreground sprites
            button.normalFgSprite = spriteName;
            button.focusedFgSprite = spriteName;
            button.hoveredFgSprite = spriteName + "Hovered";
            button.pressedFgSprite = spriteName + "Pressed";
            button.disabledFgSprite = spriteName + "Disabled";

            // Change sprites on selection change
            checkBox.eventCheckChanged += (c, v) =>
            {
                if (v)
                {
                    // Checkbox is selected
                    button.normalBgSprite = "OptionBaseFocused";
                    button.normalFgSprite = spriteName + "Pressed";
                    button.focusedFgSprite = spriteName + "Pressed";
                } else
                {
                    // Checkbox is NOT selected
                    button.normalBgSprite = "OptionBase";
                    button.normalFgSprite = spriteName;
                    button.focusedFgSprite = spriteName;

                }
            };

            return checkBox;
        }

        public static UICheckBox CreateCheckBox(UIComponent parent,
            Vector2 size,
                                                string spriteName,
                                                string toolTip,
                                                bool value,
                                                bool isStatic = false)
        {
            var checkBox = parent.AddUIComponent<UICheckBox>();
            checkBox.size = size;

            var button = checkBox.AddUIComponent<UIButton>();
            button.name = $"{Configuration.ResourcePrefix}{spriteName}";
            button.size = size;
            button.atlas = Atlas;
            button.tooltip = toolTip;
            button.relativePosition = new Vector2(0, 0);

            button.normalBgSprite = "OptionBase";
            button.hoveredBgSprite = "OptionBaseHovered";
            button.pressedBgSprite = "OptionBasePressed";
            button.disabledBgSprite = "OptionBaseDisabled";

            button.normalFgSprite = spriteName;
            button.focusedFgSprite = spriteName;

            if (!isStatic)
            {
                button.hoveredFgSprite = spriteName + "Hovered";
                button.pressedFgSprite = spriteName + "Pressed";
                button.disabledFgSprite = spriteName + "Disabled";
            }

            checkBox.isChecked = value;
            if (value)
            {
                button.normalBgSprite = "OptionBaseFocused";
                if (!isStatic)
                    button.normalFgSprite = spriteName + "Pressed";
            }

            checkBox.eventCheckChanged += (c, s) =>
            {
                if (s)
                {
                    button.normalBgSprite = "OptionBaseFocused";
                    if (!isStatic)
                        button.normalFgSprite = spriteName + "Pressed";
                }
                else
                {
                    button.normalBgSprite = "OptionBase";
                    if (!isStatic)
                        button.normalFgSprite = spriteName;
                }
            };

            return checkBox;
        }

        #endregion

        #region Utils

        private static UITextureAtlas LoadResources()
        {
            var textureAtlas =
                ResourceLoader.CreateTextureAtlas(Configuration.CustomAtlasName, Configuration.CustomSpritesNames,
                                                  Configuration.IconsNamespace);

            var defaultAtlas = ResourceLoader.GetAtlas(Configuration.DefaultAtlasName);
            var textures = Configuration.DefaultSpritesNames.Select(t => defaultAtlas[t].texture).ToArray();
            ResourceLoader.AddTexturesInAtlas(textureAtlas, textures);

            return textureAtlas;
        }

        #endregion
    }
}
