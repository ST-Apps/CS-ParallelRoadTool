namespace ParallelRoadTool.Extensions;

public static class MetaBoolExtensions
{
    internal static void Invert(this ref SimulationMetaData.MetaBool metaBool)
    {
        metaBool = metaBool switch
        {
            SimulationMetaData.MetaBool.False => SimulationMetaData.MetaBool.True,
            SimulationMetaData.MetaBool.True  => SimulationMetaData.MetaBool.False,
            _                                 => SimulationMetaData.MetaBool.Undefined
        };
    }
}
