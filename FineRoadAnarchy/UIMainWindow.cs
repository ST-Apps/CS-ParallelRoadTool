using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

namespace FineRoadAnarchy
{
    public class UIMainWindow : UIPanel
    {
        public static readonly SavedInt savedWindowX = new SavedInt("windowX", FineRoadAnarchy.settingsFileName, -1000, true);
        public static readonly SavedInt savedWindowY = new SavedInt("windowY", FineRoadAnarchy.settingsFileName, -1000, true);

        public override void Start()
        {
            name = "FRA_MainWindow";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            isVisible = false;

            UIDragHandle dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.target = parent;
            dragHandle.relativePosition = Vector3.zero;
            dragHandle.size = size;
            dragHandle.SendToBack();

            // Control panel
            UILabel label = AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.text = "Fine Road Anarchy";
            label.relativePosition = new Vector2(8, 8);
            label.SendToBack();

            absolutePosition = new Vector3(savedWindowX.value , savedWindowY.value);

            DebugUtils.Log("UIMainWindow created");
        }

        public override void Update()
        {
            isVisible = FineRoadAnarchy.instance.m_netTool.enabled;

            base.Update();
        }

        protected override void OnPositionChanged()
        {
            Vector2 resolution = GetUIView().GetScreenResolution();

            if (absolutePosition.x == -1000)
            {
                absolutePosition = new Vector2((resolution.x - width) / 2, (resolution.y - height) / 2);
            }

            absolutePosition = new Vector2(
                (int)Mathf.Clamp(absolutePosition.x, 0, resolution.x - width),
                (int)Mathf.Clamp(absolutePosition.y, 0, resolution.y - height));

            savedWindowX.value = (int)absolutePosition.x;
            savedWindowY.value = (int)absolutePosition.y;

            base.OnPositionChanged();
        }
    }
}
