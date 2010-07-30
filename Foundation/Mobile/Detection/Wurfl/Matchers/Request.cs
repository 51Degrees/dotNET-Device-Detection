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

using System.Collections.Generic;
using System.Threading;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers;

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers
{
    internal abstract class Request
    {
        #region Fields

        protected Queue<DeviceInfo> _queue;
        protected int _inProcess = 0;
        protected string _userAgent;
        protected AutoResetEvent _completeEvent;
        private FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers.Handler _handler = null;

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

        internal DeviceInfo Next()
        {
            DeviceInfo device = null;
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
        /// <param name="handler">handler containing devices</param>
        /// <returns>queue of userAgent strings</returns>
        private static Queue<DeviceInfo> CreateQueue(Handler handler)
        {
            Queue<DeviceInfo> queue = new Queue<DeviceInfo>(handler.UserAgents.Count);
            lock (handler.UserAgents)
            {
                foreach (DeviceInfo[] devices in handler.UserAgents)
                {
                    foreach (DeviceInfo device in devices)
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
