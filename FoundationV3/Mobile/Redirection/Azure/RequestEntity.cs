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

#if AZURE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;


namespace FiftyOne.Foundation.Mobile.Redirection.Azure
{
    /// <summary>
    /// Used internally to represent a requesting device.
    /// </summary>
    public class RequestEntity : TableServiceEntity
    {
        /// <summary>
        /// Constructs an instance of RequestDataModel without any
        /// data defined.
        /// </summary>
        public RequestEntity() { }

        /// <summary>
        /// Constructs an instance of RequestDataModel using the unique
        /// request ID of the device as the primary key, and the last 
        /// date the device related to the request was active.
        /// </summary>
        internal RequestEntity(RequestRecord record) :
            base(record.PartitionKey, record.RowKey)
        {
            LastActiveDate = record.LastActiveDateAsDateTime;
        }

        /// <summary>
        /// The last time the device was active in the web application.
        /// </summary>
        internal DateTime LastActiveDate { get; set; }
    }
}

#endif
