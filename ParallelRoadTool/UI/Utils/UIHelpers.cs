using ColossalFramework.UI;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Utils
{
    internal static class UIHelpers
    {

        #region Resources

        public static readonly UITextureAtlas DefaultAtlas = ResourceLoader.GetAtlas(Configuration.DefaultAtlasName);

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
                                              string text,
                                              string tooltip,
                                              Vector2 size,
                                              string sprite,
                                              bool isTextButton = false,
                                              UITextureAtlas atlas = null)
        {
            // Fallback to default atlas if none is defined
            if (atlas == null)
            {
                atlas = DefaultAtlas;
            }

            var uiButton = parent.AddUIComponent<UIButton>();
            uiButton.atlas = atlas;
            uiButton.tooltip = tooltip;
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
                uiButton.atlas = DefaultAtlas;
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

        #endregion
    }
}
