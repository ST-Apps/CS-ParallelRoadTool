namespace ParallelRoadTool.Actions;

/// <summary>
///     Base class for every mod's action.
///     Each action should be able to be executed and rendered, with an optional debug render when needed.
/// </summary>
internal abstract class ActionBase
{
    #region Properties

    /// <summary>
    ///     Easier access to game's NetTool since it will be the most used tool to execute actions.
    /// </summary>
    protected static NetTool NetTool => ToolsModifierControl.GetTool<NetTool>();

    /// <summary>
    ///     Easier access to game's RenderManager since it will be the most used tool to render actions.
    /// </summary>
    protected static RenderManager RenderManager => RenderManager.instance;

    public abstract ActionFactory.ActionType ActionType { get; }

    #endregion

    /// <summary>
    ///     This method contains the core logic for the specific action.
    ///     Executing an action is a destructive operation (e.g. game data will change).
    /// </summary>
    public abstract void Execute();

    /// <summary>
    ///     This method renders the action to provide a preview of the final outcome.
    ///     Rendering an action is a non-destructive operation (e.g. nothing will change in game data).
    /// </summary>
    /// <param name="cameraInfo"></param>
    protected abstract void Render(RenderManager.CameraInfo cameraInfo);

    /// <summary>
    ///     This method renders the action to provide a preview of the final outcome.
    ///     Rendering an action is a non-destructive operation (e.g. nothing will change in game data).
    /// </summary>
    public void Render()
    {
        Render(RenderManager.CurrentCameraInfo);
    }

    /// <summary>
    ///     This method renders any eventual extra information regarding current action for debug purposes.
    ///     Rendering an action is a non-destructive operation (e.g. nothing will change in game data).
    /// </summary>
    /// <param name="cameraInfo"></param>
    public abstract void RenderDebug(RenderManager.CameraInfo cameraInfo);
}

//internal abstract class ActionResultBase<TResult> : ActionBase where TResult : IActionResult
//{
//    #region Properties

//    public abstract TResult Result { get; protected set; }

//    #endregion
//}

internal interface IActionResult
{
    ActionFactory.ActionType ActionType { get; }
}
