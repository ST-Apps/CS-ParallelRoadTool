using System;
using System.Globalization;
using System.Linq;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UINetTypeItem : UIPanel
    {
        private const int TextFieldWidth = 50;
        private const int LabelWidth = 230;
        private const float ColumnPadding = 8f;
        private const int ReverseButtonWidth = 36;

        public UILabel Label;
        protected UITextField HorizontalOffsetField { get; private set; }
        protected UITextField VerticalOffsetField { get; private set; }
        protected UIButton DeleteButton { get; private set; }
        protected UIButton AddButton { get; private set; }
        public UICheckBox ReverseCheckbox;
        public UIDropDown DropDown { get; private set; }

        public NetInfo NetInfo;
        public bool IsCurrentItem { get; set; } = false;
        public float HorizontalOffset;
        public float VerticalOffset;
        public int Index;

        public bool populated { get; set; } = false;

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


            DropDown = UIUtil.CreateDropDown(this);
            DropDown.width = LabelWidth;
            DropDown.relativePosition = new Vector3(0, 0);
            DropDown.eventSelectedIndexChanged += DropDown_eventSelectedIndexChanged;

            ReverseCheckbox = UIUtil.CreateCheckBox(this, "Reverse", "Toggle reverse road", false);
            ReverseCheckbox.relativePosition = new Vector3(LabelWidth + ColumnPadding, 2);

            HorizontalOffsetField = UIUtil.CreateTextField(this);
            HorizontalOffsetField.relativePosition = new Vector3(LabelWidth + 2 * ColumnPadding + ReverseButtonWidth, 10);
            HorizontalOffsetField.width = TextFieldWidth;
            HorizontalOffsetField.eventTextSubmitted += HorizontalOffsetField_eventTextSubmitted;

            VerticalOffsetField = UIUtil.CreateTextField(this);
            VerticalOffsetField.relativePosition = new Vector3(LabelWidth + 3*ColumnPadding + ReverseButtonWidth + TextFieldWidth, 10);
            VerticalOffsetField.width = TextFieldWidth;
            VerticalOffsetField.eventTextSubmitted += VerticalOffsetField_eventTextSubmitted;


            Label = AddUIComponent<UILabel>();
            Label.textScale = .8f;
            Label.text = "Select a network";
            Label.autoSize = false;
            Label.width = LabelWidth;
            Label.relativePosition = new Vector3(10, 12);
            Label.isVisible = false;

            DeleteButton = UIUtil.CreateUiButton(this, string.Empty, "Remove network", new Vector2(36, 36), "Remove");
            DeleteButton.zOrder = 0;
            DeleteButton.textScale = 0.8f;
            DeleteButton.relativePosition = new Vector3(2 * TextFieldWidth + LabelWidth + ReverseButtonWidth + 3 * ColumnPadding, 0);

            DeleteButton.eventClicked += DeleteButton_eventClicked;

            AddButton = UIUtil.CreateUiButton(this, string.Empty, "Add network", new Vector2(36, 36), "Add");
            AddButton.zOrder = 1;
            AddButton.isVisible = false;
            AddButton.textScale = 0.8f;
            AddButton.relativePosition = new Vector3(2*TextFieldWidth + LabelWidth + ReverseButtonWidth + 3 * ColumnPadding, 0);

            AddButton.eventClicked += AddButton_eventClicked;

            RenderItem();
        }

        public void PopulateDropDown()
        {
            DropDown.items = ParallelRoadTool.AvailableRoadTypes
                .Select(ni => ni.GenerateBeautifiedNetName()).ToArray();
            DropDown.selectedIndex = 0;
            populated = true;

            DebugUtils.Log($"UINetTypeItem.PopulateDropDown - Loaded {DropDown.items.Length} items in dropdown.");
        }

        public void RenderItem()
        {
            DebugUtils.Log($"RenderItem {NetInfo} at {HorizontalOffset}/{VerticalOffset}");
            if (NetInfo != null)
                Label.text = NetInfo.GenerateBeautifiedNetName();

            if (!populated) PopulateDropDown();

            HorizontalOffsetField.text = HorizontalOffset.ToString(CultureInfo.InvariantCulture);
            VerticalOffsetField.text = VerticalOffset.ToString(CultureInfo.InvariantCulture);
            if (!IsCurrentItem)
            {
                int index = ParallelRoadTool.AvailableRoadTypes.FindIndex(ni => ni != null && ni.name == NetInfo.name);
                DebugUtils.Log($"selecting index {index}");
                DropDown.selectedIndex = index;
            }

            if (!IsCurrentItem) return;

            DeleteButton.isVisible = false;
            HorizontalOffsetField.isVisible = false;
            VerticalOffsetField.isVisible = false;
            ReverseCheckbox.isVisible = false;
            DropDown.isVisible = false;
            Label.isVisible = true;
            AddButton.isVisible = true;
            //Label.text = $"Current: {Label.text}";
            Label.text = "Same as selected";
        }

        private void DropDown_eventSelectedIndexChanged(UIComponent component, int index)
        {
            DebugUtils.Log("UINetTypeItem.DropDown_eventChanged");
            OnChangedCallback?.Invoke();
        }

        private void HorizontalOffsetField_eventTextSubmitted(UIComponent component, string value)
        {
            if (!float.TryParse(value, out HorizontalOffset)) return;
            OnChangedCallback?.Invoke();
        }

        private void VerticalOffsetField_eventTextSubmitted(UIComponent component, string value)
        {
            if (!float.TryParse(value, out VerticalOffset)) return;
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
