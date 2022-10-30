// <copyright file="UIModalWindow.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.UI.Shared;

using ColossalFramework;
using ColossalFramework.UI;

/// <summary>
///     Base class for a specific modal window, containing the modal animation and the basic logic to push/pop a modal
///     window.
/// </summary>
public abstract class UIModalWindow : UIWindow
{
    public UIModalWindow(string iconAtlasName)
        : base(iconAtlasName)
    {
        // Main/ModalEffect
        var modalEffect = GetUIView().panelsLibraryModalEffect;
        if (modalEffect != null && !modalEffect.isVisible)
        {
            modalEffect.Show(false);
            ValueAnimator.Animate(
                "ModalEffect",
                (val) => { modalEffect.opacity = val; },
                new AnimatedFloat(0f, 1f, 0.7f, EasingType.CubicEaseOut));
        }

        UIView.PushModal(this);
        Focus();
    }

    public override float PanelWidth => 450;

    public override float PanelHeight => 180;

    protected override bool PreClose()
    {
        UIView.PopModal();

        var modalEffect = GetUIView().panelsLibraryModalEffect;
        if (modalEffect != null && modalEffect.isVisible)
        {
            ValueAnimator.Animate(
                "ModalEffect",
                (val) => { modalEffect.opacity = val; },
                new AnimatedFloat(1f, 0f, 0.7f, EasingType.CubicEaseOut),
                () => { modalEffect.Hide(); });
        }

        Unfocus();
        return base.PreClose();
    }
}
