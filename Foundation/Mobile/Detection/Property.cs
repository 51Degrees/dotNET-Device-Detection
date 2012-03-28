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

using System.Collections.Generic;

#if VER4

using System.Linq;

#endif

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Describes a property which can be assigned to a device.
    /// </summary>
    public class Property : PropertyValue
    {
        #region Fields

        private List<Value> _values = new List<Value>();
        private bool _isList = false;
        private bool _isMandatory = false;
        private bool _showValues = false;

        #endregion

        #region Constructors

        internal Property(Provider provider, string name) : base(provider, name) { }

        internal Property(Provider provider, string name, string description) : base(provider, name, description) { }

        internal Property(Provider provider, string name, string description, string url) : base(provider, name, description, url) { }

        internal Property(Provider provider, string name, string description, string url, bool isMandatory, bool isList, bool showValues)
            : base(provider, name, description, url) 
        {
            _isMandatory = isMandatory;
            _isList = isList;
            _showValues = showValues;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if the property is only available in the Premium
        /// data set.
        /// </summary>
        public bool IsPremium
        {
            get 
            {
#if VER4
                return Provider.EmbeddedProvider.Properties.FirstOrDefault(i =>
                    i.Name == Name) == null; 
#else
                foreach (var property in Provider.EmbeddedProvider.Properties)
                    if (property.Name == Name)
                        return false;
                return true;
#endif
            }
        }

        /// <summary>
        /// Returns a list of possible values the property can have.
        /// </summary>
        public List<Value> Values
        {
            get { return _values; }
        }

        /// <summary>
        /// Returns true if the property is mandatory and should be
        /// returned for every device.
        /// </summary>
        public bool IsMandatory
        {
            get { return _isMandatory; }
        }

        /// <summary>
        /// Returns true if the property is a list type and multiple
        /// values will be returned.
        /// </summary>
        public bool IsList
        {
            get { return _isList; }
        }

        /// <summary>
        /// Returns true if the values associated with the property are suitable
        /// to be displayed. Will return false if they're numeric and do not generally
        /// present well when shown as a list.
        /// </summary>
        public bool ShowValues
        {
            get { return _showValues; }
        }

        #endregion
    }
}