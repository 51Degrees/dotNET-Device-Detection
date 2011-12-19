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

using FiftyOne.Foundation.Mobile.Detection.Handlers;
namespace FiftyOne.Foundation.Mobile.Detection.Matchers.Segment
{
    internal class Segment
    {
        private bool _isValid;
        private uint _score;
        private int _weight;
        private string _value;

        internal string Value
        {
            get { return _value; }
        }

        internal int Weight
        {
            get { return _weight; }
        }

        internal Segment(string value, int weight)
        {
            _value = value;
            _weight = weight;
        }

        internal uint Score
        {
            get { return _score; }
            set
            {
                _score = value;
                _isValid = true;
            }
        }

        internal bool IsValid
        {
            get { return _isValid; }
        }
    }
}