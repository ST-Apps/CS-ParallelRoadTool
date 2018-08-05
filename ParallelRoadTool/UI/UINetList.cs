using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ColossalFramework.UI;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Base;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UINetList : UIPanel
    {
        private UINetTypeItem _currentTool;
        private List<UINetTypeItem> _items;
        private UIPanel _space;

        public Action OnChangedCallback { private get; set; }

        #region Events/Callbacks

        public event PropertyChangedEventHandler<NetTypeItemEventArgs> OnItemChanged;
        public event EventHandler OnItemAdded;
        public event PropertyChangedEventHandler<int> OnItemDeleted;

        #endregion

        #region Handlers

        private void UnsubscribeToUiEvents()
        {
            foreach (var uiNetTypeItem in _items)
            {
                uiNetTypeItem.OnChanged -= UiNetTypeItemOnOnChanged;                
                uiNetTypeItem.OnDeleteClicked -= UiNetTypeItemOnOnDeleteClicked;

                if (uiNetTypeItem.IsCurrentItem)
                    uiNetTypeItem.OnAddClicked -= UiNetTypeItemOnOnAddClicked;
            }
        }

        private void UiNetTypeItemOnOnChanged(UIComponent component, NetTypeItemEventArgs value)
        {            
            OnItemChanged?.Invoke(this, value);
        }

        private void UiNetTypeItemOnOnDeleteClicked(UIComponent component, int index)
        {
            OnItemDeleted?.Invoke(this, index);
            //List.RemoveAt(index);
            //RenderList();
            //Changed();
        }

        private void UiNetTypeItemOnOnAddClicked(object sender, EventArgs eventArgs)
        {
            DebugUtils.Log($"{nameof(UiNetTypeItemOnOnAddClicked)}");
            OnItemAdded?.Invoke(this, null);
        }

        #endregion

        #region Unity

        public override void Start()
        {
            name = $"{Configuration.ResourcePrefix}NetList";
            padding = new RectOffset(4, 4, 4, 0);
            size = new Vector2(500 - 8 * 2, 200);
            autoLayoutPadding = new RectOffset(0, 0, 0, 4);
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutDirection = LayoutDirection.Vertical;
            backgroundSprite = "GenericPanel";
            color = Color.black;

            _space = AddUIComponent<UIPanel>();
            _space.size = new Vector2(1, 1);

            _items = new List<UINetTypeItem>();
            AddItem(null, true);
            _currentTool = _items[0];                    
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            UnsubscribeToUiEvents();
        }

        #endregion

        //public void UpdateCurrentTool(NetInfo tool)
        //{
        //    DebugUtils.Log($"Selected a new network: {tool.name}");
        //    _currentTool.NetInfo = tool;
        //    _currentTool.UpdateItem();

        //    // This one's required to update all the items that are set to "same as selected road" when the selected road changes
        //    foreach (var netTypeItem in _items)
        //    {
        //        DebugUtils.Log($"Updating dropdown NetInfo from {netTypeItem.NetInfo.name} to {tool.name}");
        //        netTypeItem.OnChangedCallback();
        //    }
        //}

        //private void Changed()
        //{
        //    OnChangedCallback?.Invoke();
        //}

        public void AddItem(NetTypeItem item, bool isCurrentItem = false)
        {
            var component = AddUIComponent<UINetTypeItem>();            
            if (!isCurrentItem)
            {
                component.NetInfo = item.NetInfo;
                component.HorizontalOffset = item.HorizontalOffset;
                component.VerticalOffset = item.VerticalOffset;
                component.IsReversed = item.IsReversed;
                component.Index = _items.Count;
                component.OnChanged += UiNetTypeItemOnOnChanged;
                component.OnDeleteClicked += UiNetTypeItemOnOnDeleteClicked;
                _items.Add(component);
            }
            else
            {
                component.OnAddClicked += UiNetTypeItemOnOnAddClicked;
                component.IsCurrentItem = true;
            }                
            
            _space.BringToFront();
        }

        public void DeleteItem(int index)
        {
            var component = _items[index];

            component.OnChanged -= UiNetTypeItemOnOnChanged;
            component.OnDeleteClicked -= UiNetTypeItemOnOnDeleteClicked;

            if (component.IsCurrentItem)
                component.OnAddClicked -= UiNetTypeItemOnOnAddClicked;

            RemoveUIComponent(component);
            Destroy(component);
        }

        //public void UpdateItem(NetTypeItemEventArgs item)
        //{
        //    var component = _items[item.ItemIndex];
        //    component.HorizontalOffset = item.HorizontalOffset;
        //    component.VerticalOffset = item.VerticalOffset;
        //    component.NetInfo = item.SelectedNetworkIndex == 0
        //        ? PrefabCollection<NetInfo>.FindLoaded(_currentTool.NetInfo.name)
        //        : Singleton<ParallelRoadTool>.instance.AvailableRoadTypes[item.SelectedNetworkIndex];
        //    component.IsReversed = item.IsReversedNetwork;
        //}
    }
}