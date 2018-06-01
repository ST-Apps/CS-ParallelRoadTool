using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Base;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UINetList : UIPanel
    {
        private UINetTypeItem _currentTool;
        private List<UINetTypeItem> _items;
        private UIPanel _space;
        public List<NetTypeItem> List;

        public Action OnChangedCallback { private get; set; }

        public override void Start()
        {
            name = "PRT_NetList";
            padding = new RectOffset(4, 4, 4, 0);
            size = new Vector2(500 - 8 * 2, 200);
            autoLayoutPadding = new RectOffset(0, 0, 0, 4);
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = "GenericPanel";
            color = Color.black;

            _items = new List<UINetTypeItem>();

            _currentTool = AddUIComponent<UINetTypeItem>();
            _currentTool.IsCurrentItem = true;
            _currentTool.OnAddCallback = () =>
            {
                DebugUtils.Log("Adding item to list");

                // get offset of previuous item
                float prevOffset = 0;
                if (List.Any())
                    prevOffset = List.Last().HorizontalOffset;

                var netInfo = _currentTool.DropDown.selectedIndex == 0
                    ? PrefabCollection<NetInfo>.FindLoaded(_currentTool.NetInfo.name)
                    : ParallelRoadTool.AvailableRoadTypes[_currentTool.DropDown.selectedIndex];

                DebugUtils.Log($"{_currentTool.NetInfo} halfWidth: {_currentTool.NetInfo.m_halfWidth}");

                var item = new NetTypeItem(netInfo, prevOffset + netInfo.m_halfWidth * 2, 0, false);
                List.Add(item);

                RenderList();

                Changed();
            };

            _space = AddUIComponent<UIPanel>();
            _space.size = new Vector2(1, 1);
        }

        public void UpdateCurrentTool(NetInfo tool)
        {
            DebugUtils.Log($"Selected a new network: {tool.name}");
            _currentTool.NetInfo = tool;
            _currentTool.RenderItem();

            // This one's required to update all the items that are set to "same as selected road" when the selected road changes
            foreach (var netTypeItem in _items)
            {
                DebugUtils.Log($"Updating dropdown NetInfo from {netTypeItem.NetInfo.name} to {tool.name}");
                netTypeItem.OnChangedCallback();
            }
        }

        private void Changed()
        {
            OnChangedCallback?.Invoke();
        }

        internal void RenderList()
        {
            // Remove items
            foreach (var child in _items) Destroy(child);

            _items.Clear();

            // Add items
            var index = 0;
            foreach (var item in List)
            {
                DebugUtils.Log($"rendering item {index} {item.NetInfo} at {item.HorizontalOffset}");

                var comp = AddUIComponent<UINetTypeItem>();
                comp.NetInfo = item.NetInfo;
                comp.HorizontalOffset = item.HorizontalOffset;
                comp.VerticalOffset = item.VerticalOffset;
                comp.Index = index++;

                comp.OnDeleteCallback = () =>
                {
                    // remove item from list
                    List.RemoveAt(comp.Index);
                    RenderList();
                    Changed();
                };

                comp.OnChangedCallback = () =>
                {
                    var i = List[comp.Index];
                    i.HorizontalOffset = comp.HorizontalOffset;
                    i.VerticalOffset = comp.VerticalOffset;
                    i.NetInfo = comp.DropDown.selectedIndex == 0
                        ? PrefabCollection<NetInfo>.FindLoaded(_currentTool.NetInfo.name)
                        : ParallelRoadTool.AvailableRoadTypes[comp.DropDown.selectedIndex];
                    i.IsReversed = comp.ReverseCheckbox.isChecked;

                    DebugUtils.Message(
                        $"OnChangedCallback item #{comp.Index}, net={i.NetInfo.GenerateBeautifiedNetName()}, HorizontalOffset={i.HorizontalOffset}, VerticalOffset={i.VerticalOffset}, IsReversed={i.IsReversed}");

                    Changed();
                };

                _items.Add(comp);
            }

            _space.BringToFront();
        }
    }
}