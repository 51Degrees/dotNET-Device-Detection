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