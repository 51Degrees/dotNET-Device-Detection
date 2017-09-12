/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright © 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
 * Caversham, Reading, Berkshire, United Kingdom RG4 7BY
 * 
 * This Source Code Form is the subject of the following patents and patent 
 * applications, owned by 51Degrees Mobile Experts Limited of 5 Charlotte
 * Close, Caversham, Reading, Berkshire, United Kingdom RG4 7BY: 
 * European Patent No. 2871816;
 * European Patent Application No. 17184134.9;
 * United States Patent Nos. 9,332,086 and 9,350,823; and
 * United States Patent Application No. 15/686,066.
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
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Threading;
using FiftyOne.Foundation.Mobile.Configuration;
using System.Data.Services.Client;
using System.Text.RegularExpressions;

namespace FiftyOne.Foundation.Mobile.Redirection.Azure
{
    internal class RequestHistory : IRequestHistory
    {
        // Stores the name of the table.
        private readonly string _tableName;

        /// <summary>
        /// The Azure storage account.
        /// </summary>
        CloudStorageAccount _storageAccount = null;

        /// <summary>
        /// Context for accessing the Azure table.
        /// </summary>
        TableServiceContext _serviceContext = null;

        // The next time this process should service the sync file.
        private DateTime _nextServiceTime = DateTime.MinValue;

        // True if the recording of device details is enabled.
        private readonly bool _enabled;

        /// <summary>
        /// The number of minutes that should elapse before the record of 
        /// previous access for the device should be removed from all
        /// possible storage mechanisims.
        /// </summary>
        private readonly int _redirectTimeout = 0;

        internal RequestHistory()
        {
            // Get the timeout used to remove devices.
            _redirectTimeout = Manager.Redirect.Timeout;

            // Get the table name.
            _tableName = Regex.Replace(Manager.Redirect.DevicesFile, "[^A-Za-z]+", String.Empty);

            // Determine if the functionality should be enabled.
            _enabled = String.IsNullOrEmpty(_tableName) == false;

            if (_enabled)
            {
                // Initialise the Azure table service creating the table if it does not exist.
                _storageAccount = CloudStorageAccount.Parse(Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetConfigurationSettingValue(Mobile.Constants.AZURE_STORAGE_NAME));
                _serviceContext = new TableServiceContext(_storageAccount.TableEndpoint.ToString(), _storageAccount.Credentials);
                _serviceContext.IgnoreResourceNotFoundException = false;
                _storageAccount.CreateCloudTableClient().CreateTableIfNotExist(_tableName);
            }
        }

        #region Private Methods

        /// <summary>
        /// Returns the entity if present from the table service. Should only be called after
        /// checking the entity is not already present in the context.
        /// </summary>
        /// <param name="record">Request record of the entity being sought.</param>
        /// <returns>The entity if present in the table service.</returns>
        private RequestEntity GetEntityFromTable(RequestRecord record)
        {
            // Create query to return all matching entities for this record.
            CloudTableQuery<RequestEntity> query = (from entity
                           in _serviceContext.CreateQuery<RequestEntity>(_tableName)
                           where entity.PartitionKey == record.PartitionKey && entity.RowKey == record.RowKey
                           select entity).AsTableServiceQuery<RequestEntity>();
            // Return the first or default entity found.
            try
            {
                return query.Execute().FirstOrDefault();
            }
            catch (DataServiceQueryException ex)
            {
                 //If an exception occurs checked for a 404 error code meaning the resource was not found.
                if (ex.Response.StatusCode == 404)
                    return null;
                throw ex;
            }
        }

        /// <summary>
        /// Returns the entity if present from the current context.
        /// </summary>
        /// <param name="record">Request record of the entity being sought.</param>
        /// <returns>The entity if present in the context.</returns>
        private RequestEntity GetEntityFromContext(RequestRecord record)
        {
            var descripter = _serviceContext.Entities.FirstOrDefault(
                    i => 
                    i.State != EntityStates.Deleted &&
                    ((RequestEntity)i.Entity).PartitionKey == record.PartitionKey &&
                    ((RequestEntity)i.Entity).RowKey == record.RowKey);
            if (descripter != null)
                return descripter.Entity as RequestEntity;
            return null;
        }

        /// <summary>
        /// Removes the request device details from the table, and then removes
        /// from memory once the table has been updated.
        /// </summary>
        /// <param name="record">Request record of the entity being removed.</param>
        private void Remove(RequestRecord record)
        {
            lock (_serviceContext)
            {
                // Get the entity matching the record held in the context.
                RequestEntity entity = GetEntityFromContext(record);
                if (entity == null)
                    // Get the entity from the table if it exists.
                    entity = GetEntityFromTable(record);

                // If the entity has been found remove it from the context and the table.
                if (entity != null)
                {
                    _serviceContext.DeleteObject(entity);
                    _serviceContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Adds the record to the table storage service, or if it already exists
        /// updates the last active date and time.
        /// </summary>
        /// <param name="record">Request record of the entity being added or updated.</param>
        private void Set(RequestRecord record)
        {
            lock (_serviceContext)
            {
                // Get the entity if it exists.
                RequestEntity entity = GetEntityFromContext(record);
                if (entity == null)
                    entity = GetEntityFromTable(record);

                if (entity == null)
                {
                    // Add the new entity to the table storage.
                    _serviceContext.AddObject(
                        _tableName,
                        new RequestEntity(record));
                }
                else
                {
                    // Update the last active time.
                    entity.LastActiveDate = record.LastActiveDateAsDateTime;
                    _serviceContext.UpdateObject(entity);
                }

                // Commit the changes back to the table service.
                _serviceContext.SaveChanges();
            }

            CheckIfServiceRequired();
        }
        
        /// <summary>
        /// If the last time the devices file was serviced to remove old entries
        /// is older than 1 minute start a thread to service the devices file and 
        /// remove old entries. If the redirect timeout is 0 indicating infinite
        /// then nothing should be purged.
        /// </summary>
        private void CheckIfServiceRequired()
        {
            if (_nextServiceTime < DateTime.UtcNow)
            {
                // Service the request history storage.
                ThreadPool.QueueUserWorkItem(
                    ServiceRequestHistory,
                    DateTime.UtcNow.AddMinutes(-_redirectTimeout));

                // Set the next time to service the sync file using a random offset to 
                // attempt to avoid conflicts with other processes.
                _nextServiceTime = DateTime.UtcNow.AddMinutes(1).AddSeconds(new Random().Next(30));
            }
        }

        /// <summary>
        /// Removes old entries that are held within this context from the table store using a single
        /// save changes operation. This means the service happens using one HTTP request which saves
        /// a lot of money with Azure. Unfortunately it does mean that there is a possibility request
        /// data not known to this instance will not be removed.
        /// </summary>
        /// <param name="value">
        /// Date as a DateTime used to determine if a request history 
        /// record is old and can be removed.
        /// </param>
        private void ServiceRequestHistory(object value)
        {
            // Remove the old records from the table.
            DateTime purgeDate = (DateTime)value;

            lock (_serviceContext)
            {
                // Get all entities in the context that are older than the purgeDate.
                var entities = from i in _serviceContext.Entities 
                               where ((RequestEntity)i.Entity).LastActiveDate < purgeDate &&
                                     i.State == EntityStates.Unchanged
                               select i;

                if (entities != null && entities.Count() > 0)
                {
                    // Delete all the entities found.
                    foreach (var entity in entities)
                        _serviceContext.DeleteObject(entity.Entity);
                    _serviceContext.SaveChanges();
                }
            }
        }

        #endregion
        
        #region IRequestHistory Members
        
        /// <summary>
        /// Determines if the device making the request has been seen by the web site previously.
        /// </summary>
        /// <param name="request">The request from the device.</param>
        /// <returns>True if the device has been seen before, otherwise false.</returns>
        public bool IsPresent(HttpRequest request)
        {
            if (_enabled == false)
                return false;

            RequestRecord record = new RequestRecord(request);

            lock (_serviceContext)
            {
                // Get the entity if it exists.
                RequestEntity entity = GetEntityFromContext(record);
                if (entity == null)
                    entity = GetEntityFromTable(record);

                // If the entity isn't null and the last active time is within the timeout period.
                return entity != null &&
                    entity.LastActiveDate.AddMinutes(_redirectTimeout) >= record.LastActiveDateAsDateTime;
            }
        }

        /// <summary>
        /// Sets the last active time for the device making the request.
        /// </summary>
        /// <param name="request">The request from the device.</param>
        public void Set(HttpRequest request)
        {
            if (_enabled == false)
                return;

            RequestRecord record = new RequestRecord(request);
            Set(record);
        }

        /// <summary>
        /// Removes the device behind the request.
        /// </summary>
        /// <param name="request">The request from the device.</param>
        public void Remove(HttpRequest request)
        {
            if (_enabled == false)
                return;

            RequestRecord record = new RequestRecord(request);
            Remove(record);
        }

        #endregion
    }
}

#endif