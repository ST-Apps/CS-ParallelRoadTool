using System.IO;
using ColossalFramework.IO;

namespace ParallelRoadTool
{
    /// <summary>
    ///     Constants and data needed for the mod to work
    /// TODO: review after completing the 3.0 rewrite
    /// </summary>
    public static class Configuration
    {
        public const string SettingsFileName = "ParallelRoadTool";
        public const string AutoSaveFileName = "_PRTAutoSave";

        public static readonly string AutoSaveFolderPath =
            Path.Combine(Path.Combine(DataLocation.localApplicationData, AssemblyName), "Presets");

        public static readonly string AutoSaveFilePath = Path.Combine(AutoSaveFolderPath, AutoSaveFileName + ".xml");

        #region UI

        public const string AssemblyName = "ParallelRoadTool";
        public const string ResourcePrefix = "PRT_";

        public const string AssetsFolder = "Assets";
        public const string IconsFolder = "Icons";
        public static readonly string IconsNamespace = $"{AssemblyName}.{AssetsFolder}.{IconsFolder}";

        public static readonly string CustomAtlasName = $"{ResourcePrefix}Atlas";
        public const string DefaultAtlasName = "Ingame";

        public static readonly string[] CustomSpritesNames =
        {
            "Add",
            "FindIt",
            "FindItDisabled",
            "FindItFocused",
            "FindItHovered",
            "FindItPressed",
            "HorizontalOffset",
            "Load",
            "Parallel",
            "ParallelDisabled",
            "ParallelFocused",
            "ParallelHovered",
            "ParallelPressed",
            "Remove",
            "Reverse",
            "ReverseDisabled",
            "ReverseFocused",
            "ReverseHovered",
            "ReversePressed",
            "VerticalOffset",
            "Save"
        };

        public static readonly string[] DefaultSpritesNames =
        {
            "OptionBase",
            "OptionBaseFocused",
            "OptionBaseHovered",
            "OptionBasePressed",
            "OptionBaseDisabled",
            "Snapping",
            "SnappingFocused",
            "SnappingHovered",
            "SnappingPressed",
            "SnappingDisabled"
        };

        #endregion
    }
}
