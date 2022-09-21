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
        private class UIPresetListRow : UIListRow
        {
            private UINetInfoTinyPanel _netInfoRow;

            public override void Display(object data, int rowIndex)
            {
                Log._Debug($"RENDERING {data}");
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

                var netData = data as NetInfo;
                _netInfoRow.Render(new NetInfoItem(netData));

                // Deselect(rowIndex);
            }
        }

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
            _searchTextField.eventTextChanged += (component, value) =>
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
                                                             var netInfo = (NetInfo)netObject;
                                                             if (netInfo.name.ToLower().Contains(value) ||
                                                                 netInfo.GenerateBeautifiedNetName().ToLower().Contains(value))
                                                                 filteredData.Add(netInfo);
                                                         }

                                                         _netItemsList.Data = filteredData;
                                                     }
                                                 };
        }

        #endregion

        #region Public API

        public void Open(UIComponent owner)
        {
            _owner = owner;

            // Move it to the correct position
            absolutePosition = owner.absolutePosition + new Vector3(0, owner.height);

            // Set width to match its owner
            width = owner.width;
            _searchTextField.FitWidth(this, UIConstants.Padding);

            // Make it appear above all the other components
            BringToFront();
            _searchTextField.Focus();
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
                items.Add(netItem);

            if (_netItemsList == null)

                // We need to create this here because this panel's size is set by its container and is not know during ctor
                _netItemsList = UIList.AddUIList<UIPresetListRow>(this, 0, 0,
                                                                  width - 2 * UIConstants.Padding,
                                                                  4 * UIConstants.LargeSize - UIConstants.Padding, UIConstants.LargeSize);
            _netItemsList.Data = items;
            _netItems = items;

            // Pre-select
            // TODO
        }

        #endregion

        #endregion
    }
}
