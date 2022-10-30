// <copyright file="XMLNetItem.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool.Models;

using System.Xml.Serialization;

/// <summary>
///     Wrapper for all the network's properties that we want to save in a preset.
/// </summary>
[XmlRoot(ElementName = "NetItem")]
public class XMLNetItem
{
    /// <summary>
    ///     Gets or sets the horizontal offset relative to the main network.
    /// </summary>
    [XmlElement]
    public float HorizontalOffset { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the direction is reversed.
    /// </summary>
    [XmlElement]
    public bool IsReversed { get; set; }

    /// <summary>
    ///     Gets or sets the name of the network.
    ///     This is in-game's basic name, not the display one.
    /// </summary>
    [XmlElement]
    public string Name { get; set; }

    /// <summary>
    ///     Gets or sets the  vertical offset relative to the main network.
    /// </summary>
    [XmlElement]
    public float VerticalOffset { get; set; }
}
