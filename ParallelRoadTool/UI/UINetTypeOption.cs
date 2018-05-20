using System;
using System.Linq;
using ParallelRoadTool.UI.Base;

namespace ParallelRoadTool.UI
{
    public class UINetTypeOption : UIDropDownTextFieldOption
    {
        public float Offset = 15f;
        public NetInfo SelectedNetInfo;

        public Action OnChangedCallback { private get; set; }

        protected override void Initialize()
        {
            Description = "Net Type";
            base.Initialize();
        }

        protected override bool PopulateDropDown()
        {
            DropDown.items = ParallelRoadTool.AvailableRoadTypes
                .Select(ni => ni.GenerateBeautifiedNetName()).ToArray();
            DropDown.selectedIndex = 0;

            TextField.text = $"{Offset}";

            DebugUtils.Log($"UINetTypeOption.PopulateDropDown - Loaded {DropDown.items.Length} items in dropdown.");

            return true;
        }

        protected override void OnSelectionChanged(int index)
        {
            SelectedNetInfo = ParallelRoadTool.AvailableRoadTypes[index];

            DebugUtils.Log($"UINetTypeOption.OnSelectionChanged - Selected net info {SelectedNetInfo.name}");

            OnChangedCallback?.Invoke();
        }

        protected override void OnTextChanged(string value)
        {            
            if (!float.TryParse(value, out Offset)) return;
            DebugUtils.Log($"UINetTypeOption.OnTextChanged - Selected offset {Offset}");

            OnChangedCallback?.Invoke();
        }
    }
}