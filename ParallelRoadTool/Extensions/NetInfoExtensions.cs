// <copyright file="NetInfoExtensions.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Extensions;

using System.Text.RegularExpressions;
using Wrappers;

public static class NetInfoExtensions
{
    /// <summary>
    ///     Returns a destination NetInfo with the same road type (elevated, tunnel etc.) as source one.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="isSlope"></param>
    /// <returns></returns>
    public static NetInfo GetNetInfoWithElevation(this NetInfo source, NetInfo destination, out bool isSlope)
    {
        isSlope = false;
        if (destination.m_netAI == null || source.m_netAI == null)
        {
            return destination;
        }

        var sourceWrapper = new RoadAIWrapper(source.m_netAI);
        var destinationWrapper = new RoadAIWrapper(destination.m_netAI);

        NetInfo result;

        if (source == sourceWrapper.Bridge || source.name.ToLowerInvariant().Contains("bridge"))
        {
            result = destinationWrapper.Bridge;
        }
        else if (source == sourceWrapper.Elevated || source.name.ToLowerInvariant().Contains("elevated"))
        {
            result = destinationWrapper.Elevated;
        }
        else if (source == sourceWrapper.Slope || source.name.ToLowerInvariant().Contains("slope"))
        {
            result = destinationWrapper.Slope;
            isSlope = true;
        }
        else if (source == sourceWrapper.Tunnel || source.name.ToLowerInvariant().Contains("tunnel"))
        {
            result = destinationWrapper.Tunnel;
        }
        else
        {
            result = destination;
        }

        // Sanity check, of them may be null
        result ??= destination;

        return result;
    }

    /// <summary>
    ///     Localizes, when possible, <see cref="NetInfo.name" /> and trims some spaces.
    /// </summary>
    /// <param name="netInfo"></param>
    /// <returns></returns>
    public static string GenerateBeautifiedNetName(this NetInfo netInfo)
    {
        // Trim string and then remove duplicate spaces
        return Regex.Replace(netInfo.GetUncheckedLocalizedTitle().Trim(), " {2,}", " ");
    }

    /// <summary>
    ///     Returns true if the <see cref="NetInfo" /> doesn't have any backward facing lane.
    /// </summary>
    /// <param name="netInfo"></param>
    /// <returns></returns>
    public static bool IsOneWayOnly(this NetInfo netInfo)
    {
        // One-way roads only have forward lanes
        return !netInfo.m_hasBackwardVehicleLanes;
    }
}
