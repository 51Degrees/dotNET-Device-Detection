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

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Matchers.EditDistance
{
    internal class Request : Matchers.Request
    {
        #region Fields

        private Results _results = null;

        #endregion

        #region Properties

        internal Results Results
        {
            get { return _results; }
        }

        #endregion

        #region Constructors

        internal Request(string userAgent, Handler handler) :
            base(userAgent, handler)
        {
            _results = new Results();
        }

        internal Request(string userAgent, Handler handler, AutoResetEvent completeEvent) :
            base(userAgent, handler, completeEvent) 
        {
            _results = new Results();
        }

        #endregion
    }
}
