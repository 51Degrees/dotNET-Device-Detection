/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent Application No. 13192291.6; and
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

#region Usings

using System;
using System.Diagnostics;
using System.Security;
using FiftyOne.Foundation.Mobile.Configuration;

#endregion

namespace FiftyOne
{
    /// <summary>
    /// Utility class, contains implementation for logging support.
    /// </summary>
    /// <remarks>
    /// This class should not be called as it is part of the internal logic.
    /// </remarks>
    public class EventLog : Log
    {
        /// <summary>
        /// Used to lock during the creation of the log level parameter and instance.
        /// </summary>
        private static readonly object _sync = new object();

        /// <summary>
        /// An instance of the event log.
        /// </summary>
        private static EventLog _instance;

        /// <summary>
        /// Set to minus 1 to ensure value loaded correctly from web.config
        /// file when the application starts.
        /// </summary>
        private static int _logLevel = -1;

        /// <summary>
        /// The process id for the application.
        /// </summary>
        private static int _processId = -1;

        /// <summary>
        /// The file name for the log file.
        /// </summary>
        private string _logFile;

        /// <summary>
        /// An internal reference to the current static instance of the class.
        /// </summary>
        protected internal static EventLog Instance
        {
            get
            {
                if (_instance == null)
                    lock (_sync)
                        if (_instance == null)
                            _instance = new EventLog();
                return _instance;
            }
        }

        /// <summary>
        /// If a log file section has been added to the web.config
        /// then return the log file path specified.
        /// </summary>
        protected override string LogFile
        {
            get
            {
                if (_logFile == null)
                {
                    lock (_sync)
                    {
                        if (_logFile == null)
                        {
                            if (Manager.Log != null && Manager.Log.Enabled)
                            {
                                _logFile = Support.GetFilePath(Manager.Log.LogFile);
                            }
                        }
                    }
                }
                return _logFile;
            }
        }

        private static int LogLevel
        {
            get
            {
                if (_logLevel == -1)
                {
                    lock (_sync)
                    {
                        if (_logLevel == -1 && Manager.Log != null && Manager.Log.Enabled)
                        {
                            switch (Manager.Log.LogLevel)
                            {
                                case "Debug":
                                    _logLevel = 4;
                                    break;
                                case "Info":
                                    _logLevel = 3;
                                    break;
                                case "Warn":
                                    _logLevel = 2;
                                    break;
                                case "Fatal":
                                    _logLevel = 1;
                                    break;
                                default:
                                    _logLevel = 0;
                                    break;
                            }
                        }
                    }
                }
                // Not specified so return 0.
                return _logLevel;
            }
        }

        internal static bool IsDebug
        {
            get { return LogLevel >= 4; }
        }

        internal static bool IsInfo
        {
            get { return LogLevel >= 3; }
        }

        internal static bool IsWarn
        {
            get { return LogLevel >= 2; }
        }

        internal static bool IsFatal
        {
            get { return LogLevel >= 1; }
        }

        private static int ProcessId
        {
            get
            {
                if (_processId == -1)
                {
                    lock (_sync)
                    {
                        if (_processId == -1)
                        {
                            try
                            {
                                _processId = GetProcessId();
                            }
                            catch (SecurityException)
                            {
                                _processId = 0;
                            }
                        }
                    }
                }
                return _processId;
            }
        }

        /// <summary>
        /// Records a debug exception in the log file if debug logging is enabled.
        /// </summary>
        /// <param name="ex">The exception to be recorded in the log file.</param>
        public static void Debug(Exception ex)
        {
            if (IsDebug)
            {
                Write("Debug", ex.Message);
                Write("Debug", ex.StackTrace);
                if (ex.InnerException != null)
                    Debug(ex.InnerException);
            }
        }

        /// <summary>
        /// Records an info exception in the log file if info logging is enabled.
        /// </summary>
        /// <param name="ex">The exception to be recorded in the log file.</param>
        public static void Info(Exception ex)
        {
            if (IsInfo)
            {
                Write("Info", ex.Message);
                Write("Info", ex.StackTrace);
                if (ex.InnerException != null)
                    Info(ex.InnerException);
            }
        }
        
        /// <summary>
        /// Records a warn exception in the log file if warn logging is enabled.
        /// </summary>
        /// <param name="ex">The exception to be recorded in the log file.</param>
        public static void Warn(Exception ex)
        {
            if (IsWarn)
            {
                Write("Warn", ex.Message);
                Write("Warn", ex.StackTrace);
                if (ex.InnerException != null)
                    Warn(ex.InnerException);
            }
        }

        /// <summary>
        /// Records a fatal exception in the log file if fatal logging is enabled.
        /// </summary>
        /// <param name="ex">The exception to be recorded in the log file.</param>
        public static void Fatal(Exception ex)
        {
            if (IsFatal)
            {
                Write("Fatal", ex.Message);
                Write("Fatal", ex.StackTrace);
                if (ex.InnerException != null)
                    Fatal(ex.InnerException);
            }
        }

        /// <summary>
        /// Records the message in the log file if debug logging is enabled.
        /// </summary>
        /// <param name="message">The text message to record in the log file.</param>
        /// <param name="args">Objects to put in the message. Follows the syntax of String.Format.</param>
        public static void Debug(string message, params object[] args)
        {
            if (IsDebug)
                Write("Debug", String.Format(message, args));
        }

        /// <summary>
        /// Records the message in the log file if info logging is enabled.
        /// </summary>
        /// <param name="message">The text message to record in the log file.</param>
        /// <param name="args">Objects to put in the message. Follows the syntax of String.Format.</param>
        public static void Info(string message, params object[] args)
        {
            if (IsInfo)
                Write("Info", String.Format(message, args));
        }

        /// <summary>
        /// Records the message in the log file if warn logging is enabled.
        /// </summary>
        /// <param name="message">The text message to record in the log file.</param>
        /// <param name="args">Objects to put in the message. Follows the syntax of String.Format.</param>
        public static void Warn(string message, params object[] args)
        {
            if (IsWarn)
                Write("Warn", String.Format(message, args));
        }

        /// <summary>
        /// Records the message in the log file if fatal logging is enabled.
        /// </summary>
        /// <param name="message">The text message to record in the log file.</param>
        /// <param name="args">Objects to put in the message. Follows the syntax of String.Format.</param>
        public static void Fatal(string message, params object[] args)
        {
            if (IsFatal)
                Write("Fatal", String.Format(message, args));
        }

        [SecuritySafeCriticalAttribute]
        private static int GetProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        /// <summary>
        /// Writes the level and message to the log file including the current time and process id.
        /// </summary>
        /// <param name="level">The level of the message. Values include debug, info, warn and fatal.</param>
        /// <param name="message">The message string to be written.</param>
        protected internal static void Write(string level, string message)
        {
            Instance.Write(String.Format("{0:o} - {1} - {2} - {3}",
                                         DateTime.UtcNow,
                                         ProcessId,
                                         level,
                                         message));
        }
    }
}