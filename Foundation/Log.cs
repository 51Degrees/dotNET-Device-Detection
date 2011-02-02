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
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

#endregion

#if VER4 
using System.Threading.Tasks;
#endif

namespace FiftyOne
{
    internal abstract class Log
    {
        protected static object _syncQueue = new object();
        protected static object _syncWait = new object();
        private readonly Queue<string> _queue = new Queue<string>();
        private bool _running;

        /// <summary>
        /// Returns the full path to the file to be written to.
        /// </summary>
        protected abstract string LogFile { get; }

        /// <summary>
        /// The main loop for the log file thread.
        /// </summary>
        protected void Run(Object stateInfo)
        {
            while (IsQueueEmpty() == false)
            {
                FileStream stream = null;
                try
                {
                    if (String.IsNullOrEmpty(LogFile) == false)
                    {
                        stream = File.Open(LogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        StreamWriter writer = null;
                        try
                        {
                            writer = new StreamWriter(stream);
                            while (IsQueueEmpty() == false)
                            {
                                lock (_syncQueue)
                                {
                                    string message = _queue.Peek();
                                    if (message != null)
                                    {
                                        writer.WriteLine(message);
                                        _queue.Dequeue();
                                    }
                                }
                            }
                            writer.Flush();
                        }
                        catch
                        {
                            // End the thread and wait until another message
                            // arrives to resume writing.
                            _running = false;
                            return;
                        }
                        finally
                        {
                            if (writer != null)
                            {
                                writer.Close();
                                writer.Dispose();
                            }
                        }
                    }
                }
                catch
                {
                    // End the thread and wait until another message comes
                    // arrives to resume writing.
                    _running = false;
                    return;
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }
                // Sleep for 50ms incase any new messages come in.
                Thread.Sleep(50);
            }
            _running = false;
        }

        /// <summary>
        /// Returns true if the message queue is empty.
        /// </summary>
        /// <returns></returns>
        private bool IsQueueEmpty()
        {
            return (_queue.Count == 0);
        }

        /// <summary>
        /// Makes sure the logging thread is running and then writes the message
        /// to the queue of messages to be serviced.
        /// </summary>
        /// <param name="message">The message to be written to the log file.</param>
        protected internal void Write(string message)
        {
            // If the queue is very large sleep for 10ms and
            // give it time to reduce.
            // This should never happen if debug is disabled.
            if (_queue.Count > 100 && _running)
            {
                lock (_syncWait)
                {
                    if (_queue.Count > 100 && _running)
                    {
                        Thread.Sleep(10);
                    }
                }
            }

            // Add this entry to the queue for writing.
            lock (_syncQueue)
            {
                _queue.Enqueue(message);
            }

            // Check the thread is running.
            if (_running == false)
            {
                lock (_syncQueue)
                {
                    if (_running == false)
                    {
                        _running = true;
#if VER4
                        Task.Factory.StartNew(() => Run(null));
#elif VER2
                        ThreadPool.QueueUserWorkItem(Run);
#endif
                    }
                }
            }
        }
    }
}