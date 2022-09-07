namespace ParallelRoadTool.Extensions;

public static class MetaBoolExtensions
{
    /// <summary>
    ///     Inverts the value of the provided <see cref="SimulationMetaData.MetaBool" />.
    ///     <see cref="SimulationMetaData.MetaBool.Undefined" /> will not be changed.
    /// </summary>
    /// <param name="metaBool"></param>
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
