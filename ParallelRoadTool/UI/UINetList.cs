using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework.UI;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UINetList : UIPanel
    {
        public List<NetTypeItem> List;
        private List<UINetTypeItem> _items;

        private UINetTypeItem _currentTool;
        private UIPanel _space;

        public Action OnChangedCallback { private get; set; }        

        public override void Start()
        {
            name = "PRT_NetList";
            padding = new RectOffset(4, 4, 4, 0);
            size = new Vector2(450 - 8*2, 200);
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

                // get sum of current offsets
                float prevOffset = 0;
                if (List.Any())
                    prevOffset = List.Last().HorizontalOffset;

                var netInfo = PrefabCollection<NetInfo>.FindLoaded(_currentTool.NetInfo.name);

                DebugUtils.Log($"{_currentTool.NetInfo} halfWidth: {_currentTool.NetInfo.m_halfWidth}");

                var item = new NetTypeItem(netInfo, prevOffset + netInfo.m_halfWidth * 2);
                List.Add(item);

                RenderList();

                Changed();
            };

            _space = AddUIComponent<UIPanel>();
            _space.size = new Vector2(1, 1);
        }

        public void UpdateCurrrentTool(NetInfo tool)
        {
            _currentTool.NetInfo = tool;
            _currentTool.RenderItem();
        }

        public void Changed()
        {
            OnChangedCallback?.Invoke();
        }

        public void RenderList()
        {
            // Remove items
            foreach (var child in _items)
            {
                Destroy(child);
            }

            _items.Clear();

            // Add items
            var index = 0;
            foreach (var item in List)
            {
                DebugUtils.Log($"rendering item {index} {item.NetInfo} at {item.HorizontalOffset}");

                var comp = AddUIComponent<UINetTypeItem>();
                comp.NetInfo = item.NetInfo;
                comp.HorizontalOffset = item.HorizontalOffset;
                comp.Index = index++;
                //comp.RenderItem();

                comp.OnDeleteCallback = () =>
                {
                    // remove item from list
                    List.RemoveAt(comp.Index);
                    RenderList();
                    Changed();
                };

                comp.OnChangedCallback = () =>
                {
                    //item.HorizontalOffset = comp.HorizontalOffset;
                    var i = List[comp.Index];
                    i.HorizontalOffset = comp.HorizontalOffset;

                    DebugUtils.Message($"OnChangedCallback {comp.Index} now at {item.HorizontalOffset}");

                    Changed();
                };

                _items.Add(comp);
            }

            _space.BringToFront();
        }
    }
}
