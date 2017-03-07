/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2015 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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

using System.IO;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Class used to link a property to one or more export maps.
    /// </summary>
    public class Map : DeviceDetectionBaseEntity
    {
        #region Fields

        /// <summary>
        /// The name of the map.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    lock (this)
                    {
                        if (_name == null)
                        {
                            _name = DataSet.Strings[_nameOffset].ToString();
                        }
                    }
                }
                return _name;
            }
        }
        private string _name;
        private readonly int _nameOffset;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of see <see cref="Map"/>.
        /// </summary>
        /// <param name="dataSet">
        /// The <see cref="DataSet"/> being created.
        /// </param>
        /// <param name="index">
        /// Index of the map within the list.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        internal Map(
            DataSet dataSet,
            int index,
            BinaryReader reader)
            : base(dataSet, index)
        {
            _nameOffset = reader.ReadInt32();
        }

        #endregion
       
        #region Methods

        /// <summary>
        /// Initialises the references to profiles. 
        /// Called from the <see cref="Factories.MemoryFactory"/> if 
        /// initialisation is enabled.
        /// </summary>
        internal void Init()
        {
            if (_name == null)
                _name = DataSet.Strings[_nameOffset].ToString();
        }
        
        /// <summary>
        /// Returns the map name.
        /// </summary>
        /// <returns>
        /// Name of this map as a string.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
