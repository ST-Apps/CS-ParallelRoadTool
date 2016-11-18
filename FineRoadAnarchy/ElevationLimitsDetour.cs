using UnityEngine;

using ColossalFramework.Math;

using FineRoadAnarchy.Redirection;

namespace FineRoadAnarchy
{
    [TargetType(typeof(RoadAI))]
    public class RoadAIDetour : RoadAI
    {
        [RedirectMethod]
        public override void GetElevationLimits(out int min, out int max)
        {
            min = -255;
            max = 255;
        }
    }

    [TargetType(typeof(PedestrianPathAI))]
    public class PedestrianPathAIDetour : PedestrianPathAI
    {
        [RedirectMethod]
        public override void GetElevationLimits(out int min, out int max)
        {
            min = -255;
            max = 255;
        }
    }

    [TargetType(typeof(TrainTrackAI))]
    public class TrainTrackAIDetour : TrainTrackAI
    {
        [RedirectMethod]
        public override void GetElevationLimits(out int min, out int max)
        {
            min = -255;
            max = 255;
        }
    }
}
