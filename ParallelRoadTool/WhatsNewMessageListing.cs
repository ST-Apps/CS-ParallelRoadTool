// <copyright file="WhatsNewMessageListing.cs" company="ST-Apps (S. Tenuta)">
// Copyright (c) ST-Apps (S. Tenuta). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace ParallelRoadTool;

using System;
using AlgernonCommons.Notifications;

/// <summary>
///     "What's new" update messages.
/// </summary>
internal static class WhatsNewMessageListing
{
    /// <summary>
    ///     Gets the list of versions and associated update message lines (as translation keys).
    /// </summary>
    internal static WhatsNewMessage[] Messages =>
        new WhatsNewMessage[]
        {
            new ()
            {
                Version = new Version("3.0"),
                MessagesAreKeys = true,
                Messages = new[]
                {
                    "PRT_UPD_30_0",
                    "PRT_UPD_30_1",
                    "PRT_UPD_30_2",
                    "PRT_UPD_30_3",
                    "PRT_UPD_30_4",
                    "PRT_UPD_30_5",
                    "PRT_UPD_30_6",
                    "PRT_UPD_30_7",
                    "PRT_UPD_30_8",
                    "PRT_UPD_30_9",
                },
            },
        };
}
