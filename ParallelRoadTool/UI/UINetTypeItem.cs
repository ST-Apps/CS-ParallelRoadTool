using System;
using System.Globalization;
using System.Linq;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UINetTypeItem : UIPanel
    {
        private const int TextFieldWidth = 65;
        private const int LabelWidth = 250;
        private const float ColumnPadding = 8f;
        private const int ReverseButtonWidth = 36;
        public float HorizontalOffset;
        public int Index;

        public NetInfo NetInfo;
        public float VerticalOffset;
        public bool IsReversed;

        private UILabel Label { get; set; }
        private UITextField HorizontalOffsetField { get; set; }
        private UITextField VerticalOffsetField { get; set; }
        private UIButton DeleteButton { get; set; }
        private UIButton AddButton { get; set; }
        public UICheckBox ReverseCheckbox { get; set; }
        public UIDropDown DropDown { get; private set; }
        public bool IsCurrentItem { get; set; }

        private bool Populated { get; set; }

        public Action OnChangedCallback { get; set; }
        public Action OnDeleteCallback { private get; set; }
        public Action OnAddCallback { private get; set; }

        public override void Start()
        {
            name = "PRT_NetTypeItem";
            atlas = ResourceLoader.GetAtlas("Ingame");
            backgroundSprite = "SubcategoriesPanel";
            color = new Color32(255, 255, 255, 255);
            size = new Vector2(500 - 8 * 2 - 4 * 2, 40);

            var panel = AddUIComponent<UIPanel>();
            panel.size = new Vector2(LabelWidth, 40);
            panel.relativePosition = Vector2.zero;

            DropDown = UIUtil.CreateDropDown(panel);
            DropDown.width = LabelWidth;
            DropDown.relativePosition = Vector2.zero;
            DropDown.eventSelectedIndexChanged += DropDown_eventSelectedIndexChanged;

            ReverseCheckbox = UIUtil.CreateCheckBox(this, "Reverse", Locale.Get("PRT_TOOLTIPS", "ReverseToggleButton"), false);
            ReverseCheckbox.relativePosition = new Vector3(LabelWidth + ColumnPadding, 2);
            ReverseCheckbox.eventCheckChanged += ReverseCheckboxOnEventCheckChanged;

            HorizontalOffsetField = UIUtil.CreateTextField(this);
            HorizontalOffsetField.relativePosition =
                new Vector3(LabelWidth + 2 * ColumnPadding + ReverseButtonWidth, 10);
            HorizontalOffsetField.width = TextFieldWidth;
            HorizontalOffsetField.eventTextSubmitted += HorizontalOffsetField_eventTextSubmitted;

            VerticalOffsetField = UIUtil.CreateTextField(this);
            VerticalOffsetField.relativePosition =
                new Vector3(LabelWidth + 3 * ColumnPadding + ReverseButtonWidth + TextFieldWidth, 10);
            VerticalOffsetField.width = TextFieldWidth;
            VerticalOffsetField.eventTextSubmitted += VerticalOffsetField_eventTextSubmitted;

            Label = AddUIComponent<UILabel>();
            Label.textScale = .8f;
            Label.text = "Select a network";
            Label.autoSize = false;
            Label.width = LabelWidth;
            Label.relativePosition = new Vector3(10, 12);
            Label.isVisible = false;

            DeleteButton = UIUtil.CreateUiButton(this, string.Empty, Locale.Get("PRT_TOOLTIPS", "RemoveNetworkButton"), new Vector2(36, 36), "Remove");
            DeleteButton.zOrder = 0;
            DeleteButton.textScale = 0.8f;
            DeleteButton.relativePosition =
                new Vector3(2 * TextFieldWidth + LabelWidth + ReverseButtonWidth + 3 * ColumnPadding, 0);

            DeleteButton.eventClicked += DeleteButton_eventClicked;

            AddButton = UIUtil.CreateUiButton(this, string.Empty, Locale.Get("PRT_TOOLTIPS", "AddNetworkButton"), new Vector2(36, 36), "Add");
            AddButton.zOrder = 1;
            AddButton.isVisible = false;
            AddButton.textScale = 0.8f;
            AddButton.relativePosition =
                new Vector3(2 * TextFieldWidth + LabelWidth + ReverseButtonWidth + 3 * ColumnPadding, 0);

            AddButton.eventClicked += AddButton_eventClicked;

            RenderItem();
        }

        private void PopulateDropDown()
        {
            DropDown.items = (IsCurrentItem ? ParallelRoadTool.AvailableRoadNames.Take(1) : ParallelRoadTool.AvailableRoadNames)                
                .ToArray();
            DropDown.selectedIndex = 0;
            Populated = true;

            DebugUtils.Log($"UINetTypeItem.PopulateDropDown - Loaded {DropDown.items.Length} items in dropdown.");
        }

        public void RenderItem()
        {
            DebugUtils.Log($"RenderItem {NetInfo} at {HorizontalOffset}/{VerticalOffset}");

            if (!Populated) PopulateDropDown();

            if (!IsCurrentItem)
            {                
                HorizontalOffsetField.text = HorizontalOffset.ToString(CultureInfo.InvariantCulture);
                VerticalOffsetField.text = VerticalOffset.ToString(CultureInfo.InvariantCulture);
                ReverseCheckbox.isChecked = IsReversed;
                var index = ParallelRoadTool.AvailableRoadTypes.FindIndex(ni => ni != null && ni.name == NetInfo.name);
                DebugUtils.Log($"selecting index {index}");
                DropDown.selectedIndex = index;
                return;
            }

            DropDown.selectedIndex = 0;
            DeleteButton.isVisible = 
                HorizontalOffsetField.isVisible = 
                VerticalOffsetField.isVisible = 
                ReverseCheckbox.isVisible = 
                DropDown.isVisible = false;            
            Label.isVisible = AddButton.isVisible = true;
            Label.text = Locale.Get("PRT_TEXTS", "SameAsSelectedLabel");
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

        private void ReverseCheckboxOnEventCheckChanged(UIComponent component, bool value)
        {
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