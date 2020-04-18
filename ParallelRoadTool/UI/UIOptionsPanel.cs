using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UIOptionsPanel : UIPanel
    {
        public override void Start()
        {
            name = $"{Configuration.ResourcePrefix}OptionsPanel";
            atlas = UIUtil.DefaultAtlas;
            backgroundSprite = "GenericPanel";
            color = new Color32(206, 206, 206, 255);
            size = new Vector2(500 - 8 * 2, 36 + 2 * 8);

            padding = new RectOffset(8, 8, 8, 8);
            autoLayoutPadding = new RectOffset(0, 4, 0, 0);
            autoLayoutDirection = LayoutDirection.Horizontal;
            autoLayoutStart = LayoutStart.TopRight;
            autoLayout = true;
            autoSize = false;

            Log._Debug($"[UIOptionsPanel.Start] Panel created with size {size} and position {position}");
        }
    }
}