/* *********************************************************************
 * The contents of this file are subject to the Mozilla Public License 
 * Version 1.1 (the "License"); you may not use this file except in 
 * compliance with the License. You may obtain a copy of the License at 
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS" 
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 * See the License for the specific language governing rights and 
 * limitations under the License.
 *
 * The Original Code is named .NET Mobile API, first released under 
 * this licence on 11th March 2009.
 * 
 * The Initial Developer of the Original Code is owned by 
 * 51 Degrees Mobile Experts Limited. Portions created by 51 Degrees 
 * Mobile Experts Limited are Copyright (C) 2009 - 2010. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

using System.IO;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Web;
using System.Diagnostics;
using FiftyOne.Foundation.Mobile.Configuration;
using System.Security.Permissions;
using System.Security;

namespace FiftyOne
{
    internal class EventLog : Log
    {
        /// <summary>
        /// An instance of the event log.
        /// </summary>
        private static EventLog _instance = null;

        /// <summary>
        /// The process id for the application.
        /// </summary>
        private static int _processId = -1;

        /// <summary>
        /// Set to minus 1 to ensure value loaded correctly from web.config
        /// file when the application starts.
        /// </summary>
        private static int _logLevel = -1;

        /// <summary>
        /// The file name for the log file.
        /// </summary>
        private string _logFile = null;

        /// <summary>
        /// Used to lock during the creation of the log level parameter and instance.
        /// </summary>
        private static object _sync = new object();

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
                            if (Manager.Log != null && Manager.Log.Enabled == true)
                            {
                                _logFile = FiftyOne.Foundation.Mobile.Configuration.Support.GetFilePath(Manager.Log.LogFile);
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
                        if (_logLevel == -1 && Manager.Log != null && Manager.Log.Enabled == true)
                        {
                            switch (Manager.Log.LogLevel)
                            {
                                case "Debug": _logLevel = 4; break;
                                case "Info": _logLevel = 3; break;
                                case "Warn": _logLevel = 2; break;
                                case "Fatal": _logLevel = 1; break;
                                default: _logLevel = 0; break;
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

        internal static void Debug(Exception ex)
        {
            if (IsDebug == true)
            {
                Write("Debug", ex.Message);
                Write("Debug", ex.StackTrace);
                if (ex.InnerException != null)
                    Debug(ex.InnerException);
            }
        }

        internal static void Info(Exception ex)
        {
            if (IsInfo == true)
            {
                Write("Info", ex.Message);
                Write("Info", ex.StackTrace);
                if (ex.InnerException != null)
                    Info(ex.InnerException);
            }
        }

        internal static void Warn(Exception ex)
        {
            if (IsWarn == true)
            {
                Write("Warn", ex.Message);
                Write("Warn", ex.StackTrace);
                if (ex.InnerException != null)
                    Warn(ex.InnerException);
            }
        }

        internal static void Fatal(Exception ex)
        {
            if (IsFatal == true)
            {
                Write("Fatal", ex.Message);
                Write("Fatal", ex.StackTrace);
                if (ex.InnerException != null)
                    Fatal(ex.InnerException);
            }
        }

        internal static void Debug(string message)
        {
            if (IsDebug == true)
                Write("Debug", message);
        }

        internal static void Info(string message)
        {
            if (IsInfo == true)
                Write("Info", message);
        }

        internal static void Warn(string message)
        {
            if (IsWarn == true)
                Write("Warn", message);
        }

        internal static void Fatal(string message)
        {
            if (IsFatal == true)
                Write("Fatal", message);
        }

        private static int GetProcessId()
        {
            return Process.GetCurrentProcess().Id;
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

        internal protected static void Write(string level, string message)
        {
            Instance.Write(String.Format("{0:s} - {1} - {2} - {3}",
                    DateTime.UtcNow,
                    ProcessId,
                    level,
                    message));
        }
    }
}