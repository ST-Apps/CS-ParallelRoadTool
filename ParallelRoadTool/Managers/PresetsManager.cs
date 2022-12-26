// <copyright file="PresetsManager.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Managers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using ColossalFramework;
using ColossalFramework.IO;
using CSUtil.Commons;
using Models;

/// <summary>
///     This manager is responsible for everything related to presets handling, such as listing presets and loading/saving
///     them.
/// </summary>
public class PresetsManager
{
    /// <summary>
    ///     Name for the auto-save file to be saved/loaded every-time we exit/launch the game.
    /// </summary>
    private const string AutoSaveDefaultFileName = ".autosave";

    /// <summary>
    ///     Path for the saved presets.
    /// </summary>
    private static readonly string PresetsFolderPath = Path.Combine(Path.Combine(DataLocation.localApplicationData, Mod.SimplifiedName), "Presets");

    static PresetsManager()
    {
        // Check, and eventually create, the presets folder
        if (!Directory.Exists(PresetsFolderPath))
        {
            Directory.CreateDirectory(PresetsFolderPath);
        }
    }

    /// <summary>
    ///     Lists all the saved files, besides <see cref="AutoSaveDefaultFileName" />.
    /// </summary>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="string"/> containing all the file names for saved presets.</returns>
    public static IEnumerable<string> ListSavedFiles()
    {
        Log.Info(@$"[{nameof(PresetsManager)}.{nameof(ListSavedFiles)}] Loading saved presets from ""{PresetsFolderPath}""");

        // Presets folder is missing, skip
        if (!Directory.Exists(PresetsFolderPath))
        {
            return new string[] { };
        }

        // Get all files matching *.xml besides the auto-save one that can't be overwritten
        var files = Directory.GetFiles(PresetsFolderPath, "*.xml")
            .Select(Path.GetFileNameWithoutExtension).Where(f => f != AutoSaveDefaultFileName)
            .ToArray();

        Log.Info($"[{nameof(PresetsManager)}.{nameof(ListSavedFiles)}] Found {files.Length} presets");
        Log._Debug($"[{nameof(PresetsManager)}.{nameof(ListSavedFiles)}] Files: [{string.Join(", ", files)}]");

        return files;
    }

    /// <summary>
    ///     Checks if the preset file exists.
    ///     This is mostly used to show the overwrite modal during preset saving.
    /// </summary>
    /// <param name="fileName">Name of the preset to check for existence.</param>
    /// <returns>True if the preset exists.</returns>
    public static bool PresetExists(string fileName)
    {
        var path = GetFullPathFromFileName(fileName);
        return File.Exists(path);
    }

    /// <summary>
    ///     Saves the provided networks to an XML preset.
    /// </summary>
    /// <param name="networks"><see cref="IEnumerable{T}"/> of <see cref="NetInfoItem"/> containing the networks that will be saved as preset.</param>
    /// <param name="fileName">Name of the file to save the preset to.</param>
    public static void SavePreset(IEnumerable<NetInfoItem> networks, string fileName = null)
    {
        // Presets folder is missing, skip
        if (!Directory.Exists(PresetsFolderPath))
        {
            Directory.CreateDirectory(PresetsFolderPath);
        }

        // Default to auto-save if no filename is provided
        fileName ??= AutoSaveDefaultFileName;
        var path = GetFullPathFromFileName(fileName);

        // If preset already exists we firstly delete it
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        // Generate the array of elements with the serializable items
        var xmlNetItems = networks.Select(n => new XMLNetItem
        {
            Name = n.Name,
            IsReversed = n.IsReversed,
            HorizontalOffset = n.HorizontalOffset,
            VerticalOffset = n.VerticalOffset,
        }).ToArray();

        Log.Info(@$"[{nameof(PresetsManager)}.{nameof(SavePreset)}] Saving preset to ""{path}"" with {xmlNetItems.Length} networks");

        try
        {
            // Write the array for file
            var xmlSerializer = new XmlSerializer(xmlNetItems.GetType());
            using var streamWriter = new StreamWriter(path);
            xmlSerializer.Serialize(streamWriter, xmlNetItems);
        }
        catch (Exception e)
        {
            Log.Info(@$"[{nameof(PresetsManager)}.{nameof(SavePreset)}] Failed saving ""{fileName}"" to {path}");
            Log.Exception(e);

            throw;
        }
    }

    /// <summary>
    ///     Simply deletes the preseet by deleting the related file (if existing).
    /// </summary>
    /// <param name="fileName">Name of the file to be deleted.</param>
    public static void DeletePreset(string fileName)
    {
        if (!PresetExists(fileName))
        {
            Log.Info(@$"[{nameof(PresetsManager)}.{nameof(DeletePreset)}] Preset ""{fileName}"" doesn't exist, cannot delete.");

            return;
        }

        var path = GetFullPathFromFileName(fileName);
        File.Delete(path);

        Log.Info(@$"[{nameof(PresetsManager)}.{nameof(DeletePreset)}] Delete preset ""{fileName}"" at ""{path}"".");
    }

    /// <summary>
    ///     Loads the provided XML file into a collection of <see cref="NetInfoItem" />.
    /// </summary>
    /// <param name="fileName">Name of the file to be loaded as a preset.</param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="NetInfoItem"/> containing all the loaded networks.</returns>
    public static IEnumerable<NetInfoItem> LoadPreset(string fileName = null)
    {
        // Default to auto-save if no filename is provided
        fileName ??= AutoSaveDefaultFileName;
        var path = GetFullPathFromFileName(fileName);

        // Provided file doesn't exist, abort
        if (!File.Exists(path))
        {
            Log.Info(@$"[{nameof(PresetsManager)}.{nameof(LoadPreset)}] Provided preset file not found on path: ""{path}""");
            return new NetInfoItem[] { };
        }

        Log.Info(@$"[{nameof(PresetsManager)}.{nameof(LoadPreset)}] Loading preset from ""{path}""");

        try
        {
            // Deserialize the provided file
            var xmlSerializer = new XmlSerializer(typeof(XMLNetItem[]));
            using var streamReader = new StreamReader(path);
            var data = (XMLNetItem[])xmlSerializer.Deserialize(streamReader);

            // Convert the deserialized results into our internal format
            var result = ToNetInfoItems(data).ToArray();

            Log._Debug($"[{nameof(PresetsManager)}.{nameof(LoadPreset)}] Loaded {result.Length} networks.");
            return result;
        }
        catch (Exception e)
        {
            Log.Info(@$"[{nameof(PresetsManager)}.{nameof(SavePreset)}] Failed reading ""{fileName}"" from {path}");
            Log.Exception(e);

            throw;
        }
    }

    /// <summary>
    ///     Generates the full path for a given file inside <see cref="PresetsFolderPath" />.
    /// </summary>
    /// <param name="fileName">Name of the preset.</param>
    /// <returns>Full path in presets folder for the given file name.</returns>
    private static string GetFullPathFromFileName(string fileName)
    {
        return Path.Combine(PresetsFolderPath, $"{fileName}.xml");
    }

    /// <summary>
    ///     Converts a collection of <see cref="XMLNetItem" /> to a collection of <see cref="NetInfoItem" />.
    /// </summary>
    /// <param name="networks"><see cref="IEnumerable{T}"/> of <see cref="XMLNetItem"/> that needs to be converted.</param>
    /// <returns><see cref="IEnumerable{T}"/> of <see cref="NetInfoItem"/> containing converted items.</returns>
    private static IEnumerable<NetInfoItem> ToNetInfoItems(IEnumerable<XMLNetItem> networks)
    {
        return networks.Select(n => new NetInfoItem(
            Singleton<ParallelRoadToolManager>.instance.FromName(n.Name),
            n.HorizontalOffset,
            n.VerticalOffset,
            n.IsReversed));
    }
}
