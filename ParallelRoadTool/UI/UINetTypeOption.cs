using NetworkSkins.UI;
using ParallelRoadTool;
using System.Linq;

namespace NetworkSkins.Meshes
{
    public class UINetTypeOption : UIDropDownOption
    {

        public NetInfo selectedNetInfo;

        protected override void Initialize()
        {
            Description = "Net Type";
            base.Initialize();
        }

        protected override bool PopulateDropDown()
        {
            DebugUtils.Log("UINetTypeOption.PopulateDropDown - START");

            DropDown.items = ParallelRoadTool.ParallelRoadTool.AvailableRoadTypes.Select(ni => ni.GenerateBeautifiedNetName()).ToArray();
            DropDown.selectedIndex = 1;

            DebugUtils.Log($"UINetTypeOption.PopulateDropDown - Loaded {DropDown.items.Length} items in dropdown.");

            return true;
        }

        protected override void OnSelectionChanged(int index)
        {
            selectedNetInfo = ParallelRoadTool.ParallelRoadTool.AvailableRoadTypes[index];
        }
    }
}
