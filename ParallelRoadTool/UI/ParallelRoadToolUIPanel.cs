using System;
using ColossalFramework.UI;
using ParallelRoadTool.EventArgs;
using UnityEngine;

namespace ParallelRoadTool.UI
{
    /// <summary>
    /// This class should be the base for any implementation of the main UIPanel for the tool.
    /// The reason behind this class is that it should give us a common interface to work with while we try different kinds of UIs.
    /// It should also help to swap between UIs without much issues, making development faster.
    /// </summary>
    public abstract class ParallelRoadToolUIPanel : UIPanel
    {
        protected UITextureAtlas TextureAtlas;

        #region Events 

        /// <summary>
        /// Event launched when the tool status is toggled
        /// </summary>
        public event EventHandler<ParallelToolToggledEventArgs> ParallelToolToggled;
        protected virtual void OnParallelToolToggled(object sender, ParallelToolToggledEventArgs args)
        {
            ParallelToolToggled?.Invoke(sender, args);
        }

        /// <summary>
        /// Event launched when anything changes in tool's configuration (number of networks, network type, distances etc.)
        /// </summary>
        public event EventHandler<NetworksConfigurationChangedEventArgs> NetworksConfigurationChanged;
        protected virtual void OnNetworksConfigurationChanged(object sender, NetworksConfigurationChangedEventArgs args)
        {
            NetworksConfigurationChanged?.Invoke(sender, args);
        }

        #endregion

        protected void LoadResources()
        {
            string[] spriteNames =
            {
                "Anarchy",
                "AnarchyDisabled",
                "AnarchyFocused",
                "AnarchyHovered",
                "AnarchyPressed",
                "Bending",
                "BendingDisabled",
                "BendingFocused",
                "BendingHovered",
                "BendingPressed"
            };

            TextureAtlas = ResourceLoader.CreateTextureAtlas("ParallelRoadTool", spriteNames, "ParallelRoadTool.Icons.");

            var defaultAtlas = ResourceLoader.GetAtlas("Ingame");
            Texture2D[] textures =
            {
                defaultAtlas["OptionBase"].texture,
                defaultAtlas["OptionBaseFocused"].texture,
                defaultAtlas["OptionBaseHovered"].texture,
                defaultAtlas["OptionBasePressed"].texture,
                defaultAtlas["OptionBaseDisabled"].texture
            };

            ResourceLoader.AddTexturesInAtlas(TextureAtlas, textures);
        }

    }
}
