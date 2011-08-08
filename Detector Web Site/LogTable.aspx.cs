using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
#if AZURE
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure.ServiceRuntime;
using FiftyOne;
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