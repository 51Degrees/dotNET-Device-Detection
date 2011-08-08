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
