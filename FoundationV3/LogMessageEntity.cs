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

#if AZURE

using Microsoft.WindowsAzure.StorageClient;
using System;

namespace FiftyOne
{
    /// <summary>
    /// Class used to encapsulate a message to be written to the log table service.
    /// </summary>
    public class LogMessageEntity : TableServiceEntity
    {
        /// <summary>
        /// Creates a new instance of LogMessageEntity.
        /// </summary>
        public LogMessageEntity() { }

        /// <summary>
        /// Creates a new instance of LogMessageEntity initialised
        /// with the message provided.
        /// </summary>
        /// <param name="message">The message to be associated with the entity.</param>
        public LogMessageEntity(string message) :
            base(DateTime.UtcNow.Hour.ToString(), Guid.NewGuid().ToString())
        {
            Message = message;
        }

        /// <summary>
        /// The log message.
        /// </summary>
        public string Message { get; set; }
    }
}

#endif