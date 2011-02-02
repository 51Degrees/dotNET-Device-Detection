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

using System.Text.RegularExpressions;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Wurfl.Handlers
{
    internal abstract class AlphaNumericHandler : CatchAllHandler
    {
        // This is among the least precise handler.
        private const int CONFIDENCE = 2;

        private readonly Regex _pattern;

        internal AlphaNumericHandler()
        {
            _pattern = new Regex(Expression, RegexOptions.Compiled);
        }

        internal override byte Confidence
        {
            get { return CONFIDENCE; }
        }

        protected abstract string Expression { get; }

        protected internal override bool CanHandle(string userAgent)
        {
            return _pattern.IsMatch(userAgent);
        }
    }

    internal class NumericHandler : AlphaNumericHandler
    {
        // This is among the least precise handler.
        private const int CONFIDENCE = 2;

        internal override byte Confidence
        {
            get { return CONFIDENCE; }
        }

        protected override string Expression
        {
            get { return @"^\d"; }
        }
    }

    internal class AlphaHandlerAtoF : AlphaNumericHandler
    {
        // This is among the least precise handler.
        private const int CONFIDENCE = 2;

        internal override byte Confidence
        {
            get { return CONFIDENCE; }
        }

        protected override string Expression
        {
            get { return @"(?i)^[A-F]"; }
        }
    }

    internal class AlphaHandlerGtoN : AlphaNumericHandler
    {
        // This is among the least precise handler.
        private const int CONFIDENCE = 2;

        internal override byte Confidence
        {
            get { return CONFIDENCE; }
        }

        protected override string Expression
        {
            get { return @"(?i)^[G-N]"; }
        }
    }

    internal class AlphaHandlerOtoS : AlphaNumericHandler
    {
        // This is among the least precise handler.
        private const int CONFIDENCE = 2;

        internal override byte Confidence
        {
            get { return CONFIDENCE; }
        }

        protected override string Expression
        {
            get { return @"(?i)^[O-S]"; }
        }
    }

    internal class AlphaHandlerTtoZ : AlphaNumericHandler
    {
        // This is among the least precise handler.
        private const int CONFIDENCE = 2;

        internal override byte Confidence
        {
            get { return CONFIDENCE; }
        }

        protected override string Expression
        {
            get { return @"(?i)^[T-Z]"; }
        }
    }
}