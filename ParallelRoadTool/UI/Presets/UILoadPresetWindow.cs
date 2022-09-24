using System.Collections.Generic;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using ParallelRoadTool.Managers;
using ParallelRoadTool.Models;
using ParallelRoadTool.UI.Shared;
using ParallelRoadTool.UI.Utils;
using UnityEngine;

// ReSharper disable ClassNeverInstantiated.Global

namespace ParallelRoadTool.UI.Presets
{
    /// <summary>
    ///     This <see cref="UIModalWindow" /> is used to show the load dialog for a specific preset file.
    ///     The dialog will show a <see cref="UIList" /> with all the saved presets, a <see cref="UIList" /> containing the
    ///     <see cref="NetInfoItem" /> that we're about to load and a <see cref="UIButton" /> to load the preset.
    /// </summary>
    public sealed class UILoadPresetWindow : UIModalWindow
    {
        #region Properties

        public override float PanelHeight => 256;

        public override float PanelWidth => 512;

        protected override string PanelTitle => Translations.Translate("LABEL_LOAD_PRESET_WINDOW_TITLE");

        #endregion

        #region Events

        public event PropertyChangedEventHandler<string> LoadButtonEventClicked;

        #endregion

        #region Callbacks

        private void FileListOnEventSelectionChanged(UIComponent component, object fileName)
        {
            var presetItems = PresetsManager.LoadPreset((string)fileName);
            _presetDetails.LoadPreset(presetItems);

            _loadPresetButton.isEnabled = true;
        }

        private void LoadPresetButtonOnEventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            LoadButtonEventClicked?.Invoke(this, (string)_fileList.SelectedItem);
        }

        #endregion

        #region Control

        #region Internals

        private void AttachToEvents()
        {
            _fileList.EventSelectionChanged += FileListOnEventSelectionChanged;
            _loadPresetButton.eventClicked += LoadPresetButtonOnEventClicked;
        }

        private void DetachFromEvents()
        {
            _fileList.EventSelectionChanged -= FileListOnEventSelectionChanged;
            _loadPresetButton.eventClicked -= LoadPresetButtonOnEventClicked;
        }

        #endregion

        #region Public API

        /// <summary>
        ///     Reloads the list with the newly provided file names.
        /// </summary>
        /// <param name="items"></param>
        public void RefreshItems(IEnumerable<string> items)
        {
            var fileList = new FastList<object>();
            foreach (var item in items) fileList.Add(item);

            _fileList.Data = fileList;
        }

        #endregion

        #endregion

        #region Unity

        #region Components

        private readonly UIList _fileList;
        private readonly UIPresetDetailsPanel _presetDetails;
        private readonly UIButton _loadPresetButton;

        #endregion

        #region Lifecycle

        public UILoadPresetWindow() : base("Parallel")
        {
            Container.autoLayoutDirection = LayoutDirection.Horizontal;

            // Main/FileList
            _fileList = UIList.AddUIList<UIFileListRow>(Container, 0, 0,
                                                        Container.width / 2 - 2 * UIConstants.Padding,
                                                        Container.height - 2 * UIConstants.Padding, UIConstants.TinySize);
            _fileList.RowHeight -= 4;

            // Main/DetailsContainer
            var detailsContainer = Container.AddUIComponent<UIPanel>();
            detailsContainer.autoLayoutDirection = LayoutDirection.Vertical;
            detailsContainer.autoLayoutPadding.bottom = UIConstants.Padding;
            detailsContainer.autoLayout = true;
            detailsContainer.size = _fileList.size;

            // Main/DetailsContainer/PresetDetails
            _presetDetails = detailsContainer.AddUIComponent<UIPresetDetailsPanel>();
            _presetDetails.size = detailsContainer.size - new Vector2(0, UIConstants.SmallSize + UIConstants.Padding);

            // Main/DetailsContainer/LoadButton
            _loadPresetButton = UIButtons.AddButton(detailsContainer, 0, 0, Translations.Translate("LABEL_LOAD_PRESET_BUTTON_TITLE"),
                                                    detailsContainer.width);
            _loadPresetButton.isEnabled = false;

            // Events
            AttachToEvents();
        }

        public override void OnDestroy()
        {
            DetachFromEvents();

            base.OnDestroy();
        }

        #endregion

        #endregion
    }
}
