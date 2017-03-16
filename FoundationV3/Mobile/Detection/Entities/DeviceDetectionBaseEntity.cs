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

namespace FiftyOne.Foundation.Mobile.Detection.Entities
{
    /// <summary>
    /// This class provides quality of life improvements for entity classes in
    /// the FiftyOne.Foundation.Mobile.Detection.Entities namespace.
    /// For example, returning a <see cref="Entities.DataSet"/> rather than an 
    /// <see cref="IDataSet"/> 
    /// </summary>
    public class DeviceDetectionBaseEntity : BaseEntity
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataSet"></param>
        /// <param name="indexOrOffset"></param>
        public DeviceDetectionBaseEntity(IDataSet dataSet, int indexOrOffset) 
            : base(dataSet, indexOrOffset) { }

        /// <summary>
        /// Get the dataset that this entity belongs to
        /// </summary>
        public new DataSet DataSet
        {
            get { return base.DataSet as DataSet; }
        }
    }
}
