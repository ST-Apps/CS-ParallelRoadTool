// <copyright file="NetInfoExtensions.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Extensions;

using Wrappers;

/// <summary>
///     This class provides extension methods for <see cref="NetInfo"/> objects.
/// </summary>
public static class NetInfoExtensions
{
    /// <summary>
    ///     Returns a destination NetInfo with the same road type (elevated, tunnel etc.) as source one.
    ///     It uses the <see cref="RoadAIWrapper"/> to read the correct road type info and extract it.
    /// </summary>
    /// <param name="source"><see cref="NetInfo"/> object from which we're trying to copy the road type.</param>
    /// <param name="destination"><see cref="NetInfo"/> object matching source's road type.</param>
    /// <param name="isSlope">True if the matching <see cref="NetInfo"/> is a slope road type.</param>
    /// <returns>A <see cref="NetInfo"/>with the same road type (elevated, tunnel etc.) as source one.</returns>
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

        // Sanity check, one of them may be null
        result ??= destination;

        return result;
    }

    /// <summary>
    ///     Check if the provided <see cref="NetInfo"/> is one-way only by checking if <see cref="NetInfo.m_hasBackwardVehicleLanes"/> is false.
    ///     A one-way <see cref="NetInfo"/> will only have forward lanes.
    /// </summary>
    /// <param name="netInfo"><see cref="NetInfo"/> for which to check road's direction.</param>
    /// <returns>True if the <see cref="NetInfo" /> doesn't have any backward facing lane.</returns>
    public static bool IsOneWayOnly(this NetInfo netInfo)
    {
        // One-way roads only have forward lanes
        return !netInfo.m_hasBackwardVehicleLanes;
    }
}
