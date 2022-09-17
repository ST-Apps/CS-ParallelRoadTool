﻿using AlgernonCommons.Patching;
using ICities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ParallelRoadTool.UI;
using AlgernonCommons.Translation;
using AlgernonCommons;
using ColossalFramework.UI;
using AlgernonCommons.Notifications;
using HarmonyLib;
using ParallelRoadTool.Settings;

namespace ParallelRoadTool
{
    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public sealed class Mod : PatcherMod<UIOptionsPanel, PatcherBase>, IUserMod
    {
        #region Fields

        /// <summary>
        /// Minimum minor version that is compatible with the mod.
        /// </summary>
        private const string CompatibleVersion = "1.15";

        /// <summary>
        /// Mod's current base version.
        /// </summary>
        private readonly string Version = BuildConfig.applicationVersion;
        // private const string Version = "3.0.0";

        #endregion

        #region Properties

#if DEBUG
        private const string Branch = "dev";

        /// <summary>
        /// Gets the mod's base display name (name only).
        /// For DEBUG builds we also include the current branch name.
        /// </summary>
        public override string BaseName => $"[BETA] Parallel Road Tool {Version}-{Branch}";
#else
        /// <summary>
        /// Gets the mod's base display name (name only).
        /// </summary>
        public override string BaseName => $"Parallel Road Tool {Version}";
#endif

        /// <summary>
        /// Gets the mod's unique Harmony identifier.
        /// </summary>
        public override string HarmonyID => "it.stapps.cities.parallelroadtool";

        /// <summary>
        /// Gets the mod's description for display in the content manager.
        /// </summary>
        public string Description => Translations.Translate("MOD_DESCRIPTION");

        #endregion

        #region Lifecycle

        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public override void OnEnabled()
        {
            // Disable mod if version isn't compatible.
            if (!BuildConfig.applicationVersion.StartsWith(CompatibleVersion))
            {
                Logging.Error("invalid game version detected!");

                // Display error message.
                // First, check to see if UIView is ready.
                if (UIView.GetAView() != null)
                {
                    // It's ready - attach the hook now.
                    DisplayVersionError();
                }
                else
                {
                    // Otherwise, queue the hook for when the intro's finished loading.
                    LoadingManager.instance.m_introLoaded += DisplayVersionError;
                }

                // Don't do anything else - no options panel hook, no Harmony patching.
                return;
            }

            // All good - continue as normal.
            base.OnEnabled();
        }

        /// <summary>
        /// Saves settings file.
        /// </summary>
        public override void SaveSettings() => XMLSettingsFile.Save();

        /// <summary>
        /// Loads settings file.
        /// </summary>
        public override void LoadSettings() => XMLSettingsFile.Load();

        #endregion

        #region Control

        #region Internals

        /// <summary>
        /// Displays a version incompatibility error.
        /// </summary>
        private static void DisplayVersionError()
        {
            var versionErrorNotification = NotificationBase.ShowNotification<ListNotification>();
            versionErrorNotification.AddParas(Translations.Translate("WRONG_VERSION"), Translations.Translate("SHUT_DOWN"), BuildConfig.applicationVersion);
        }

        #endregion

        #endregion
    }
}
