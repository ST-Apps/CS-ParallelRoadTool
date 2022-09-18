using AlgernonCommons.UI;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Shared
{
    public abstract class UIWindow : StandalonePanel
    {
        #region Unity

        #region Components

        protected UIPanel Container;

        #endregion

        #region Lifecycle

        public UIWindow(string iconAtlasName)
        {
            var spriteAtlas = UITextures.LoadSingleSpriteAtlas(iconAtlasName);
            SetIcon(spriteAtlas, "normal");

            // Main/ContainerPanel
            Container = AddUIComponent<UIPanel>();
            Container.backgroundSprite = "GenericPanel";
            Container.autoLayoutDirection = LayoutDirection.Vertical;
            Container.autoLayoutPadding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);
            Container.autoLayout = true;
            Container.relativePosition = new Vector2(UIConstants.Padding, spriteAtlas["normal"].height + UIConstants.Padding);
            Container.FitWidth(this, UIConstants.Padding);
            Container.height = PanelHeight - spriteAtlas["normal"].height - 2 * UIConstants.Padding;
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (!Input.GetKey(KeyCode.Escape)) return;

            p.Use();
            Close();
        }

        #endregion

        #endregion
    }
}
