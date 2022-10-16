using System;
using System.Collections.Generic;
using System.Linq;
using ColossalFramework;
using ParallelRoadTool.Actions;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Utils;
using ParallelRoadTool.Wrappers;

namespace ParallelRoadTool.Managers;

internal static class ActionsManager
{
    #region Fields

    private static readonly Queue<ActionBase> ActionsQueue = new();

    public static readonly Queue<object> ActionsOutputs = new();

    #endregion

    #region Properties

    /// <summary>
    ///     Easier access to game's NetTool since it will be the most used tool to execute actions.
    /// </summary>
    private static NetTool NetTool => ToolsModifierControl.GetTool<NetTool>();

    #endregion

    public static void GenerateQueue(NetInfo info, NetTool.ControlPoint startPoint, NetTool.ControlPoint middlePoint, NetTool.ControlPoint endPoint)
    {
        // Empty old actions to start fresh
        ActionsQueue.Clear();

        // Render only if we have at least two distinct points
        if (startPoint.m_position == endPoint.m_position)
            return;

        // Reset start direction because it feels like it's never right
        startPoint.m_direction = (middlePoint.m_position - startPoint.m_position).normalized;

        for (var i = 0; i < Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes.Count; i++)
        {
            var currentRoadInfos = Singleton<ParallelRoadToolManager>.instance.SelectedNetworkTypes[i];

            // Horizontal offset must be negated to appear on the correct side of the original segment if we're on left-handed drive
            var horizontalOffset = currentRoadInfos.HorizontalOffset * (Singleton<ParallelRoadToolManager>.instance.IsLeftHandTraffic ? 1 : -1);
            var verticalOffset = currentRoadInfos.VerticalOffset;

            // If the user didn't select a NetInfo we'll use the one he's using for the main road                
            var selectedNetInfo = info.GetNetInfoWithElevation(currentRoadInfos.NetInfo ?? info, out _);

            // If the user is using a vertical offset we try getting the relative elevated net info and use it
            if (verticalOffset > 0 && selectedNetInfo.m_netAI.GetCollisionType() != ItemClass.CollisionType.Elevated)
                selectedNetInfo = new RoadAIWrapper(selectedNetInfo.m_netAI).elevated ?? selectedNetInfo;

            // Generate offset points for the current network
            ControlPointUtils.GenerateOffsetControlPoints(startPoint, middlePoint, endPoint, horizontalOffset, verticalOffset, selectedNetInfo, i,
                                                          NetTool.m_mode, out var currentStartPoint, out var currentMiddlePoint,
                                                          out var currentEndPoint);

            // Generate the CreateSegment action
            ActionsQueue.Enqueue(ActionFactory.Create(i, currentRoadInfos, currentStartPoint, currentMiddlePoint, currentEndPoint));
        }
    }

    /// <summary>
    ///     Executes and consumes all the <see cref="ActionBase" /> in the queue.
    /// </summary>
    public static IEnumerable<IActionResult> Execute()
    {
        // TODO: far uscire l'output giusto?
        while (ActionsQueue.Any())
        {
            var action = ActionsQueue.Dequeue();
            IActionResult result = null;

            action.Execute();

            switch (action.ActionType)
            {
                case ActionFactory.ActionType.CreateNode:
                    break;
                case ActionFactory.ActionType.CreateSegment:
                    result = (action as CreateSegmentAction)?.Result;
                    break;
                case ActionFactory.ActionType.MoveNode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            yield return result;

            // if (output != null)
            //ActionsOutputs.Enqueue(output);
        }
    }

    /// <summary>
    ///     Renders all the <see cref="ActionBase" /> in the queue without consuming them.
    /// </summary>
    public static void Render()
    {
        foreach (var action in ActionsQueue) action.Render();
    }
}
