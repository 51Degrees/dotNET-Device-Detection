/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

using FiftyOne.Foundation.Mobile.Detection.Factories;
using FiftyOne.Foundation.Mobile.Detection.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiftyOne.Foundation.Mobile.Detection.Entities.Memory
{
    /// <summary>
    /// A list of properties in memory as a fixed list. Contains an accessor
    /// which can be used to retrieve entries by property name.
    /// </summary>
    public class PropertiesList : MemoryFixedList<Property, DataSet>
    {
        #region Constructors

        /// <summary>
        /// Constructs a new instance of <see cref="PropertiesList"/>
        /// </summary>
        /// <param name="dataSet">
        /// The <see cref="DataSet"/> being created.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        /// <param name="entityFactory">
        /// Used to create new instances of the entity.
        /// </param>
        internal PropertiesList(DataSet dataSet, Reader reader, BaseEntityFactory<Property, DataSet> entityFactory)
            : base(dataSet, reader, entityFactory)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the properties in the list as a dictionary where
        /// the key is the name of the property. Used to rapidly return
        /// this property from the name.
        /// </summary>
        private IDictionary<string, Property> PropertyNameDictionary
        {
            get
            {
                if (_propertyNameDictionary == null)
                {
                    lock (this)
                    {
                        if (_propertyNameDictionary == null)
                        {
                            _propertyNameDictionary = this.ToDictionary(k => k.Name);
                        }
                    }
                }
                return _propertyNameDictionary;
            }
        }
        private IDictionary<string, Property> _propertyNameDictionary;

        #endregion

        #region Accessors

        /// <summary>
        /// Returns the property matching the name provided, or null
        /// if no such property is available.
        /// </summary>
        /// <param name="propertyName">
        /// Property name required.
        /// </param>
        /// <returns>
        /// The property matching the name, otherwise null.
        /// </returns>
        public Property this[string propertyName]
        {
            get
            {
                Property property = null;
                PropertyNameDictionary.TryGetValue(propertyName, out property);
                return property;
            }
        }

        #endregion
    }
}
