using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    public class UINetList : UIPanel
    {
        #region Properties

        private List<UINetTypeItem> _items;
        private UIPanel _space;

        #endregion

        #region Events/Callbacks

        public event PropertyChangedEventHandler<NetTypeItemEventArgs> OnItemChanged;
        public event EventHandler OnItemAdded;
        public event PropertyChangedEventHandler<int> OnItemDeleted;
        public event PropertyChangedEventHandler<int> OnSearchModeToggled;

        #endregion

        #region Handlers

        private void UnsubscribeToUiEvents()
        {
            foreach (var uiNetTypeItem in _items)
            {
                uiNetTypeItem.OnChanged -= UiNetTypeItemOnOnChanged;
                uiNetTypeItem.OnDeleteClicked -= UiNetTypeItemOnOnDeleteClicked;
                uiNetTypeItem.OnSearchModeToggled -= UiNetTypeItemOnOnSearchModeToggled;

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
        }

        private void UiNetTypeItemOnOnAddClicked(object sender, EventArgs eventArgs)
        {
            Log._Debug($"[{nameof(UINetList)}.{nameof(UiNetTypeItemOnOnAddClicked)}] Event triggered with eventArgs: {eventArgs}");

            OnItemAdded?.Invoke(this, null);
        }

        private void UiNetTypeItemOnOnSearchModeToggled(UIComponent component, int value)
        {
            Log._Debug($"[{nameof(UINetList)}.{nameof(UiNetTypeItemOnOnSearchModeToggled)}] Event triggered with value: {value}");

            OnSearchModeToggled?.Invoke(this, value);
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

            PresetsUtils.Import(Configuration.AutoSaveFileName);
        }

        public override void OnDestroy()
        {
            UnsubscribeToUiEvents();

            Destroy(_space);
            foreach (var uiNetTypeItem in _items) Destroy(uiNetTypeItem);
            base.OnDestroy();
        }

        #endregion

        #region Control

        public void DisableSearchMode(int index)
        {
            _items[index].DisableSearchMode();
        }

        public void FilterItemDropdown(int index, string text)
        {
            _items[index].FilterDropdown(text);
        }

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
                component.OnSearchModeToggled += UiNetTypeItemOnOnSearchModeToggled;
                _items.Add(component);
            }
            else
            {
                component.OnAddClicked += UiNetTypeItemOnOnAddClicked;
                component.IsCurrentItem = true;
            }

            _space.BringToFront();
        }

        public void UpdateItem(NetTypeItem item, int index)
        {
            var currentItem = _items[index];
            currentItem.HorizontalOffset = item.HorizontalOffset;
            currentItem.VerticalOffset = item.VerticalOffset;
            currentItem.IsReversed = item.IsReversed;

            currentItem.UpdateItem();
        }

        public void DeleteItem(int index)
        {
            // Destroy UI
            var component = _items[index];
            component.OnChanged -= UiNetTypeItemOnOnChanged;
            component.OnDeleteClicked -= UiNetTypeItemOnOnDeleteClicked;
            if (component.IsCurrentItem)
                component.OnAddClicked -= UiNetTypeItemOnOnAddClicked;
            RemoveUIComponent(component);
            Destroy(component);

            // Remove stored component
            _items.RemoveAt(index);

            // We need to shift index value for any element after current index or we'll lose update events
            for (var i = index; i < _items.Count; i++) _items[i].Index -= 1;
        }

        public void ClearItems()
        {
            Log._Debug($"[{nameof(UINetList)}.{nameof(ClearItems)}] Deleting {_items.Count} networks");

            // deleting items from last to first to avoid index shifting for remaining items
            for (var i = _items.Count - 1; i >= 0; i--)
            {
                Log._Debug($"[{nameof(UINetList)}.{nameof(ClearItems)}] Deleting item at index: {i}: {_items[i].NetInfo.name}");

                DeleteItem(i);
            }
        }

        public void UpdateDropdowns()
        {
            for (var i = _items.Count - 1; i >= 0; i--) _items[i].UpdateDropdown();
        }

        #endregion
    }
}
