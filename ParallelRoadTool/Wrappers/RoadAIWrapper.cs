// <copyright file="RoadAIWrapper.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Wrappers;

using System.Reflection;

// ReSharper disable InconsistentNaming
public class RoadAIWrapper
{
    private readonly NetAI m_ai;
    private readonly FieldInfo m_bridge;

    private readonly FieldInfo m_elevated;
    private readonly FieldInfo m_invisible;
    private readonly FieldInfo m_slope;
    private readonly FieldInfo m_tunnel;

    public RoadAIWrapper(NetAI ai)
    {
        m_ai = ai;

        try
        {
            m_elevated = m_ai.GetType().GetField("m_elevatedInfo");
            m_bridge = m_ai.GetType().GetField("m_bridgeInfo");
            m_slope = m_ai.GetType().GetField("m_slopeInfo");
            m_tunnel = m_ai.GetType().GetField("m_tunnelInfo");
            m_invisible = m_ai.GetType().GetField("m_invisible");
        }
        catch
        {
            m_elevated = null;
            m_bridge = null;
            m_slope = null;
            m_tunnel = null;
            m_invisible = null;
        }
    }

    public bool HasElevation => m_elevated != null && m_bridge != null && m_slope != null && m_tunnel != null;

    public NetInfo Info
    {
        get => m_ai.m_info;
        set => m_ai.m_info = value;
    }

    public NetInfo Elevated
    {
        get => HasElevation ? m_elevated.GetValue(m_ai) as NetInfo : null;
        set
        {
            if (!HasElevation)
            {
                return;
            }

            m_elevated.SetValue(m_ai, value);
        }
    }

    public NetInfo Bridge
    {
        get => HasElevation ? m_bridge.GetValue(m_ai) as NetInfo : null;
        set
        {
            if (!HasElevation)
            {
                return;
            }

            m_bridge.SetValue(m_ai, value);
        }
    }

    public NetInfo Slope
    {
        get => HasElevation ? m_slope.GetValue(m_ai) as NetInfo : null;
        set
        {
            if (!HasElevation)
            {
                return;
            }

            m_slope.SetValue(m_ai, value);
        }
    }

    public NetInfo Tunnel
    {
        get => HasElevation ? m_tunnel.GetValue(m_ai) as NetInfo : null;
        set
        {
            if (!HasElevation)
            {
                return;
            }

            m_tunnel.SetValue(m_ai, value);
        }
    }

    public bool IsInvisible()
    {
        if (m_invisible != null)
        {
            return (bool)m_invisible.GetValue(m_ai);
        }

        return false;
    }
}
