using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FineRoadTool;

namespace ParallelRoadTool.Extensions
{
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
            if (destination.m_netAI == null || source.m_netAI == null) return destination;

            var sourceWrapper = new RoadAIWrapper(source.m_netAI);
            var destinationWrapper = new RoadAIWrapper(destination.m_netAI);

            NetInfo result;

            if (source == sourceWrapper.bridge || source.name.ToLowerInvariant().Contains("bridge"))
                result = destinationWrapper.bridge;
            else if (source == sourceWrapper.elevated || source.name.ToLowerInvariant().Contains("elevated"))
                result = destinationWrapper.elevated;
            else if (source == sourceWrapper.slope || source.name.ToLowerInvariant().Contains("slope"))
            {
                result = destinationWrapper.slope;
                isSlope = true;
            }
            else if (source == sourceWrapper.tunnel || source.name.ToLowerInvariant().Contains("tunnel"))
                result = destinationWrapper.tunnel;
            else
                result = destination;

            // Sanity check, of them may be null
            result = result ?? destination;

            DebugUtils.Log($"Got a {destination.name}, new road is {result.name} [source = {source.name}]");

            return result;
        }

    }
}
