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

        #endregion

        #region Events

        public event PropertyChangedEventHandler<int> DeleteNetworkButtonEventClicked;
        
        private void NetSetupPanel_DeleteNetworkButtonEventClicked(UIComponent component, int index)
        {
            Log._Debug($"[{nameof(UINetListPanel)}.{nameof(NetSetupPanel_DeleteNetworkButtonEventClicked)}] Deleting item with index {index}");

            DeleteNetworkButtonEventClicked?.Invoke(component, index);

            // To avoid moving back and forth between UI and data we just delete the panel here
            // DeleteNetwork(component, index);
            // RemoveUIComponent(component);
        }

        public void RefreshNetworks(List<NetInfoItem> networks)
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

        //private static readonly object _lock = new object();

        //private void DeleteNetwork(UIComponent component, int index)
        //{
        //    lock (_lock)
        //    {
        //        var children = new List<GameObject>();
        //        for (int i = 0; i < childCount; i++)
        //        {
        //            if (i == index) continue;
        //            children.Add(transform.GetChild(i).gameObject);
        //        }
        //        transform.DetachChildren();
        //        DestroyImmediate(component);
        //        SortNetworks(children);
        //    }
        //}

        //private void SortNetworks(List<GameObject> children)
        //{
        //    // TODO: non funziona
        //    // TODO: spostare Destroy fuori da DeleteNetwork e rendere DeleteNetwork un sor
        //    // TODO: childPanels non serve generarlo così, basta fare children.Add(transform.GetChild.GetComponent alla 47)
        //    var childPanels = children.Select(c => c.GetComponent<UINetSetupPanel>()).OrderBy(p => p.HorizontalOffset).ToArray();
        //    //foreach (var child in childPanels)
        //    //{
        //    //    //child.transform.parent = transform;
        //    //    this.AttachUIComponent(child.gameObject);
        //    //}
        //    this.AttachUIComponent(childPanels[3].gameObject);
        //    this.AttachUIComponent(childPanels[1].gameObject);
        //    this.AttachUIComponent(childPanels[0].gameObject);
        //    this.AttachUIComponent(childPanels[2].gameObject);
        //}

        #endregion

        #region Unity

        #region Components

        private readonly UIList<UINetSetupPanel> _netSetupPanels = new UIList<UINetSetupPanel>();

        #endregion

        public override void Awake()
        {
            // NetworkList
            name = $"{Configuration.ResourcePrefix}NetworkList";
            autoLayout = true;
            autoLayoutPadding = LayoutPadding;
            autoFitChildrenVertically = true;
            autoLayoutDirection = LayoutDirection.Vertical;
        }

        #endregion

        #region Control

        /// <summary>
        /// 
        /// </summary>
        /// <param name="netInfo"></param>
        /// <param name="isCurrentNetwork">True if this is the info panel for the currently selected network in <see cref="NetTool"/>. Current network can't be customized.</param>
        internal void AddNetwork(NetInfoItem netInfo)
        {
            // Basic setup
            var netSetupPanel = AddUIComponent<UINetSetupPanel>();
            netSetupPanel.FitWidth(this, 0);
            netSetupPanel.DeleteNetworkButtonEventClicked += NetSetupPanel_DeleteNetworkButtonEventClicked;
            _netSetupPanels.Add(netSetupPanel);

            // Finally render the panel
            netSetupPanel.Render(netInfo);
            netSetupPanel.CurrentIndex = childCount - 1;
        }

        #endregion
    }
}
