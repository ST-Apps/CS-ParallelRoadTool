using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using ColossalFramework;
using ColossalFramework.IO;
using CSUtil.Commons;
using ParallelRoadTool.Models;

namespace ParallelRoadTool.Managers
{
    /// <summary>
    ///     This manager is responsible for everything related to presets handling, such as listing presets and loading/saving
    ///     them.
    /// </summary>
    public class PresetsManager
    {
        #region Fields

        /// <summary>
        ///     Name for the auto-save file to be saved/loaded every-time we exit/launch the game.
        /// </summary>
        private const string AutoSaveDefaultFileName = ".autosave";

        /// <summary>
        ///     Path for the saved presets.
        /// </summary>
        private static readonly string PresetsFolderPath =
            Path.Combine(Path.Combine(DataLocation.localApplicationData, Mod.SimplifiedName), "Presets");

        #endregion

        static PresetsManager()
        {
            // Check, and eventually create, the presets folder
            if (!Directory.Exists(PresetsFolderPath))
                Directory.CreateDirectory(PresetsFolderPath);
        }

        #region Methods

        #region Internals

        /// <summary>
        ///     Generates the full path for a given file inside <see cref="PresetsFolderPath" />.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetFullPathFromFileName(string fileName)
        {
            return Path.Combine(PresetsFolderPath, $"{fileName}.xml");
        }

        /// <summary>
        ///     Converts a collection of <see cref="XMLNetItem" /> to a collection of <see cref="NetInfoItem" />.
        /// </summary>
        /// <param name="networks"></param>
        /// <returns></returns>
        private static IEnumerable<NetInfoItem> ToNetInfoItems(IEnumerable<XMLNetItem> networks)
        {
            return networks.Select(n => new NetInfoItem(Singleton<ParallelRoadToolManager>.instance.FromName(n.Name), n.HorizontalOffset,
                                                        n.VerticalOffset, n.IsReversed));
        }

        #endregion

        #region Public API

        /// <summary>
        ///     Lists all the saved files, besides <see cref="AutoSaveDefaultFileName" />.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> ListSavedFiles()
        {
            Log.Info(@$"[{nameof(PresetsManager)}.{nameof(ListSavedFiles)}] Loading saved presets from ""{PresetsFolderPath}""");

            // Presets folder is missing, skip
            if (!Directory.Exists(PresetsFolderPath)) return new string[] { };

            // Get all files matching *.xml besides the auto-save one that can't be overwritten
            var files = Directory.GetFiles(PresetsFolderPath, "*.xml")
                                 .Select(Path.GetFileNameWithoutExtension)
                                 .Where(f => f != AutoSaveDefaultFileName)
                                 .ToArray();

            Log.Info($"[{nameof(PresetsManager)}.{nameof(ListSavedFiles)}] Found {files.Length} presets");
            Log._Debug($"[{nameof(PresetsManager)}.{nameof(ListSavedFiles)}] Files: [{string.Join(", ", files)}]");

            return files;
        }

        /// <summary>
        ///     Checks if the preset file exists.
        ///     This is mostly used to show the overwrite modal during preset saving.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool PresetExists(string fileName)
        {
            var path = GetFullPathFromFileName(fileName);
            return !File.Exists(path);
        }

        /// <summary>
        ///     Saves the provided networks to an XML preset.
        /// </summary>
        /// <param name="networks"></param>
        /// <param name="fileName"></param>
        public static void SavePreset(IEnumerable<NetInfoItem> networks, string fileName = null)
        {
            // Presets folder is missing, skip
            if (!Directory.Exists(PresetsFolderPath))
                Directory.CreateDirectory(PresetsFolderPath);

            // Default to auto-save if no filename is provided
            fileName ??= AutoSaveDefaultFileName;
            var path = GetFullPathFromFileName(fileName);

            // If preset already exists we firstly delete it
            if (File.Exists(path))
                File.Delete(path);

            // Generate the array of elements with the serializable items
            var xmlNetItems = networks
                              .Select(n => new XMLNetItem
                              {
                                  Name = n.Name,
                                  IsReversed = n.IsReversed,
                                  HorizontalOffset = n.HorizontalOffset,
                                  VerticalOffset = n.VerticalOffset
                              })
                              .ToArray();

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
        ///     Loads the provided XML file into a collection of <see cref="NetInfoItem" />.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
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

        #endregion

        #endregion
    }
}
