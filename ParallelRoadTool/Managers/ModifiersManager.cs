namespace ParallelRoadTool.Managers;

/// <summary>
///     Handles key modifiers such as CTRL, ALT or SHIFT.
/// </summary>
internal static class ModifiersManager
{
    #region Properties

    public static bool IsShiftPressed { get; set; }

    public static bool IsCtrlPressed { get; set; }

    public static bool IsAltPressed { get; set; }

    #endregion
}
