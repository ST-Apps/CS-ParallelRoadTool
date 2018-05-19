using NetworkSkins.UI;
using ParallelRoadTool;
using System;
using System.Linq;

namespace NetworkSkins.Meshes
{
    public class UINetTypeOption : UIDropDownTextFieldOption
    {
        public NetInfo selectedNetInfo;
        public float offset = 15f;

        public Action OnChangedCallback { private get; set; }

        protected override void Initialize()
        {
            Description = "Net Type";            
            base.Initialize();
        }

        protected override bool PopulateDropDown()
        {
            DropDown.items = ParallelRoadTool.ParallelRoadTool.AvailableRoadTypes.Select(ni => ni.GenerateBeautifiedNetName()).ToArray();
            DropDown.selectedIndex = 0;

            TextField.text = $"{offset}";

            DebugUtils.Log($"UINetTypeOption.PopulateDropDown - Loaded {DropDown.items.Length} items in dropdown.");

            return true;
        }

        protected override void OnSelectionChanged(int index)
        {
            selectedNetInfo = ParallelRoadTool.ParallelRoadTool.AvailableRoadTypes[index];

            DebugUtils.Log($"UINetTypeOption.OnSelectionChanged - Selected net info {selectedNetInfo.name}");

            OnChangedCallback?.Invoke();
        }

        protected override void OnTextChanged(string value)
        {
            if (float.TryParse(value, out offset))
            {
                DebugUtils.Log($"UINetTypeOption.OnTextChanged - Selected offset {offset}");

                OnChangedCallback?.Invoke();
            }
        }
    }
}
