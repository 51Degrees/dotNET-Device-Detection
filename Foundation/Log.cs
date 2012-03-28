/* *********************************************************************
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
using System.Collections.Generic;
using System.IO;
using System.Threading;

#endregion

#if VER4 

using System.Threading.Tasks;

#endif

#if AZURE

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Text.RegularExpressions;

#endif

namespace FiftyOne
{
    /// <summary>
    /// This is the base class for recording messages in text files. The write operation to
    /// file occurs outside of the current thread ensuring minimal impact on performance.
    /// If the process completes before the thread has finished writing it is possible that
    /// some messages will not be written. This is by design.
    /// </summary>
    /// <remarks>
    /// This class should not be used in developers code.
    /// </remarks>
    public abstract class Log
    {
        #region Fields

        /// <summary>
        /// An internal object used for synchronising access to the message queue.
        /// </summary>
        protected internal static object _syncQueue = new object();

        /// <summary>
        /// An internal object used to synchronising access to the log file.
        /// </summary>
        protected internal static object _syncWait = new object();

        /// <summary>
        /// The message queue to use to record log entries.
        /// </summary>
        private readonly Queue<string> _queue = new Queue<string>();

        /// <summary>
        /// Set to true if the message writing thread is running.
        /// </summary>
        private bool _running;

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Returns the full path to the file to be written to.
        /// </summary>
        protected abstract string LogFile { get; }

        #endregion

        #region Methods

#if AZURE

        /// <summary>
        /// The main loop for the log table service thread.
        /// </summary>
        protected void Run(Object stateInfo)
        {
            // Initialise the Azure table service creating the table if it does not exist.
            var storageAccount = CloudStorageAccount.Parse(Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetConfigurationSettingValue(FiftyOne.Foundation.Mobile.Constants.AZURE_STORAGE_NAME));
            var serviceContext = new TableServiceContext(storageAccount.TableEndpoint.ToString(), storageAccount.Credentials);
            var tableName = Regex.Replace(LogFile, "[^A-Za-z]+", String.Empty);
            storageAccount.CreateCloudTableClient().CreateTableIfNotExist(tableName);

            while (IsQueueEmpty() == false)
            {
                try
                {
                    while (IsQueueEmpty() == false)
                    {
                        lock (_syncQueue)
                        {
                            string message = _queue.Peek();
                            if (message != null)
                            {
                                var entity = new LogMessageEntity(message);
                                serviceContext.AddObject(tableName, entity);
                                _queue.Dequeue();
                            }
                        }
                    }
                    // Commit the new log entries to the database.
                    serviceContext.SaveChanges();
                }
                catch
                {
                    // End the thread and wait until another message comes
                    // arrives to resume writing.
                    _running = false;
                    return;
                }

                // Sleep for 50ms incase any new messages come in.
                Thread.Sleep(50);
            }
            _running = false;
        }

#else

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

#endif

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
#else
                        ThreadPool.QueueUserWorkItem(Run);
#endif
                    }
                }
            }
        }

        #endregion
    }
}