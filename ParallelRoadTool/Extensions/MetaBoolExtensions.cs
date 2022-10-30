// <copyright file="MetaBoolExtensions.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Extensions;

/// <summary>
///     This class provides extension methods for <see cref="SimulationMetaData"/> objects.
/// </summary>
public static class MetaBoolExtensions
{
    /// <summary>
    ///     Inverts the value of the provided <see cref="SimulationMetaData.MetaBool" />.
    ///     <see cref="SimulationMetaData.MetaBool.Undefined" /> will not be changed.
    /// </summary>
    /// <param name="metaBool">The <see cref="SimulationMetaData"/> object that we want to invert.</param>
    internal static void Invert(this ref SimulationMetaData.MetaBool metaBool)
    {
        metaBool = metaBool switch
        {
            SimulationMetaData.MetaBool.False => SimulationMetaData.MetaBool.True,
            SimulationMetaData.MetaBool.True => SimulationMetaData.MetaBool.False,
            _ => SimulationMetaData.MetaBool.Undefined
        };
    }
}
