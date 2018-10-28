namespace ParallelRoadTool
{
    /// <summary>
    ///     Constants and data needed for the mod to work
    /// </summary>
    public static class Configuration
    {
        public const string SettingsFileName = "ParallelRoadTool";

        #region UI

        public const string AssemblyName = "ParallelRoadTool";
        public const string ResourcePrefix = "PRT_";

        public const string AssetsFolder = "Assets";
        public const string IconsFolder = "Icons";
        public static readonly string IconsNamespace = $"{AssemblyName}.{AssetsFolder}.{IconsFolder}";
        public const string LocalizationFolder = "Localizations";
        public static readonly string LocalizationNamespace = $"{AssemblyName}.{AssetsFolder}.{LocalizationFolder}";

        public static readonly string CustomAtlasName = $"{ResourcePrefix}Atlas";
        public const string DefaultAtlasName = "Ingame";

        public static readonly string[] CustomSpritesNames =
        {
            "Add",
            "AddDisabled",
            "AddFocused",
            "AddHovered",
            "AddPressed",
            "FindIt",
            "FindItDisabled",
            "FindItFocused",
            "FindItHovered",
            "FindItPressed",
            "Remove",
            "RemoveDisabled",
            "RemoveFocused",
            "RemoveHovered",
            "RemovePressed",
            "Parallel",
            "ParallelDisabled",
            "ParallelFocused",
            "ParallelHovered",
            "ParallelPressed",
            "Reverse",
            "ReverseDisabled",
            "ReverseFocused",
            "ReverseHovered",
            "ReversePressed",
            "Tutorial"
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