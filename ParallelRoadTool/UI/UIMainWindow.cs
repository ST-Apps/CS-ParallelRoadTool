using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UIMainWindow : UIPanel
    {
        public static readonly SavedInt savedWindowX =
            new SavedInt("windowX", ParallelRoadTool.SettingsFileName, -1000, true);

        public static readonly SavedInt savedWindowY =
            new SavedInt("windowY", ParallelRoadTool.SettingsFileName, -1000, true);

        public override void Start()
        {
            name = "PRT_MainWindow";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            isVisible = false;
            size = new Vector2(450, 280);
            padding = new RectOffset(8, 8, 8, 8);
            autoLayoutPadding = new RectOffset(0, 0, 0, 4);

            var label = AddUIComponent<UILabel>();
            label.name = "PRT_TitleLabel";
            label.textScale = 0.9f;
            label.text = "Parallel Road Tool 7";
            label.autoSize = false;
            label.width = 450;
            label.SendToBack();

            var dragHandle = label.AddUIComponent<UIDragHandle>();
            dragHandle.target = this;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.size = label.size;
 
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;

            absolutePosition = new Vector3(savedWindowX.value, savedWindowY.value);

            OnPositionChanged();
            DebugUtils.Log($"UIMainWindow created {size} | {position}");
        }

        public override void Update()
        {
            if (ParallelRoadTool.Instance != null)
                isVisible = ParallelRoadTool.Instance.IsToolActive();
       
            base.Update();
        }
 
        protected override void OnPositionChanged()
        {            
            var resolution = GetUIView().GetScreenResolution();            

            if (absolutePosition.x == -1000)
                absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);

            absolutePosition = new Vector2(
                (int) Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                (int) Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            DebugUtils.Log($"UIMainWindow OnPositionChanged | {resolution} | {absolutePosition}");

            savedWindowX.value = (int) absolutePosition.x;
            savedWindowY.value = (int) absolutePosition.y;
        }
 
    }
}