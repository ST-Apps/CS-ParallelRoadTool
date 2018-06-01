using System;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UIOptionsPanel : UIPanel
    {

        #region Events

        public event PropertyChangedEventHandler<bool> OnToolToggled;

        #endregion

        public override void Start()
        {
            name = "PRT_OptionsPanel";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "GenericPanel";
            color = new Color32(206, 206, 206, 255);
            size = new Vector2(500 - 8 * 2, 36 + 2 * 8);

            padding = new RectOffset(8, 8, 8, 8);
            autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayout = true;
            autoSize = false;
            

            DebugUtils.Log($"UIOptionsPanel created {size} | {position}");
        }
    }
}