using System;
using System.Linq;
using ParallelRoadTool.UI.Base;

namespace ParallelRoadTool.UI
{
    public class UINetTypeOption : UIDropDownTextFieldOption
    {
        public float Offset = 15f;
        
        public Action OnChangedCallback { private get; set; }
        public Action OnDeleteButtonCallback { private get; set; }

        public NetInfo SelectedNetInfo { get; private set; }
        public bool HideDeleteButton
        {
            set => DeleteButton.enabled = value;
        }

        protected override void Initialize()
        {
            Description = "Net Type";
            base.Initialize();
        }

        protected override bool PopulateDropDown()
        {
            return false;
            /*DropDown.items = ParallelRoadTool.AvailableRoadTypes
                .Select(ni => ni.GenerateBeautifiedNetName()).ToArray();
            DropDown.selectedIndex = 0;

            TextField.text = $"{HorizontalOffset}";

            DebugUtils.Log($"UINetTypeOption.PopulateDropDown - Loaded {DropDown.items.Length} items in dropdown.");

            return true;
            */
        }

        protected override void OnSelectionChanged(int index)
        {
            //SelectedNetInfo = ParallelRoadTool.AvailableRoadTypes[index];

            DebugUtils.Log($"UINetTypeOption.OnSelectionChanged - Selected net info {SelectedNetInfo.name}");

            OnChangedCallback?.Invoke();
        }

        protected override void OnTextChanged(string value)
        {            
            if (!float.TryParse(value, out Offset)) return;
            DebugUtils.Log($"UINetTypeOption.OnTextChanged - Selected HorizontalOffset {Offset}");

            OnChangedCallback?.Invoke();
        }

        protected override void OnDeleteButtonClicked()
        {
            DebugUtils.Log("UINetTypeOption.OnDeleteButtonClicked");

            OnDeleteButtonCallback?.Invoke();
        }
    }
}