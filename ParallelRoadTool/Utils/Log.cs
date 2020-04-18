using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

namespace CSUtil.Commons
{
#if !DEBUG
#if TRACE
#error TRACE is defined outside of a DEBUG build, please remove
#endif
#endif

    public static class Log
    {
        private static readonly object LogLock = new object();

        private static readonly string LogFilename = Path.Combine(Application.dataPath, "PRT.log");

        private static readonly Stopwatch _sw = Stopwatch.StartNew();

        static Log()
        {
            try
            {
                if (File.Exists(LogFilename)) File.Delete(LogFilename);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     Will log only if debug mode
        /// </summary>
        /// <param name="s">The text</param>
        [Conditional("DEBUG")]
        public static void _Debug(string s)
        {
            LogToFile(s, LogLevel.Debug);
        }

        /// <summary>
        ///     Will log only if debug mode, the string is prepared using string.Format
        /// </summary>
        /// <param name="format">The text</param>
        [Conditional("DEBUG")]
        public static void _DebugFormat(string format, params object[] args)
        {
            LogToFile(string.Format(format, args), LogLevel.Debug);
        }

        /// <summary>
        ///     Will log only if debug mode is enabled and the condition is true
        ///     NOTE: If a lambda contains values from `out` and `ref` scope args,
        ///     then you can not use a lambda, instead use `if (cond) { Log._Debug }`
        /// </summary>
        /// <param name="cond">The condition</param>
        /// <param name="s">The function which returns text to log</param>
        // TODO: Add log thread and replace formatted strings with lists to perform late formatting in that thread
        [Conditional("DEBUG")]
        public static void _DebugIf(bool cond, Func<string> s)
        {
            if (cond) LogToFile(s(), LogLevel.Debug);
        }

        [Conditional("TRACE")]
        public static void _Trace(string s)
        {
            LogToFile(s, LogLevel.Trace);
        }

        public static void Info(string s)
        {
            LogToFile(s, LogLevel.Info);
        }

        /// <summary>
        ///     Will log a warning only if debug mode
        /// </summary>
        /// <param name="s">The text</param>
        [Conditional("DEBUG")]
        public static void _DebugOnlyWarning(string s)
        {
            LogToFile(s, LogLevel.Warning);
        }

        /// <summary>
        ///     Log a warning only in debug mode if cond is true
        ///     NOTE: If a lambda contains values from `out` and `ref` scope args,
        ///     then you can not use a lambda, instead use `if (cond) { Log._DebugOnlyWarning }`
        /// </summary>
        /// <param name="cond">The condition</param>
        /// <param name="s">The function which returns text to log</param>
        [Conditional("DEBUG")]
        public static void _DebugOnlyWarningIf(bool cond, Func<string> s)
        {
            if (cond) LogToFile(s(), LogLevel.Warning);
        }

        public static void Warning(string s)
        {
            LogToFile(s, LogLevel.Warning);
        }

        public static void WarningFormat(string format, params object[] args)
        {
            LogToFile(string.Format(format, args), LogLevel.Warning);
        }

        /// <summary>
        ///     Log a warning only if cond is true
        ///     NOTE: If a lambda contains values from `out` and `ref` scope args,
        ///     then you can not use a lambda, instead use `if (cond) { Log.Warning }`
        /// </summary>
        /// <param name="cond">The condition</param>
        /// <param name="s">The function which returns text to log</param>
        public static void WarningIf(bool cond, Func<string> s)
        {
            if (cond) LogToFile(s(), LogLevel.Warning);
        }

        public static void Error(string s)
        {
            LogToFile(s, LogLevel.Error);
        }

        public static void Exception(Exception e)
        {
            LogToFile($"Exception detected, message is: {e.Message}", LogLevel.Exception);
            LogToFile(e.StackTrace, LogLevel.Exception);
        }

        public static void ErrorFormat(string format, params object[] args)
        {
            LogToFile(string.Format(format, args), LogLevel.Error);
        }

        /// <summary>
        ///     Log error only in debug mode
        /// </summary>
        /// <param name="s">The text</param>
        [Conditional("DEBUG")]
        public static void _DebugOnlyError(string s)
        {
            LogToFile(s, LogLevel.Error);
        }

        /// <summary>
        ///     Writes an Error message about something not implemented. Debug only.
        /// </summary>
        /// <param name="what">The hint about what is not implemented</param>
        [Conditional("DEBUG")]
        public static void NotImpl(string what)
        {
            LogToFile("Not implemented: " + what, LogLevel.Error);
        }

        private static void LogToFile(string log, LogLevel level)
        {
            try
            {
                Monitor.Enter(LogLock);

                using (var w = File.AppendText(LogFilename))
                {
                    var secs = _sw.ElapsedTicks / Stopwatch.Frequency;
                    var fraction = _sw.ElapsedTicks % Stopwatch.Frequency;
                    w.WriteLine(
                        $"[{level}] " +
                        $"{secs:n0}.{fraction:D7}: " +
                        $"{log}");

                    if (level == LogLevel.Warning || level == LogLevel.Error)
                    {
                        w.WriteLine(new StackTrace().ToString());
                        w.WriteLine();
                    }
                }
            }
            finally
            {
                Monitor.Exit(LogLock);
            }
        }

        private enum LogLevel
        {
            Trace,
            Debug,
            Info,
            Warning,
            Error,
            Exception
        }
    }
}