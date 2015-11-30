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

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// Implementation of Component where Http Headers are provided by the data file
    /// and are no longer hardcoded like V3.1.
    /// </summary>
    public class ComponentV32 : Component, IComparable<Component>
    {
        #region Properties

        /// <summary>
        /// List of http headers that should be checked in order to perform
        /// a detection where more headers than User-Agent are available. This
        /// data is used by methods that can Http Header collections.
        /// </summary>
        public override string[] HttpHeaders
        {
            get
            {
                if (_httpHeaders == null)
                {
                    lock (this)
                    {
                        if (_httpHeaders == null)
                        {
                            _httpHeaders = _httpHeaderOffsets.Select(i =>
                                DataSet.Strings[i].ToString()).ToArray();
                        }
                    }
                }
                return _httpHeaders;
            }
        }
        private string[] _httpHeaders;
        private readonly int[] _httpHeaderOffsets;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of see <see cref="Component"/>
        /// </summary>
        /// <remarks>
        /// Reads the string offsets to the Http Headers during the constructor.
        /// </remarks>
        /// <param name="dataSet">
        /// The <see cref="DataSet"/> being created.
        /// </param>
        /// <param name="index">
        /// Index of the component within the list.
        /// </param>
        /// <param name="reader">
        /// Reader connected to the source data structure and positioned to 
        /// start reading.
        /// </param>
        internal ComponentV32(
            DataSet dataSet,
            int index,
            BinaryReader reader)
            : base(dataSet, index, reader)
        {
            _httpHeaderOffsets = new int[reader.ReadUInt16()];
            for(int i = 0; i < _httpHeaderOffsets.Length; i++)
            {
                _httpHeaderOffsets[i] = reader.ReadInt32();
            }
        }

        #endregion
    }
}
