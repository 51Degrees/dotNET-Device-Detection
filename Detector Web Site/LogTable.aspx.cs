/* *********************************************************************
 * This Source Code Form is copyright of 51Degrees Mobile Experts Limited. 
 * Copyright 2017 51Degrees Mobile Experts Limited, 5 Charlotte Close,
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
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 * ********************************************************************* */

using System;
#if AZURE
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.ServiceRuntime;
#endif

namespace Detector
{
    public partial class Table : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Load_Data();
        }

        protected void Load_Data()
        {
#if AZURE
            //Access the storage account
            var storageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("fiftyonedegrees"));
            //Create the service context to access the table
            var serviceContext = new TableServiceContext(storageAccount.TableEndpoint.ToString(), storageAccount.Credentials);
            
            //Getting the table entries
            foreach (var row in serviceContext.CreateQuery<LogMessageEntity>("log"))    //"log" - the name of the table you wish to see
            {
                OutBox.Text += row.Message;
            }
#else
            OutBox.Text = "This page will only work when compiled for use with Windows Azure";
#endif
        }

        protected void DelAllBut_Click(object sender, EventArgs e)
        {
#if AZURE
            var storageAccount = CloudStorageAccount.Parse(Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironment.GetConfigurationSettingValue("fiftyonedegrees"));
            var serviceContext = new TableServiceContext(storageAccount.TableEndpoint.ToString(), storageAccount.Credentials);
            storageAccount.CreateCloudTableClient().CreateTableIfNotExist("log");

            foreach (var row in serviceContext.CreateQuery<LogMessageEntity>("log"))
            {
                serviceContext.DeleteObject(row);
            }
            serviceContext.SaveChanges();

            Page.Response.Redirect(Page.Request.Url.ToString(), true);
#endif
        }
    }
}