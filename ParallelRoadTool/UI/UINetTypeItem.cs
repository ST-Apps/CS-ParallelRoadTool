using System;
using System.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UINetTypeItem : UIPanel
    {
        private const int TextFieldWidth = 60;
        private const int LabelWidth = 300;
        private const float ColumnPadding = 8f;

        public UILabel Label;
        protected UITextField TextField { get; private set; }
        protected UIButton DeleteButton { get; private set; }
        protected UIButton AddButton { get; private set; }

        public NetInfo NetInfo;
        public bool IsCurrentItem { get; set; } = false;
        public float HorizontalOffset;
        public int Index;

        public Action OnChangedCallback { private get; set; }
        public Action OnDeleteCallback { private get; set; }
        public Action OnAddCallback { private get; set; }

        public override void Start()
        {
            name = "PRT_NetTypeItem";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            color = new Color32(255, 255, 255, 255);
            size = new Vector2(450 - 8*2 - 4*2, 40);            

            TextField = UIUtil.CreateTextField(this);
            TextField.relativePosition = new Vector3(LabelWidth, 10);
            TextField.width = TextFieldWidth;

            TextField.eventTextSubmitted += TextField_eventTextSubmitted;

            Label = AddUIComponent<UILabel>();
            Label.textScale = 1f;
            Label.text = "Select a network";
            Label.autoSize = false;
            Label.width = LabelWidth;
            Label.relativePosition = new Vector3(10, 12);

            DeleteButton = UIUtil.CreateUiButton(this, string.Empty, "Remove network", new Vector2(36, 36), "Remove");
            DeleteButton.zOrder = 0;
            DeleteButton.textScale = 0.8f;
            DeleteButton.relativePosition = new Vector3(TextFieldWidth + LabelWidth + 3 * ColumnPadding, 0);

            DeleteButton.eventClicked += DeleteButton_eventClicked;

            AddButton = UIUtil.CreateUiButton(this, string.Empty, "Add network", new Vector2(36, 36), "Add");
            AddButton.zOrder = 1;
            AddButton.isVisible = false;
            AddButton.textScale = 0.8f;
            AddButton.relativePosition = new Vector3(TextFieldWidth + LabelWidth + 3 * ColumnPadding, 0);

            AddButton.eventClicked += AddButton_eventClicked;

            RenderItem();
        }        

        public void RenderItem()
        {
            DebugUtils.Log($"RenderItem {NetInfo} at {HorizontalOffset}");
            if (NetInfo != null)
                Label.text = NetInfo.GenerateBeautifiedNetName();

            TextField.text = HorizontalOffset.ToString(CultureInfo.InvariantCulture);

            if (!IsCurrentItem) return;

            DeleteButton.isVisible = false;
            TextField.isVisible = false;
            AddButton.isVisible = true;
            Label.text = $"Current: {Label.text}";
        }

        private void TextField_eventTextSubmitted(UIComponent component, string value)
        {
            if (!float.TryParse(value, out HorizontalOffset)) return;
            OnChangedCallback?.Invoke();
        }

        private void AddButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            DebugUtils.Log("UINetTypeItem.AddButton_eventClicked");
            OnAddCallback?.Invoke();
        }

        private void DeleteButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            DebugUtils.Log("UINetTypeItem.DeleteButton_eventClicked");
            OnDeleteCallback?.Invoke();
        }

    }
}
