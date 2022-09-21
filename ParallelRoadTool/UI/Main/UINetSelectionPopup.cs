using System.Collections.Generic;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Shared;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Main
{
    internal class UINetSelectionPopup : UIPanel
    {
        private class UINetItemListRow : UIListRow
        {
            private UINetInfoTinyPanel _netInfoRow;

            public override float RowHeight => UIConstants.LargeSize;

            public override void Display(object data, int rowIndex)
            {
                if (_netInfoRow == null)
                {
                    // Init our row
                    width = parent.width;
                    height = RowHeight;
                    isInteractive = false;

                    // Set the item
                    _netInfoRow = AddUIComponent<UINetInfoTinyPanel>();
                    _netInfoRow.relativePosition = Vector2.zero;
                    _netInfoRow.isInteractive = false;
                }

                var netData = data as NetInfoItem;
                _netInfoRow.NetInfoItem = netData;

                Deselect(rowIndex);
            }

            /// <summary>
            /// Sets the row display to the selected state (highlighted).
            /// </summary>
            public override void Select()
            {
                // Background.spriteName = null; //"ListItemHighlight";
                Background.opacity = 1f;
            }

            /// <summary>
            /// Sets the row display to the deselected state.
            /// </summary>
            /// <param name="rowIndex">Row index number (for background banding).</param>
            public override void Deselect(int rowIndex)
            {
                Background.spriteName = "GenericPanel";
                Background.color = _netInfoRow.NetInfoItem.Color;
                Background.opacity = 0.25f;
            }
        }

        private NetInfoItem _ownerItem;

        #region Events

        public event PropertyChangedEventHandler<NetTypeItemEventArgs> OnPopupSelectionChanged;

        #endregion

        #region Unity

        #region Components

        private UIComponent _owner;
        private readonly UITextField _searchTextField;
        private UIList _netItemsList;
        private FastList<object> _netItems;

        #endregion

        #region Lifecycle

        public UINetSelectionPopup()
        {
            // Main
            backgroundSprite = "OptionsDropboxListbox";
            zOrder = int.MaxValue;
            clipChildren = true;
            autoLayout = true;
            autoLayoutPadding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);
            autoLayoutDirection = LayoutDirection.Vertical;

            // Main/SearchText
            _searchTextField = UITextFields.AddBigTextField(this, 0, 0);

            height = _searchTextField.height + 4 * UIConstants.LargeSize + 4 * UIConstants.Padding;

            AttachToEvents();
        }

        public override void LateUpdate()
        {
            var ray = GetCamera().ScreenPointToRay(Input.mousePosition);

            // TODO: add condition on children
            if (!Input.GetMouseButtonDown(0) || Raycast(ray) || _owner.Raycast(ray) || _netItemsList.Raycast(ray))
                return;
            Close();
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (!Input.GetKey(KeyCode.Escape)) return;

            p.Use();
            Close();
        }

        #endregion

        #endregion

        #region Control

        #region Internals

        private void AttachToEvents()
        {
            _searchTextField.eventTextChanged += (_, value) =>
                                                 {
                                                     if (string.IsNullOrEmpty(value))
                                                     {
                                                         _netItemsList.Data = _netItems;
                                                     }
                                                     else
                                                     {
                                                         var filteredData = new FastList<object>();

                                                         foreach (var netObject in _netItems)
                                                         {
                                                             var netInfo = (NetInfoItem)netObject;
                                                             if (netInfo.Name.ToLower().Contains(value) ||
                                                                 netInfo.BeautifiedName.ToLower().Contains(value))
                                                                 filteredData.Add(netInfo);
                                                         }

                                                         _netItemsList.Data = filteredData;
                                                         _netItemsList.CurrentPosition = 0;
                                                     }
                                                 };
        }

        private void NetItemsListOnEventSelectionChanged(UIComponent component, object value)
        {
            var netInfoItem = (NetInfoItem)value;
            OnPopupSelectionChanged?.Invoke(null, new NetTypeItemEventArgs(-1, netInfoItem.Name));
        }

        #endregion

        #region Public API

        public void Open(UIComponent owner, NetInfoItem currentItem = null)
        {
            _owner = owner;

            // Move it to the correct position
            absolutePosition = owner.absolutePosition + new Vector3(0, owner.height);

            // Set width to match its owner
            width = owner.width - 2 * UIConstants.Padding;
            _searchTextField.FitWidth(this, UIConstants.Padding);

            // Make it appear above all the other components
            BringToFront();
            _searchTextField.Focus();

            _ownerItem = currentItem;
        }

        public void Close()
        {
            isVisible = false;

            DestroyImmediate(_netItemsList);
            DestroyImmediate(this);
        }

        public void LoadNetworks(IEnumerable<NetInfo> networks)
        {
            var items = new FastList<object>();
            foreach (var netItem in networks)
                items.Add(new NetInfoItem(netItem));

            if (_netItemsList == null)
            {

                // We need to create this here because this panel's size is set by its container and is not know during ctor
                _netItemsList = UIList.AddUIList<UINetItemListRow>(this, 0, 0,
                                                                  width - 2 * UIConstants.Padding,
                                                                  4 * UIConstants.LargeSize - UIConstants.Padding, UIConstants.LargeSize);
                _netItemsList.BackgroundSprite = null;
                _netItemsList.EventSelectionChanged += NetItemsListOnEventSelectionChanged;
            }

            _netItemsList.Data = items;
            _netItems = items;

            // Pre-select
            if (_ownerItem != null)
            {
                _netItemsList.FindItem<NetInfoItem>(i => i.BeautifiedName == _ownerItem.BeautifiedName);
            }
        }

        #endregion

        #endregion
    }
}
