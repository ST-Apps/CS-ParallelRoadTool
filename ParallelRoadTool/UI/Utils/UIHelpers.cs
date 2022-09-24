using System.Linq;
using ColossalFramework.UI;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Utils
{
    /// <summary>
    ///     TODO: review!
    /// </summary>
    internal static class UIHelpers
    {
        #region Resources

        public static readonly UITextureAtlas Atlas = LoadResources();

        #endregion

        #region Utils

        private static UITextureAtlas LoadResources()
        {
            var textureAtlas =
                ResourceLoader.CreateTextureAtlas(Constants.CustomAtlasName, Constants.CustomSpritesNames,
                                                  Constants.IconsNamespace);

            var defaultAtlas = ResourceLoader.GetAtlas(Constants.DefaultAtlasName);
            var textures = Constants.DefaultSpritesNames.Select(t => defaultAtlas[t].texture).ToArray();
            ResourceLoader.AddTexturesInAtlas(textureAtlas, textures);

            return textureAtlas;
        }

        #endregion

        #region Helpers

        /// <summary>
        ///     Creates a <see cref="RectOffset" /> with the very same padding values for each side.
        /// </summary>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static RectOffset RectOffsetFromPadding(int padding)
        {
            return new RectOffset(padding, padding, padding, padding);
        }

        /// <summary>
        ///     Generates a random color starting from a string's hash.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Color ColorFromString(string str)
        {
            var hash = str.GetHashCode();
            var hue = Mathf.Clamp(Mathf.Abs(hash % 360), 20, 340) / 360f;

            return Color.HSVToRGB(hue, 0.5f, 0.85f);
        }

        /// <summary>
        ///     Fits component's width to its parent while keeping margin on both left and right size
        /// </summary>
        /// <param name="component"></param>
        /// <param name="parent"></param>
        /// <param name="margin"></param>
        internal static void FitWidth(this UIComponent component, UIComponent parent, int margin)
        {
            component.size = new Vector2(parent.size.x - 2 * margin, component.size.y);
        }

        /// <summary>
        ///     Fits component's height to its parent while keeping margin on both left and right size
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
        ///     Creates an <see cref="UIButton" /> as child of parent <see cref="UIComponent" />.
        /// TODO: replace this with UIButtons from Commons
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="text"></param>
        /// <param name="tooltip"></param>
        /// <param name="size"></param>
        /// <param name="sprite"></param>
        /// <param name="isTextButton"></param>
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
            if (size == Vector2.zero) size = new Vector2(UIConstants.MediumSize, UIConstants.MediumSize);
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


        #endregion
    }
}
