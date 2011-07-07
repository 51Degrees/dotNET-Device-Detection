/* *********************************************************************
 * The contents of this file are subject to the Mozilla internal License 
 * Version 1.1 (the "License"); you may not use this file except in 
 * compliance with the License. You may obtain a copy of the License at 
 * http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS IS" 
 * basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 * See the License for the specific language governing rights and 
 * limitations under the License.
 *
 * The Original Code is named .NET Mobile API, first released under 
 * this licence on 11th March 2009.
 * 
 * The Initial Developer of the Original Code is owned by 
 * 51 Degrees Mobile Experts Limited. Portions created by 51 Degrees 
 * Mobile Experts Limited are Copyright (C) 2009 - 2011. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System.Collections;
using System.Collections.Generic;
using System.Web;
using FiftyOne.Foundation.Mobile.Detection.Wurfl;
using FiftyOne.Foundation.Mobile.Detection.Wurfl.Configuration;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection
{
    /// <summary>
    /// Used to create the Capabilities collection based on the input like user agent string.
    /// </summary>
    internal static class Factory
    {
        #region Fields

        /// <summary>
        /// Cache for mobile capabilities. Items are removed approx. 60 minutes after the last
        /// time they were used.
        /// </summary>
        private static readonly Cache<IDictionary> _cache = new Cache<IDictionary>(60);

        /// <summary>
        /// Lock used when 
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Used to obtain the mobile capabilities for the request or user agent string
        /// from the device data source provided.
        /// </summary>
        private static MobileCapabilities _mobileCapabilities;

        /// <summary>
        /// Class used to record new devices.
        /// </summary>
        private static NewDevice _newDevice = new NewDevice(Manager.NewDevicesURL, Manager.NewDeviceDetail);

        #endregion

        #region Private Properties

        /// <summary>
        /// Returns a single instance of the MobileCapabilities class used to provide
        /// capabilities to enhance the request.
        /// </summary>
        private static MobileCapabilities MobileCapabilities
        {
            get
            {
                if (_mobileCapabilities == null)
                {
                    lock (_lock)
                    {
                        if (_mobileCapabilities == null)
                        {
                            if (Manager.Enabled)
                            {
                                List<string> wurflFiles = new List<string>();
                                wurflFiles.Add(Manager.WurflFilePath);
                                wurflFiles.AddRange(Manager.WurflPatchFiles);

                                _mobileCapabilities = new Wurfl.MobileCapabilities(
                                    new Provider(wurflFiles.ToArray(), Manager.CapabilitiesWhiteList, Manager.UseActualDeviceRoot));
                            }
                        }
                    }
                }
                return _mobileCapabilities;
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Creates a new <see cref="MobileCapabilities"/> class based on the useragent
        /// string provided.
        /// </summary>
        /// <param name="userAgent">The useragent for the device.</param>
        /// <returns></returns>
        internal static IDictionary Create(string userAgent)
        {
            IDictionary caps;

            // We can't do anything with empty user agent strings.
            if (userAgent == null)
                return null;
            
            if (_cache.GetTryParse(userAgent, out caps))
            {
                // Return these capabilities for adding to the existing ones.
                return caps;
            }

            // Create the new mobile capabilities and record the collection of
            // capabilities for quick creation in future requests.
            caps = MobileCapabilities.Create(userAgent);
            _cache[userAgent] = caps;
            return caps;
        }

        /// <summary>
        /// Creates a new <see cref="MobileCapabilities"/> class based on the context
        /// of the requesting device.
        /// </summary>
        /// <param name="request">HttpRequest from the device.</param>
        /// <param name="currentCapabilities">Capabilities already determined by other sources.</param>
        /// <returns>A new mobile capabilities</returns>
        internal static IDictionary Create(HttpRequest request, IDictionary currentCapabilities)
        {
            IDictionary caps;

            // We can't do anything with empty user agent strings.
            if (request.UserAgent == null)
                return null;

            if (_cache.GetTryParse(request.UserAgent, out caps))
            {
                // Return these capabilities for adding to the existing ones.
                return caps;
            }

            // Create the new mobile capabilities and record the collection of
            // capabilities for quick creation in future requests.
            caps = MobileCapabilities.Create(request, currentCapabilities);
            _cache[request.UserAgent] = caps;

            // Send the header details to 51Degrees.mobi.
            RecordNewDevice(request);

            return caps;            
        }

        /// <summary>
        /// Send the header details to 51Degrees.mobi if the configuration is enabled.
        /// </summary>
        /// <param name="request">Details about the request that was used to create the new device.</param>
        private static void RecordNewDevice(HttpRequest request)
        {
            if (_newDevice.Enabled)
                _newDevice.RecordNewDevice(request);
        }

        #endregion
    }
}