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

namespace FiftyOne.Foundation.Mobile.Detection.Matchers.EditDistance
{
    internal class Request : Matchers.Request
    {
        #region Fields

        private readonly Results _results;

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

        #endregion
    }
}