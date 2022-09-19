using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ColossalFramework;
using CSUtil.Commons;
using ParallelRoadTool.Models;

namespace ParallelRoadTool.Managers
{
    public class PresetsManager
    {
        #region Properties

        // TODO: make it constant and move out of Configuration
        private static string AutoSaveFolderName = Configuration.AutoSaveFolderPath;
        private static string AutoSaveDefaultFileName = Configuration.AutoSaveFileName;

        #endregion

        #region Methods

        #region Internals

        #endregion

        #region Public API

        public static IEnumerable<string> ListSavedFiles()
        {
            Log.Info(@$"[{nameof(PresetsManager)}.{nameof(ListSavedFiles)}] Loading saved presets from ""{AutoSaveFolderName}""");

            if (!Directory.Exists(AutoSaveFolderName)) return new string[] { };

            // Get all files matching *.xml besides the auto-save one that can't be overwritten
            var files = Directory.GetFiles(AutoSaveFolderName, "*.xml")
                            .Where(f => Path.GetFileNameWithoutExtension(f) != AutoSaveFolderName)
                            .Select(Path.GetFileNameWithoutExtension)
                            .ToArray();

            Log.Info($"[{nameof(PresetsManager)}.{nameof(ListSavedFiles)}] Found {files.Length} presets");
            Log._Debug($"[{nameof(PresetsManager)}.{nameof(ListSavedFiles)}] Files: [{string.Join(", ", files)}]");

            return files;
        }

        public static bool CanSavePreset(string fileName)
        {
            var path = Path.Combine(AutoSaveFolderName, $"{fileName}.xml");
            return !File.Exists(path);
        }

        public static void SavePreset(string fileName)
        {
            var path = Path.Combine(AutoSaveFolderName, $"{fileName}.xml");
            var xmlNetItems = Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes
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

        public static IEnumerable<XMLNetItem> LoadPreset(string fileName)
        {
            var path = Path.Combine(AutoSaveFolderName, $"{fileName}.xml");

            Log.Info(@$"[{nameof(PresetsManager)}.{nameof(LoadPreset)}] Loading preset from ""{path}""");

            try
            {
                var xmlSerializer = new XmlSerializer(typeof(XMLNetItem[]));
                using var streamReader = new StreamReader(path);
                var data = xmlSerializer.Deserialize(streamReader);

                return (XMLNetItem[])data;
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
