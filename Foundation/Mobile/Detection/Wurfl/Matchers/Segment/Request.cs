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

#region Usings

using System.Threading;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.Segment
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
            _target = Handler.CreateSegments(userAgent);
            _results = new Results();
        }

        internal Request(string userAgent, SegmentHandler handler, AutoResetEvent completeEvent)
            : base(userAgent, handler, completeEvent)
        {
            _target = Handler.CreateSegments(userAgent);
            _results = new Results();
        }

        #endregion
    }
}