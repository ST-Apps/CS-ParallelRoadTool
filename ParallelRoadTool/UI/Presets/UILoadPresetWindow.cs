// <copyright file="UILoadPresetWindow.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Presets;

using System.Collections.Generic;
using AlgernonCommons.Translation;
using AlgernonCommons.UI;
using ColossalFramework.UI;
using CSUtil.Commons;
using Managers;
using Models;
using Shared;
using UnityEngine;
using Utils;

// ReSharper disable ClassNeverInstantiated.Global
/// <summary>
///     This <see cref="UIModalWindow" /> is used to show the load dialog for a specific preset file.
///     The dialog will show a <see cref="UIList" /> with all the saved presets, a <see cref="UIList" /> containing the
///     <see cref="NetInfoItem" /> that we're about to load and a <see cref="UIButton" /> to load the preset.
/// </summary>
public sealed class UILoadPresetWindow : UIModalWindow
{
    private readonly UIList _fileList;
    private readonly UIPresetDetailsPanel _presetDetails;
    private readonly UIButton _loadPresetButton;
    private readonly UIButton _deletePresetButton;

    public UILoadPresetWindow() : base("PRT-Logo-Small")
    {
        Container.autoLayoutDirection = LayoutDirection.Horizontal;

        // Main/FileList
        _fileList = UIList.AddUIList<UIFileListRow>(Container, 0, 0, (Container.width / 2) - (2 * UIConstants.Padding),
                                                    Container.height - (2 * UIConstants.Padding), UIConstants.TinySize);
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

        // Main/DetailsContainer/ButtonsContainer
        var buttonsContainer = detailsContainer.AddUIComponent<UIPanel>();
        buttonsContainer.autoLayoutDirection = LayoutDirection.Horizontal;
        buttonsContainer.autoLayoutPadding.bottom = UIConstants.Padding;
        buttonsContainer.autoLayout = true;
        buttonsContainer.height = UIConstants.MediumSize;
        buttonsContainer.width = _fileList.width;

        // Main/DetailsContainer/LoadButton
        _loadPresetButton = UIButtons.AddButton(buttonsContainer, 0, 0, Translations.Translate("LABEL_LOAD_PRESET_BUTTON_TITLE"),
                                                buttonsContainer.width/2);
        _loadPresetButton.isEnabled = false;

        // Main/DetailsContainer/DeleteButton
        _deletePresetButton = UIButtons.AddButton(buttonsContainer, 0, 0, Translations.Translate("LABEL_DELETE_PRESET_BUTTON_TITLE"),
                                                buttonsContainer.width/2);
        _deletePresetButton.isEnabled = false;

        // Events
        AttachToEvents();
    }

    public event PropertyChangedEventHandler<string> LoadButtonEventClicked;
    public event PropertyChangedEventHandler<string> DeleteButtonEventClicked;

    public override float PanelHeight => 256;

    public override float PanelWidth => 512;

    protected override string PanelTitle => Translations.Translate("LABEL_LOAD_PRESET_WINDOW_TITLE");

    /// <summary>
    ///     Reloads the list with the newly provided file names.
    /// </summary>
    /// <param name="items"></param>
    public void RefreshItems(IEnumerable<string> items)
    {
        var fileList = new FastList<object>();
        foreach (var item in items)
        {
            fileList.Add(item);
        }

        _fileList.Data = fileList;
        _fileList.SelectedIndex = -1;
        _presetDetails.ClearPreset();
    }

    public override void OnDestroy()
    {
        DetachFromEvents();

        base.OnDestroy();
    }

    private void FileListOnEventSelectionChanged(UIComponent component, object fileName)
    {
        var presetItems = PresetsManager.LoadPreset((string)fileName);
        _presetDetails.LoadPreset(presetItems);

        _loadPresetButton.isEnabled = true;
        _deletePresetButton.isEnabled = true;
    }

    private void LoadPresetButtonOnEventClicked(UIComponent component, UIMouseEventParameter eventParam)
    {
        LoadButtonEventClicked?.Invoke(this, (string)_fileList.SelectedItem);
    }

    private void DeletePresetButtonOnEventClicked(UIComponent component, UIMouseEventParameter eventParam)
    {
        // Show a confirmation popup asking to delete the file
        UIView.library.ShowModal<ConfirmPanel>("ConfirmPanel", (_, ret) =>
        {
            if (ret == 0)
            {
                // No action to do, user decided to not delete the file
                Log.Info(@$"[{nameof(UILoadPresetWindow)}.{nameof(DeletePresetButtonOnEventClicked)}] User refused to delete ""{(string)_fileList.SelectedItem}"".");
                return;
            }

            // We can delete and thus save the file
            Log.Info(@$"[{nameof(UILoadPresetWindow)}.{nameof(DeletePresetButtonOnEventClicked)}] User accepted to delete ""{(string)_fileList.SelectedItem}"", deleting...");
            
            DeleteButtonEventClicked?.Invoke(this, (string)_fileList.SelectedItem);
        }).SetMessage(
            Translations.Translate("LABEL_DELETE_CONFIRMATION_PRESET_TITLE"),
            string.Format(Translations.Translate("LABEL_DELETE_CONFIRMATION_PRESET_MESSAGE"), (string)_fileList.SelectedItem));
    }

    private void AttachToEvents()
    {
        _fileList.EventSelectionChanged += FileListOnEventSelectionChanged;
        _loadPresetButton.eventClicked += LoadPresetButtonOnEventClicked;
        _deletePresetButton.eventClicked += DeletePresetButtonOnEventClicked;
    }

    private void DetachFromEvents()
    {
        _fileList.EventSelectionChanged -= FileListOnEventSelectionChanged;
        _loadPresetButton.eventClicked -= LoadPresetButtonOnEventClicked;
        _deletePresetButton.eventClicked -= DeletePresetButtonOnEventClicked;
    }
}
