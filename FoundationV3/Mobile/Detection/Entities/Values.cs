/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2014 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent Application No. 13192291.6; and
 * United States Patent Application Nos. 14/085,223 and 14/085,301.
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0.
 * 
 * If a copy of the MPL was not distributed with this file, You can obtain
 * one at http://mozilla.org/MPL/2.0/.
 * 
 * This Source Code Form is “Incompatible With Secondary Licenses”, as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Encapsulates a list of one or more values. Provides methods
    /// to return boolean, double and string representations of the
    /// values list.
    /// </summary>
    /// <para>
    /// The class contains helper methods to make consuming the data set easier.
    /// </para>
    /// <para>
    /// For more information see http://51degrees.com/Support/Documentation/Net.aspx
    /// </para>
    public class Values : List<Value>
    {
        #region Fields

        /// <summary>
        /// The property the list of values relates to.
        /// </summary>
        private readonly Property _property;

        #endregion

        #region Properties

        /// <summary>
        /// Returns true if any of the values in this list are the
        /// default ones for the property.
        /// </summary>
        public bool IsDefault
        {
            get
            {
                return this.Any(i => i.IsDefault);
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of the values list.
        /// </summary>
        /// <param name="property">Property the values list relates to</param>
        /// <param name="values">IEnumerable of values to use to initialise the list</param>
        internal Values(Property property, IEnumerable<Value> values)
            : base (values)
        {
            _property = property;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The value represented as a boolean.
        /// </summary>
        /// <returns>A boolean representation of the only item in the list.</returns>
        /// <exception cref="MobileException">
        /// Thrown if the method is called for a property with multiple values
        /// </exception>
        public bool ToBool()
        {
            if (_property.IsList)
                throw new MobileException("ToBool can only be used on non List properties");
            return this[0].ToBool();
        }

        /// <summary>
        /// The value represented as a double.
        /// </summary>
        /// <returns>A double representation of the only item in the list.</returns>
        /// <exception cref="MobileException">
        /// Thrown if the method is called for a property with multiple values
        /// </exception>
        public double ToDouble()
        {
            if (_property.IsList)
                throw new MobileException("ToDouble can only be used on non List properties");
            return this[0].ToDouble();
        }

        /// <summary>
        /// Returns the values as a string array.
        /// </summary>
        /// <returns></returns>
        public string[] ToStringArray()
        {
            return this.Select(i => i.Name).ToArray();
        }

        /// <summary>
        /// The values represented as a string where multiple values are seperated 
        /// by colons.
        /// </summary>
        /// <returns>The values as a string</returns>
        public override string ToString()
        {
            return String.Join(
                Constants.ValueSeperator,
                this.Select(i => i.ToString()).ToArray());
        }

        #endregion
    }
}
