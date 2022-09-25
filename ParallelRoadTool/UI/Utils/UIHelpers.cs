using ColossalFramework.UI;
using UnityEngine;

namespace ParallelRoadTool.UI.Utils
{
    internal static class UIHelpers
    {
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
            var hue  = Mathf.Clamp(Mathf.Abs(hash % 360), 20, 340) / 360f;

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
    }
}
