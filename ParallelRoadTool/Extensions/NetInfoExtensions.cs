using System.Text.RegularExpressions;
using ParallelRoadTool.Wrappers;

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

            var sourceWrapper      = new RoadAIWrapper(source.m_netAI);
            var destinationWrapper = new RoadAIWrapper(destination.m_netAI);

            NetInfo result;

            if (source == sourceWrapper.bridge || source.name.ToLowerInvariant().Contains("bridge"))
            {
                result = destinationWrapper.bridge;
            }
            else if (source == sourceWrapper.elevated || source.name.ToLowerInvariant().Contains("elevated"))
            {
                result = destinationWrapper.elevated;
            }
            else if (source == sourceWrapper.slope || source.name.ToLowerInvariant().Contains("slope"))
            {
                result  = destinationWrapper.slope;
                isSlope = true;
            }
            else if (source == sourceWrapper.tunnel || source.name.ToLowerInvariant().Contains("tunnel"))
            {
                result = destinationWrapper.tunnel;
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
}
