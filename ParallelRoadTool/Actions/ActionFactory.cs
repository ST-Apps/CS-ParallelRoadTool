using System;
using ParallelRoadTool.Models;

namespace ParallelRoadTool.Actions;

internal static class ActionFactory
{
    //public static ActionBase Create(ActionType actionType, params object[] parameters)
    //{
    //    return actionType switch
    //    {
    //        ActionType.CreateNode => throw new NotImplementedException(),
    //        ActionType.CreateSegment => new CreateSegmentAction((int)parameters[0], parameters[1] as NetInfoItem, (NetTool.ControlPoint)parameters[2],
    //                                                            (NetTool.ControlPoint)parameters[3], (NetTool.ControlPoint)parameters[4]),
    //        ActionType.MoveNode => throw new NotImplementedException(),
    //        _                   => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
    //    };
    //}

    public static CreateSegmentAction Create(int                  networkIndex,
                                             NetInfoItem          netInfoItem,
                                             NetTool.ControlPoint targetStartPoint,
                                             NetTool.ControlPoint targetMiddlePoint,
                                             NetTool.ControlPoint targetEndPoint)
    {
        return new CreateSegmentAction(networkIndex, netInfoItem, targetStartPoint, targetMiddlePoint, targetEndPoint);
    }

    internal enum ActionType
    {
        CreateNode,
        CreateSegment,
        MoveNode
    }
}
