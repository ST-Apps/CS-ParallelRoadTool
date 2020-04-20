namespace ParallelRoadTool.Utils
{
    /// <summary>
    ///     Utility methods to deal with Harmony-patched code
    /// </summary>
    public static class HarmonyUtils
    {
        /// <summary>
        ///     Checks if provided methodName matches with expected name, by also checking if methodName has been patched by
        ///     Harmony.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsNameMatching(string methodName, string name)
        {
            return methodName == name
                   || methodName.StartsWith($"{name}_Patch")
                   || methodName.StartsWith($"DMD<DMD<{name}_Patch");
        }
    }
}
