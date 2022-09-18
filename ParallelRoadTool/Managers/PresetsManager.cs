using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSUtil.Commons;

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
            Log.Info($"[{nameof(PresetsManager)}.{nameof(ListSavedFiles)}] Loading saved presets from {AutoSaveFolderName}");

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

        #endregion

        #endregion
    }
}
