using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Interfaces;
using ParallelRoadTool.UI.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    internal class UINetListPanel : UIPanel
    {
        #region Constants

        // We only have padding for bottom side to separate multiple elements
        private readonly RectOffset LayoutPadding = new RectOffset(0, 0, 0, UIConstants.Padding);

        private static readonly object _lock = new object();

        #endregion

        #region Events

        public event PropertyChangedEventHandler<int> DeleteNetworkButtonEventClicked;
        public event PropertyChangedEventHandler<NetTypeItemEventArgs> NetTypeEventChanged;
        public event MouseEventHandler ToggleDropdownButtonEventClick;

        private void NetSetupPanel_DeleteNetworkButtonEventClicked(UIComponent component, int index)
        {
            Log._Debug($"[{nameof(UINetListPanel)}.{nameof(NetSetupPanel_DeleteNetworkButtonEventClicked)}] Deleting item with index {index}");

            DeleteNetworkButtonEventClicked?.Invoke(component, index);
        }

        private void NetSetupPanel_NetTypeEventChanged(UIComponent component, NetTypeItemEventArgs value)
        {
            NetTypeEventChanged?.Invoke(null, value);
        }

        private void NetSetupPanelOnToggleDropdownButtonEventClick(UIComponent component, UIMouseEventParameter eventparam)
        {
            // We don't use this because we want to route the exact caller
            ToggleDropdownButtonEventClick?.Invoke(component, null);
        }

        public void RefreshNetworks(List<NetInfoItem> networks)
        {
            lock(_lock)
            {
                var children = GetComponentsInChildren<UINetSetupPanel>();

                Log._Debug($"[{nameof(UINetListPanel)}.{nameof(RefreshNetworks)}] Received {networks.Count} networks with {children.Length} children");

                for (int i = 0; i < children.Length; i++)
                {
                    if (i >= networks.Count)
                    {
                        DestroyImmediate(children[i]);
                    }
                    else
                    {
                        children[i].Render(networks[i]);
                        children[i].CurrentIndex = i;
                    }
                }
            }
        }

        #endregion

        #region Unity

        #region Components

        private UILabel _tutorialText;

        #endregion

        public override void Awake()
        {
            // NetworkList
            name = $"{Configuration.ResourcePrefix}NetworkList";
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutPadding = LayoutPadding;
            autoLayoutDirection = LayoutDirection.Vertical;
        }

        #endregion

        #region Control

        /// <summary>
        /// 
        /// </summary>
        /// <param name="netInfo"></param>
        internal void AddNetwork(NetInfoItem netInfo)
        {
            // Basic setup
            var netSetupPanel = AddUIComponent<UINetSetupPanel>();
            netSetupPanel.FitWidth(this, 0);
            netSetupPanel.DeleteNetworkButtonEventClicked += NetSetupPanel_DeleteNetworkButtonEventClicked;
            netSetupPanel.NetTypeEventChanged += NetSetupPanel_NetTypeEventChanged;
            netSetupPanel.ToggleDropdownButtonEventClick += NetSetupPanelOnToggleDropdownButtonEventClick;

            // Finally render the panel
            netSetupPanel.Render(netInfo);
            netSetupPanel.CurrentIndex = childCount - 1;
        }

        #endregion
    }
}
