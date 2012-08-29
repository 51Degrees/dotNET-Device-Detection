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

using System.Collections.Generic;
using System.Threading;
using FiftyOne.Foundation.Mobile.Detection.Handlers;
using System;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Matchers
{
    internal abstract class Request
    {
        #region Classes

#if VER4
        /// <summary>
        /// A simple countdown singalling class derived for .NET 4.
        /// Used to co-ordinate processing across multiple threads.
        /// </summary>
        internal class CountdownEvent : System.Threading.CountdownEvent 
        {
            private static readonly int _count = Environment.ProcessorCount;
            internal CountdownEvent() : base(_count) { }
        }
#else
        /// <summary>
        /// A simple countdown singalling class.
        /// Used to co-ordinate processing across multiple threads.
        /// </summary>
        internal class CountdownEvent
        {
            private static readonly int _count = Environment.ProcessorCount;
            private int _remain;
            private EventWaitHandle _event;

            internal CountdownEvent()
            {
                _remain = _count;
                _event = new ManualResetEvent(false);
            }

            internal int InitialCount
            {
                get { return _count; }
            }

            internal void Signal()
            {
                // The last thread to signal also sets the event.
                if (Interlocked.Decrement(ref _remain) == 0)
                    _event.Set();
            }

            /// <summary>
            /// Blocks the current thread until the timeout has passed or
            /// the object is signalled.
            /// </summary>
            /// <param name="milliSeconds"></param>
            /// <returns></returns>
            internal bool Wait(int milliSeconds)
            {
                return _event.WaitOne(milliSeconds);
            }
        }
#endif

        #endregion

        #region Fields

        private readonly Handler _handler;
        protected readonly CountdownEvent _completeEvent;
        protected readonly Queue<BaseDeviceInfo> _queue;
        protected readonly string _userAgent;

        #endregion

        #region Properties

        internal string UserAgent
        {
            get { return _userAgent; }
        }

        internal virtual Handler Handler
        {
            get { return _handler; }
        }

        internal int Count
        {
            get { return _queue.Count; }
        }

        /// <summary>
        /// Returns the number of threads 
        /// </summary>
        internal int ThreadCount
        {
            get { return _completeEvent.InitialCount; }
        }

        #endregion

        #region Constructors

        internal Request(string userAgent, Handler handler)
        {
            _userAgent = userAgent;
            _queue = CreateQueue(handler);
            _handler = handler;
            _completeEvent = new CountdownEvent();
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Waits until the time has elapsed, or the process has signaled complete.
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        internal bool Wait(int millisecondsTimeout)
        {
            return _completeEvent.Wait(millisecondsTimeout);
        }

        /// <summary>
        /// Returns the next device in the queue to be checked.
        /// </summary>
        /// <returns></returns>
        internal BaseDeviceInfo Next()
        {
            lock (_queue)
                if (_queue.Count > 0)
                    return _queue.Dequeue();
            return null;
        }

        /// <summary>
        /// Tells the waiting main thread that this worker thread has finished.
        /// </summary>
        internal void Complete()
        {
            _completeEvent.Signal();
        }

        /// <summary>
        /// Takes a handler and returns a queue containing the userAgent strings. Ensures a
        /// lock is obtained on the handler before creating the queue to avoid another thread
        /// accessing or changing content at the same time.
        /// </summary>
        /// <param name="handler">Handler containing devices.</param>
        /// <returns>A queue of devices.</returns>
        private static Queue<BaseDeviceInfo> CreateQueue(Handler handler)
        {
            Queue<BaseDeviceInfo> queue = new Queue<BaseDeviceInfo>(handler.Devices.Count);
            lock (handler.Devices)
            {
                foreach (BaseDeviceInfo[] devices in handler.Devices.Values)
                {
                    foreach (BaseDeviceInfo device in devices)
                    {
                        queue.Enqueue(device);
                    }
                }
            }
            return queue;
        }

        #endregion
    }
}