// <copyright file="UIRightDragHandle.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Main;

using ColossalFramework.UI;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
///     Utility class that allows right-click to drag, disabling left-click one.
/// </summary>
public class UIRightDragHandle : UIDragHandle
{
    public event DragEventHandler EventDragStart
    {
        add => eventDragStart += value;
        remove => eventDragStart -= value;
    }

    public event DragEventHandler EventDragEnd
    {
        add => eventDragEnd += value;
        remove => eventDragEnd -= value;
    }

    protected override void OnMouseDown(UIMouseEventParameter p)
    {
        if (p.buttons != UIMouseButton.Right)
        {
            return;
        }

        p = new UIMouseEventParameter(p.source, UIMouseButton.Left, p.clicks, p.ray, p.position, p.moveDelta, p.wheelDelta);

        base.OnMouseDown(p);
    }

    protected override void OnMouseMove(UIMouseEventParameter p)
    {
        if (p.buttons != UIMouseButton.Right)
        {
            return;
        }

        p = new UIMouseEventParameter(p.source, UIMouseButton.Left, p.clicks, p.ray, p.position, p.moveDelta, p.wheelDelta);

        base.OnMouseMove(p);
    }
}
