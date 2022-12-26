// <copyright file="UIWindow.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Shared;

using AlgernonCommons.UI;
using ColossalFramework.UI;
using UnityEngine;
using Utils;

/// <summary>
///     Base class for a generic closable and draggable window.
/// </summary>
public abstract class UIWindow : StandalonePanel
{
    protected UIPanel Container;

    protected UIWindow(string iconAtlasName)
    {
        var spriteAtlas = UITextures.LoadSingleSpriteAtlas(iconAtlasName);
        SetIcon(spriteAtlas, "normal");

        // Main/ContainerPanel
        Container = AddUIComponent<UIPanel>();
        Container.name = nameof(Container);
        Container.backgroundSprite = "GenericPanel";
        Container.autoLayoutDirection = LayoutDirection.Vertical;
        Container.autoLayoutPadding = UIHelpers.RectOffsetFromPadding(UIConstants.Padding);
        Container.autoLayoutPadding.bottom = 0;
        Container.autoLayout = true;
        Container.relativePosition = new Vector2(UIConstants.Padding, spriteAtlas["normal"].height + UIConstants.Padding);
        Container.FitWidth(this, UIConstants.Padding);
        Container.height = PanelHeight - spriteAtlas["normal"].height - (2 * UIConstants.Padding);
    }

    protected override void OnKeyDown(UIKeyEventParameter p)
    {
        if (!Input.GetKey(KeyCode.Escape))
        {
            return;
        }

        p.Use();
        Close();
    }
}
