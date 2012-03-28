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

using System.Threading;
using FiftyOne.Foundation.Mobile.Detection.Handlers;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Matchers.Segment
{
    internal class Request : Matchers.Request
    {
        #region Fields

        private readonly Results _results;
        private readonly Segments _target;

        #endregion

        #region Properties

        internal Segments Target
        {
            get { return _target; }
        }

        internal new SegmentHandler Handler
        {
            get { return (SegmentHandler) base.Handler; }
        }

        internal Results Results
        {
            get { return _results; }
        }

        #endregion

        #region Constructors

        internal Request(string userAgent, SegmentHandler handler)
            : base(userAgent, handler)
        {
            _target = Handler.CreateAllSegments(userAgent);
            _results = new Results();
        }

        internal Request(string userAgent, SegmentHandler handler, AutoResetEvent completeEvent)
            : base(userAgent, handler, completeEvent)
        {
            _target = Handler.CreateAllSegments(userAgent);
            _results = new Results();
        }

        #endregion
    }
}