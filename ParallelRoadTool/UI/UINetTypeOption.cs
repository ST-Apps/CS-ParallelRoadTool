using NetworkSkins.UI;
using ParallelRoadTool;
using System;
using System.Linq;

namespace NetworkSkins.Meshes
{
    public class UINetTypeOption : UIDropDownOption
    {
        public NetInfo selectedNetInfo;

        public Action SelectionChangedCallback { private get; set; }

        protected override void Initialize()
        {
            Description = "Net Type";
            base.Initialize();
        }

        protected override bool PopulateDropDown()
        {
            DropDown.items = ParallelRoadTool.ParallelRoadTool.AvailableRoadTypes.Select(ni => ni.GenerateBeautifiedNetName()).ToArray();
            DropDown.selectedIndex = 0;

            DebugUtils.Log($"UINetTypeOption.PopulateDropDown - Loaded {DropDown.items.Length} items in dropdown.");

            return true;
        }

        protected override void OnSelectionChanged(int index)
        {            
            selectedNetInfo = ParallelRoadTool.ParallelRoadTool.AvailableRoadTypes[index];

            DebugUtils.Log($"UINetTypeOption.OnSelectionChanged - Selected net info {selectedNetInfo.name}");

            SelectionChangedCallback?.Invoke();
        }
    }
}
