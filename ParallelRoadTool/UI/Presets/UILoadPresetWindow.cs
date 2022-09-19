using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using ParallelRoadTool.UI.Shared;
using ParallelRoadTool.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSUtil.Commons;
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
        }

        private void DetachFromEvents()
        {
            _fileList.EventSelectionChanged -= FileListOnEventSelectionChanged;
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

        //private readonly UITextField _fileNameInput;
        //private readonly UIButton _saveButton;
        private readonly UIList _fileList;
        private readonly UIPresetDetailsPanel _presetDetails;

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

            _presetDetails = Container.AddUIComponent<UIPresetDetailsPanel>();
            _presetDetails.size = _fileList.size;

            //var bottomPanel = Container.AddUIComponent<UIPanel>();
            //bottomPanel.relativePosition = new Vector2(0, UIConstants.Padding);
            //bottomPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            //bottomPanel.autoLayout = true;
            //bottomPanel.autoFitChildrenVertically = true;
            //bottomPanel.FitWidth(Container, UIConstants.Padding);

            //// Main/FileName
            //_fileNameInput = UITextFields.AddBigTextField(bottomPanel, 0, 0);
            //_fileNameInput.FitWidth(bottomPanel, 0);

            //// Main/Save
            //var saveButtonAtlas = UITextures.LoadSingleSpriteAtlas("Save");
            //_saveButton = UIButtons.AddIconButton(bottomPanel, 0, 0, saveButtonAtlas["normal"].height, saveButtonAtlas);
            //_saveButton.isEnabled = false; //Disabled at start
            //_fileNameInput.height = saveButtonAtlas["normal"].height;
            //_fileNameInput.width -= _saveButton.width - UIConstants.Padding;

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
