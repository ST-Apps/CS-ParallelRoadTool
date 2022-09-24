using System.Collections.Generic;
using ColossalFramework.UI;
using CSUtil.Commons;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

namespace ParallelRoadTool.UI.Main
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class UINetListPanel : UIPanel
    {
        #region Fields

        private static readonly object Lock = new();

        #endregion

        #region Events

        public event PropertyChangedEventHandler<int> DeleteNetworkButtonEventClicked;
        public event PropertyChangedEventHandler<NetTypeItemEventArgs> NetTypeEventChanged;
        public event ChildComponentEventHandler OnPopupOpened;

        #endregion

        #region Callbacks

        private void NetSetupPanel_DeleteNetworkButtonEventClicked(UIComponent component, int index)
        {
            Log._Debug($"[{nameof(UINetListPanel)}.{nameof(NetSetupPanel_DeleteNetworkButtonEventClicked)}] Deleting item with index {index}.");

            DeleteNetworkButtonEventClicked?.Invoke(component, index);
        }

        private void NetSetupPanel_NetTypeEventChanged(UIComponent component, NetTypeItemEventArgs value)
        {
            NetTypeEventChanged?.Invoke(null, value);
        }

        private void NetSetupPanelOnOnPopupOpened(UIComponent container, UIComponent child)
        {
            OnPopupOpened?.Invoke(container, child);
        }

        #endregion

        #region Unity

        #region Lifecycle

        public override void Awake()
        {
            base.Awake();

            // NetworkList
            name = $"{Constants.ResourcePrefix}NetworkList";
            autoFitChildrenVertically = true;
            autoLayout = true;
            autoLayoutPadding = new RectOffset(0, 0, 0, UIConstants.Padding);
            autoLayoutDirection = LayoutDirection.Vertical;
        }

        #endregion

        #endregion

        #region Control

        #region Internals

        /// <summary>
        /// Adds a <see cref="UINetSetupPanel"/> and renders its contents.
        /// We also register to its events.
        /// </summary>
        /// <param name="netInfo"></param>
        internal void AddNetwork(NetInfoItem netInfo)
        {
            // Basic setup
            var netSetupPanel = AddUIComponent<UINetSetupPanel>();
            netSetupPanel.FitWidth(this, 0);

            // Events
            netSetupPanel.DeleteNetworkButtonEventClicked += NetSetupPanel_DeleteNetworkButtonEventClicked;
            netSetupPanel.NetTypeEventChanged += NetSetupPanel_NetTypeEventChanged;
            netSetupPanel.OnPopupOpened += NetSetupPanelOnOnPopupOpened;
            netSetupPanel.OnPopupSelectionChanged += NetSetupPanelOnOnPopupSelectionChanged;

            // Finally render the panel
            netSetupPanel.Render(netInfo);
            netSetupPanel.CurrentIndex = childCount - 1;
        }

        private void NetSetupPanelOnOnPopupSelectionChanged(UIComponent component, NetTypeItemEventArgs value)
        {
            NetTypeEventChanged?.Invoke(component, value);
        }

        #endregion

        #region Public API

        /// <summary>
        /// This panel acts as a list with some kind of built-in virtualization.
        /// This means that we'll try to reuse every panel that has been added to this one.
        /// If the provided networks are more than the number of <see cref="UINetSetupPanel"/> we already have then we add more.
        /// If not, we re-render the ones that are visible already and remove the exceeding ones.
        /// </summary>
        /// <param name="networks"></param>
        public void RefreshNetworks(List<NetInfoItem> networks)
        {
            // Avoid race conditions and other ugly things if users click stuff too quick.
            lock (Lock)
            {
                // Retrieve all the NetSetupPanels we already have
                var children = GetComponentsInChildren<UINetSetupPanel>();

                Log._Debug($"[{nameof(UINetListPanel)}.{nameof(RefreshNetworks)}] Received {networks.Count} networks with {children.Length} children");

                for (var i = 0; i < children.Length; i++)
                {
                    if (i >= networks.Count)
                    {
                        // This means that we have more NetSetupPanel than what we need, so we just forcefully remove them
                        DestroyImmediate(children[i]);
                    }
                    else
                    {
                        // This means we can re-use the current panel and we can update its index
                        children[i].Render(networks[i]);
                        children[i].CurrentIndex = i;
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
