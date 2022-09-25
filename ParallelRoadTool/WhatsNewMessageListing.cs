using System;
using AlgernonCommons.Notifications;

namespace ParallelRoadTool
{
    /// <summary>
    ///     "What's new" update messages.
    /// </summary>
    internal static class WhatsNewMessageListing
    {
        #region Properties

        /// <summary>
        ///     Gets the list of versions and associated update message lines (as translation keys).
        /// </summary>
        internal static WhatsNewMessage[] Messages => new WhatsNewMessage[]
        {
            new()
            {
                Version         = new Version("3.0"),
                MessagesAreKeys = true,
                Messages = new[]
                {
                    "PRT_UPD_30_0",
                    "PRT_UPD_30_1"
                }
            }
        };

        #endregion
    }
}
