using UnityEngine;

using ParallelRoadTool.Redirection;

namespace ParallelRoadTool.Detours
{
    [TargetType(typeof(RoadAI))]
    public class RoadAIDetour : NetAI
    {
        [RedirectMethod]
        public override void GetElevationLimits(out int min, out int max)
        {
            min = int.MinValue / 256;
            max = int.MaxValue / 256;
        }

        [RedirectMethod]
        public override ToolBase.ToolErrors CheckBuildPosition(bool test, bool visualize, bool overlay, bool autofix, ref NetTool.ControlPoint startPoint, ref NetTool.ControlPoint middlePoint, ref NetTool.ControlPoint endPoint, out BuildingInfo ownerBuilding, out Vector3 ownerPosition, out Vector3 ownerDirection, out int productionRate)
        {
            base.CheckBuildPosition(test, visualize, overlay, autofix, ref startPoint, ref middlePoint, ref endPoint, out ownerBuilding, out ownerPosition, out ownerDirection, out productionRate);
            return ToolBase.ToolErrors.None;
        }
    }

    [TargetType(typeof(PedestrianPathAI))]
    public class PedestrianPathAIDetour : NetAI
    {
        [RedirectMethod]
        public override void GetElevationLimits(out int min, out int max)
        {
            min = int.MinValue / 256;
            max = int.MaxValue / 256;
        }

        [RedirectMethod]
        public override ToolBase.ToolErrors CheckBuildPosition(bool test, bool visualize, bool overlay, bool autofix, ref NetTool.ControlPoint startPoint, ref NetTool.ControlPoint middlePoint, ref NetTool.ControlPoint endPoint, out BuildingInfo ownerBuilding, out Vector3 ownerPosition, out Vector3 ownerDirection, out int productionRate)
        {
            base.CheckBuildPosition(test, visualize, overlay, autofix, ref startPoint, ref middlePoint, ref endPoint, out ownerBuilding, out ownerPosition, out ownerDirection, out productionRate);
            return ToolBase.ToolErrors.None;
        }
    }

    [TargetType(typeof(TrainTrackAI))]
    public class TrainTrackAIDetour : NetAI
    {
        [RedirectMethod]
        public override void GetElevationLimits(out int min, out int max)
        {
            min = int.MinValue / 256;
            max = int.MaxValue / 256;
        }

        [RedirectMethod]
        public override ToolBase.ToolErrors CheckBuildPosition(bool test, bool visualize, bool overlay, bool autofix, ref NetTool.ControlPoint startPoint, ref NetTool.ControlPoint middlePoint, ref NetTool.ControlPoint endPoint, out BuildingInfo ownerBuilding, out Vector3 ownerPosition, out Vector3 ownerDirection, out int productionRate)
        {
            base.CheckBuildPosition(test, visualize, overlay, autofix, ref startPoint, ref middlePoint, ref endPoint, out ownerBuilding, out ownerPosition, out ownerDirection, out productionRate);
            return ToolBase.ToolErrors.None;
        }
    }
}
