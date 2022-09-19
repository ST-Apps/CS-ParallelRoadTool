using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Shared;
using ParallelRoadTool.UI.Utils;
using System.Collections.Generic;
using ParallelRoadTool.Managers;
using UnityEngine;

namespace ParallelRoadTool.UI.Presets
{
    public sealed class UILoadPresetWindow : UIModalWindow
    {

        #region Events

        public event PropertyChangedEventHandler<string> LoadButtonEventClicked;

        #endregion

        #region Callbacks

        private void FileListOnEventSelectionChanged(UIComponent component, object fileName)
        {
            var presetItems = PresetsManager.LoadPreset((string)fileName);
            _presetDetails.LoadPreset(presetItems);
        }

        private void LoadPresetButtonOnEventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            LoadButtonEventClicked?.Invoke(this, (string) _fileList.SelectedItem);
        }

        #endregion

        #region Properties

        public override float PanelHeight => 256;

        public override float PanelWidth => 512;

        protected override string PanelTitle => Translations.Translate("LABEL_LOAD_PRESET_WINDOW_TITLE");

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
            _loadPresetButton = UIButtons.AddButton(detailsContainer, 0, 0, Translations.Translate("LABEL_LOAD_PRESET_BUTTON_TITLE"), detailsContainer.width);

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
