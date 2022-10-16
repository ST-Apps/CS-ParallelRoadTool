using System;
using ColossalFramework;
using CSUtil.Commons;
using HarmonyLib;
using ParallelRoadTool.Extensions;
using ParallelRoadTool.Models;
using ParallelRoadTool.Utils;
using UnityEngine;

namespace ParallelRoadTool.Actions;

/// <summary>
///     This action will create a segment based on the three provided <see cref="NetTool.ControlPoint" /> items.
/// </summary>
internal class CreateSegmentAction : ActionBase
{
    #region Fields

    private readonly NetTool.ControlPoint _endPoint;
    private readonly NetTool.ControlPoint _middlePoint;

    private readonly NetInfoItem _netInfoItem;
    private readonly int         _networkIndex;

    private readonly NetTool.ControlPoint _startPoint;
    private readonly NetTool.ControlPoint _targetEndPoint;
    private readonly NetTool.ControlPoint _targetMiddlePoint;

    private readonly NetTool.ControlPoint _targetStartPoint;

    #endregion

    #region Properties

    public override ActionFactory.ActionType ActionType => ActionFactory.ActionType.CreateSegment;

    public CreateSegmentActionResult Result { get; private set; }

    #endregion

    public CreateSegmentAction(int         networkIndex,
                               NetInfoItem netInfoItem,

                               //NetTool.ControlPoint startPoint,
                               //NetTool.ControlPoint middlePoint,
                               //NetTool.ControlPoint endPoint,
                               NetTool.ControlPoint targetStartPoint,
                               NetTool.ControlPoint targetMiddlePoint,
                               NetTool.ControlPoint targetEndPoint)
    {
        _networkIndex = networkIndex;
        _netInfoItem  = netInfoItem;

        _targetStartPoint  = targetStartPoint;
        _targetMiddlePoint = targetMiddlePoint;
        _targetEndPoint    = targetEndPoint;

        //_startPoint  = startPoint;
        //_middlePoint = middlePoint;
        //_endPoint    = endPoint;
    }

    #region Reverse Patches

    [HarmonyPatch]
    private static class NetToolReversePatch
    {
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NetTool), nameof(global::NetTool.RenderOverlay), typeof(RenderManager.CameraInfo), typeof(NetInfo), typeof(Color),
                      typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
        internal static void RenderOverlay(object                   instance,
                                           RenderManager.CameraInfo cameraInfo,
                                           NetInfo                  info,
                                           Color                    color,
                                           NetTool.ControlPoint     startPoint,
                                           NetTool.ControlPoint     middlePoint,
                                           NetTool.ControlPoint     endPoint)
        {
            // No implementation is required as this will call the original method
            throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(NetTool), "CreateNodeImpl", typeof(NetInfo), typeof(bool), typeof(bool), typeof(NetTool.ControlPoint),
                      typeof(NetTool.ControlPoint), typeof(NetTool.ControlPoint))]
        public static bool CreateNodeImpl(object               instance,
                                          NetInfo              info,
                                          bool                 needMoney,
                                          bool                 switchDirection,
                                          NetTool.ControlPoint startPoint,
                                          NetTool.ControlPoint middlePoint,
                                          NetTool.ControlPoint endPoint)
        {
            // No implementation is required as this will call the original method
            throw new NotImplementedException("This is not supposed to be happening, please report this exception with its stacktrace!");
        }
    }

    #endregion

    #region ActionBase

    public override void Execute()
    {
        // After lots of tries this is what looks like being the easiest option to deal with inverting a network's direction.
        // To invert the current network we temporarily invert traffic direction for the current game.
        // This will force the CreateSegment method to receive true for the invert parameter and thus to create every segment in the opposite direction.
        // This value will be restored once we will be done with all of the segments we need to create.
        if (_netInfoItem.IsReversed)
        {
            Log._Debug($"[{nameof(CreateSegmentAction)}.{nameof(Execute)}] Inverting: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");

            Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic.Invert();

            Log._Debug($"[{nameof(CreateSegmentAction)}.{nameof(Execute)}] Inverted: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");
        }

        // Draw the offset segment for the current network
        if (!NetToolReversePatch.CreateNodeImpl(NetTool, _netInfoItem.NetInfo, false, false, _targetStartPoint, _targetMiddlePoint, _targetEndPoint))
        {
            var toolErrors = NetTool.CreateNode(_netInfoItem.NetInfo, _targetStartPoint, _targetMiddlePoint, _targetEndPoint,
                                                NetTool.m_nodePositionsSimulation, 1000, true, false, true, false, false, false, 0, out _, out _,
                                                out _, out _);

            Log.Error($"[{nameof(CreateSegmentAction)}.{nameof(Execute)}] Segment creation failed because {toolErrors:g}");
        }

        // Creation completed, we store the new ids so that we can match everything later
        // If nodes are 0 we retrieve them back from their position
        var targetStartPointNodeId = _targetStartPoint.m_node;
        var targetEndPointNodeId = _targetEndPoint.m_node;
        if (targetStartPointNodeId == 0)
            _targetStartPoint.m_position.AtPosition(_netInfoItem.NetInfo, out targetStartPointNodeId, out _);
        if (targetEndPointNodeId == 0)
            _targetEndPoint.m_position.AtPosition(_netInfoItem.NetInfo, out targetEndPointNodeId, out _);

        // We can now store them in the temporary state that is passed between prefix and postfix
        Result = new CreateSegmentActionResult(_networkIndex, _targetStartPoint with { m_node = targetStartPointNodeId }, _targetMiddlePoint,
                                               _targetEndPoint with { m_node = targetEndPointNodeId });

        if (!_netInfoItem.IsReversed) return;
        Log._Debug($"[{nameof(CreateSegmentAction)}.{nameof(Execute)}] Reverting: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");

        Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic.Invert();

        Log._Debug($">>> Reverted: {Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic:g}");
    }

    protected override void Render(RenderManager.CameraInfo cameraInfo)
    {
        // Check if current node can be created. If not change color to red.
        var currentColor = _netInfoItem.Color;
        if (!ControlPointUtils.CanCreate(_netInfoItem.NetInfo, _targetStartPoint, _targetMiddlePoint, _targetEndPoint, _netInfoItem.IsReversed))
            currentColor = Color.red;

        // Render the overlay for current offset segment
        NetToolReversePatch.RenderOverlay(NetTool, cameraInfo, _netInfoItem.NetInfo, currentColor, _targetStartPoint, _targetMiddlePoint,
                                          _targetEndPoint);
    }

    public override void RenderDebug(RenderManager.CameraInfo cameraInfo)
    {
        throw new NotImplementedException();
    }

    #endregion
}

internal class CreateSegmentActionResult : IActionResult
{
    #region Properties

    public int NetworkIndex { get; }

    public NetTool.ControlPoint TargetStartPoint { get; }

    public NetTool.ControlPoint TargetEndPoint { get; }

    #endregion

    public CreateSegmentActionResult(int                  networkIndex,
                                     NetTool.ControlPoint targetStartPoint,
                                     NetTool.ControlPoint targetEndPoint,
                                     NetTool.ControlPoint targetMiddlePoint)
    {
        NetworkIndex     = networkIndex;
        TargetStartPoint = targetStartPoint;
        TargetEndPoint   = targetEndPoint;
    }

    public ActionFactory.ActionType ActionType => ActionFactory.ActionType.CreateSegment;
}
