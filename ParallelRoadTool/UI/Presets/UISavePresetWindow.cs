﻿// <copyright file="UISavePresetWindow.cs" company="ST-Apps (S. Tenuta)">
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
using Shared;
using UnityEngine;
using Utils;

// ReSharper disable ClassNeverInstantiated.Global
/// <summary>
///     This <see cref="UIModalWindow" /> is used to show the save dialog for the current network configuration.
///     The dialog will show a <see cref="UITextField" /> with the currently selected file name, a <see cref="UIButton" />
///     to save the preset and a <see cref="UIList" /> containing all the files that we already have saved before.
/// </summary>
public sealed class UISavePresetWindow : UIModalWindow
{
    private readonly UITextField _fileNameInput;
    private readonly UIButton _saveButton;
    private readonly UIList _fileList;

    public UISavePresetWindow()
        : base("PRT-Logo-Small")
    {
        var topPanel = Container.AddUIComponent<UIPanel>();
        topPanel.relativePosition = new Vector2(0, UIConstants.Padding);
        topPanel.autoLayoutDirection = LayoutDirection.Horizontal;
        topPanel.autoLayout = true;
        topPanel.autoFitChildrenVertically = true;
        topPanel.FitWidth(Container, UIConstants.Padding);

        // Main/FileName
        _fileNameInput = UITextFields.AddBigTextField(topPanel, 0, 0);
        _fileNameInput.FitWidth(topPanel, 0);

        // Main/Save
        _saveButton = UIButtons.AddIconButton(topPanel, 0, 0, UIConstants.SmallSize, UITextures.LoadQuadSpriteAtlas("PRT-Save"));
        _saveButton.isEnabled = false; // Disabled at start
        _fileNameInput.height = UIConstants.SmallSize;
        _fileNameInput.width -= _saveButton.width - UIConstants.Padding;

        // Main/FileList
        _fileList = UIList.AddUIList<UIFileListRow>(Container, 0, 0, Container.width - (2 * UIConstants.Padding),
                                                    Container.height - topPanel.height - (3 * UIConstants.Padding), UIConstants.TinySize);
        _fileList.RowHeight -= 4;

        // Events
        AttachToEvents();
    }

    public event PropertyChangedEventHandler<string> SaveButtonEventClicked;

    public override float PanelHeight => 256;

    protected override string PanelTitle => Translations.Translate("LABEL_SAVE_PRESET_WINDOW_TITLE");

    public void RefreshItems(IEnumerable<string> items)
    {
        var fileList = new FastList<object>();
        foreach (var item in items)
        {
            fileList.Add(item);
        }

        _fileList.Data = fileList;
    }

    public override void OnDestroy()
    {
        DetachFromEvents();

        base.OnDestroy();
    }

    private void FileListOnEventSelectionChanged(UIComponent component, object value)
    {
        // Set the clicked item as filename
        _fileNameInput.text = (string)value;
    }

    private void FileNameInput_eventTextChanged(UIComponent component, string value)
    {
        // If text is not empty we can make the save button interactive again, otherwise we disable it
        _saveButton.isEnabled = !string.IsNullOrEmpty(value);
    }

    private void SaveButtonOnEventClicked(UIComponent component, UIMouseEventParameter eventParam)
    {
        // Check if file is going to be overwritten
        if (PresetsManager.PresetExists(_fileNameInput.text))
        {
            Log.Info(@$"[{nameof(UISavePresetWindow)}.{nameof(SaveButtonOnEventClicked)}] File ""{_fileNameInput.text}"" already exists, asking for confirmation...");

            // Show a confirmation popup asking to overwrite the file
            UIView.library.ShowModal<ConfirmPanel>(
                "ConfirmPanel",
                (_, ret) =>
                {
                    if (ret == 0)
                    {
                        // No action to do, user decided to not overwrite the file
                        Log.Info(@$"[{nameof(UISavePresetWindow)}.{nameof(SaveButtonOnEventClicked)}] User refused to overwrite ""{_fileNameInput.text}"".");
                        return;
                    }

                    // We can overwrite and thus save the file
                    Log.Info(@$"[{nameof(UISavePresetWindow)}.{nameof(SaveButtonOnEventClicked)}] User accepted to overwrite ""{_fileNameInput.text}"", saving...");
                    SaveButtonEventClicked?.Invoke(this, _fileNameInput.text);
                }).SetMessage(
                    Translations.Translate("LABEL_OVERWRITE_PRESET_TITLE"),
                    string.Format(
                        Translations.Translate("LABEL_OVERWRITE_PRESET_MESSAGE"), _fileNameInput.text));
        }
        else
        {
            // We can just save the file without asking for confirmation
            Log.Info(@$"[{nameof(UISavePresetWindow)}.{nameof(SaveButtonOnEventClicked)}] File ""{_fileNameInput.text}"" doesn't exists, saving...");
            SaveButtonEventClicked?.Invoke(this, _fileNameInput.text);
        }
    }

    private void AttachToEvents()
    {
        _fileList.EventSelectionChanged += FileListOnEventSelectionChanged;
        _fileNameInput.eventTextChanged += FileNameInput_eventTextChanged;
        _saveButton.eventClicked += SaveButtonOnEventClicked;
    }

    private void DetachFromEvents()
    {
        _fileList.EventSelectionChanged -= FileListOnEventSelectionChanged;
        _fileNameInput.eventTextChanged -= FileNameInput_eventTextChanged;
        _saveButton.eventClicked -= SaveButtonOnEventClicked;
    }
}
