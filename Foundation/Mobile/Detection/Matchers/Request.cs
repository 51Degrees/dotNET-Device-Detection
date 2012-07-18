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

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Matchers
{
    internal abstract class Request
    {
        #region Fields

        private readonly Handler _handler;
        protected AutoResetEvent _completeEvent;
        protected int _inProcess;
        protected Queue<BaseDeviceInfo> _queue;
        protected string _userAgent;

        #endregion

        #region Properties

        internal string UserAgent
        {
            get { return _userAgent; }
        }

        internal int InProcess
        {
            get { return _inProcess; }
        }

        internal virtual Handler Handler
        {
            get { return _handler; }
        }

        internal int Count
        {
            get { return _queue.Count; }
        }

        #endregion

        #region Constructors

        internal Request(string userAgent, Handler handler)
        {
            _userAgent = userAgent;
            _queue = CreateQueue(handler);
            _handler = handler;
        }

        internal Request(string userAgent, Handler handler, AutoResetEvent completeEvent)
        {
            _userAgent = userAgent;
            _queue = CreateQueue(handler);
            _handler = handler;
            _completeEvent = completeEvent;
        }

        #endregion

        #region Methods

        internal BaseDeviceInfo Next()
        {
            BaseDeviceInfo device = null;
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    // Increase the counter indicating the number
                    // of devices currently being processed.
                    _inProcess++;
                    // Take the next device from the queue and provide
                    // it to the calling function.
                    device = _queue.Dequeue();
                }
            }
            return device;
        }

        internal void Complete()
        {
            lock (_queue)
            {
                // Decrease the counter of activie devices being processed.
                _inProcess--;
                // If no more devices are being processed, the queue is
                // empty, and there is a waiting thread signal the waiting 
                // thread to continue processing.
                if (_inProcess == 0 && _completeEvent != null && _queue.Count == 0)
                    _completeEvent.Set();
            }
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