/* *********************************************************************
 * The contents of this file are subject to the Mozilla Public License 
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
 * Mobile Experts Limited are Copyright (C) 2009 - 2012. All Rights Reserved.
 * 
 * Contributor(s):
 *     James Rosewell <james@51degrees.mobi>
 * 
 * ********************************************************************* */

#region Usings

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using FiftyOne.Foundation.Mobile.Detection.Matchers;
using System.Collections.Specialized;

#endregion

namespace FiftyOne.Foundation.Mobile.Detection.Handlers
{
    /// <summary>
    /// Base handler class for device detection.
    /// </summary>
    public abstract class Handler : IComparable<Handler>
    {
        #region Constants

        /// <summary>
        /// The default confidence to assign to results from the handler.
        /// </summary>
        private const byte DEFAULT_CONFIDENCE = 5;

        /// <summary>
        /// HTTP headers containing uaprof urls.
        /// </summary>
        private static readonly string[] UAPROF_HEADERS = new[]
                                                              {
                                                                  "profile",
                                                                  "x-wap-profile",
                                                                  "X-Wap-Profile"
                                                              };

        #endregion

        #region Fields

        /// <summary>
        /// A collection of domain names used with uaprof urls.
        /// </summary>
        private readonly List<string> _uaProfDomains = new List<string>();

        /// <summary>
        /// A single collection of all uaprof urls used by devices assigned to this handler.
        /// </summary>
        private readonly SortedDictionary<int, BaseDeviceInfo[]> _uaprofs =
            new SortedDictionary<int, BaseDeviceInfo[]>();

        /// <summary>
        /// A single collection of all devices assigned to this handler, keyed on the hashcode of the useragent.
        /// </summary>
        private readonly SortedDictionary<int, BaseDeviceInfo[]> _devices =
            new SortedDictionary<int, BaseDeviceInfo[]>();

        /// <summary>
        /// The name of the handler for debugging purposes.
        /// </summary>
        private string _name = null;

        /// <summary>
        /// The default device to be used if no match is found.
        /// </summary>
        private string _defaultDeviceId = null;

        /// <summary>
        /// The confidence matches from this handler should be given
        /// compared to other handlers.
        /// </summary>
        private byte _confidence = 0;

        /// <summary>
        /// True if the UA Prof HTTP headers should be checked.
        /// </summary>
        private bool _checkUAProfs = false;

        /// <summary>
        /// A list of regex's that if matched will indicate support for this handler.
        /// </summary>
        private List<HandleRegex> _canHandleRegex = new List<HandleRegex>();

        /// <summary>
        /// A list of regex's that if matched indicate the handler is not supported.
        /// </summary>
        private List<HandleRegex> _cantHandleRegex = new List<HandleRegex>();
        
        /// <summary>
        /// The provider instance the handler is associated with.
        /// </summary>
        private readonly BaseProvider _provider;

        #endregion

        #region Public Properties
        
        /// <summary>
        /// The name of the handler for debugging purposes.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }
        
        /// <summary>
        /// Returns true/false depending on if the UA Profs should be checked.
        /// </summary>
        public bool CheckUAProfs { get { return _checkUAProfs; } }

        /// <summary>
        /// The confidence to assign to results from this handler.
        /// </summary>
        public byte Confidence
        {
            get { return _confidence; }
        }

        /// <summary>
        /// A list of regexs that if matched indicate the handler does support the 
        /// useragent passed to it.
        /// </summary>
        public List<HandleRegex> CanHandleRegex
        {
            get { return _canHandleRegex; }
        }

        /// <summary>
        /// A list of regexs that if matched indicate the handler does not support the 
        /// useragent passed to it.
        /// </summary>
        public List<HandleRegex> CantHandleRegex
        {
            get { return _cantHandleRegex; }
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Returns the provider the handler is associated to.
        /// </summary>
        internal BaseProvider Provider
        {
            get { return _provider; }
        }

        /// <summary>
        /// Returns the list of devices assigned to the handler keyed on the useragent's hashcode.
        /// </summary>
        internal SortedDictionary<int, BaseDeviceInfo[]> UAProfs
        {
            get { return _uaprofs; }
        }

        /// <summary>
        /// Returns the list of devices assigned to the handler keyed on the useragent's hashcode.
        /// </summary>
        internal SortedDictionary<int, BaseDeviceInfo[]> Devices
        {
            get { return _devices; }
        }
        
        #endregion

        #region Constructor

        /// <summary>
        /// Constructs an instance of <see cref="Handler"/>.
        /// </summary>
        /// <param name="provider">Reference to the provider instance the handler will be associated with.</param>
        /// <param name="name">Name of the handler for debugging purposes.</param>
        /// <param name="defaultDeviceId">The default device ID to return if no match is possible.</param>
        /// <param name="confidence">The confidence this handler should be given compared to others.</param>
        /// <param name="checkUAProfs">True if UAProfs should be checked.</param>
        internal Handler(BaseProvider provider, string name, string defaultDeviceId, byte confidence, bool checkUAProfs)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentNullException(name);

            _provider = provider;
            _name = name;
            _defaultDeviceId = defaultDeviceId;
            _confidence = confidence > 0 ? confidence : DEFAULT_CONFIDENCE;
            _checkUAProfs = checkUAProfs;
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// The inheriting classes match method.
        /// </summary>
        /// <param name="userAgent">The useragent to match.</param>
        /// <returns>A result set of matching devices.</returns>
        internal abstract Results Match(string userAgent);

        #endregion

        #region Public Methods

        /// <summary>
        /// Compares this handler to another where the handler is used
        /// as a key in a sorted list.
        /// </summary>
        /// <param name="other">The other handler to compare to this one.</param>
        /// <returns>See string CompareTo method.</returns>
        public int CompareTo(Handler other)
        {
            return Name.CompareTo(other.Name);
        }

        /// <summary>
        /// Returns true or false depending on the handlers ability
        /// to match the user agent provided.
        /// </summary>
        /// <param name="userAgent">The user agent to be tested.</param>
        /// <returns>True if this handler can support the useragent, otherwise false.</returns>
        public virtual bool CanHandle(string userAgent)
        {
            foreach (HandleRegex regex in _cantHandleRegex)
                if (regex.IsMatch(userAgent))
                    return false;

            foreach (HandleRegex regex in _canHandleRegex)
                if (regex.IsMatch(userAgent))
                    return true;

            return false;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Adds a new device to the handler.
        /// </summary>
        /// <param name="device">device being added to the handler.</param>
        internal virtual void Set(BaseDeviceInfo device)
        {
            SetUserAgent(device);
            SetUaProf(device);
        }

        /// <summary>
        /// Returns the device matching the userAgent string if one is available.
        /// </summary>
        /// <param name="userAgent">userAgent being sought.</param>
        /// <returns>null if no device is found. Otherwise the matching device.</returns>
        internal BaseDeviceInfo GetDeviceInfo(string userAgent)
        {
            // Get the devices with the same hashcode as the useragent.
            BaseDeviceInfo[] devices = GetDeviceInfo(_devices, userAgent);
            if (devices != null && devices.Length > 0)
            {
                // If only one device available return this one.
                if (devices.Length == 1)
                    return devices[0];

                // Look at each device for an exact match. Very rare
                // that more than one device will be returned.
                foreach (BaseDeviceInfo device in devices)
                {
                    if (device.UserAgent == userAgent)
                    {
                        return device;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns all the devices that match the UA prof provided.
        /// </summary>
        /// <param name="uaprof">UA prof to search for.</param>
        /// <returns>Results containing all the matching devices.</returns>
        internal Results GetResultsFromUAProf(string uaprof)
        {
            BaseDeviceInfo[] devices = GetDeviceInfo(_uaprofs, uaprof);
            if (devices != null && devices.Length > 0)
            {
                // Add the devices to the list of results and return.
                Results results = new Results();
                results.AddRange(devices);
                return results;
            }
            return null;
        }

        /// <summary>
        /// Checks to see if the handler can support this device.
        /// </summary>
        /// <param name="device">Device to be checked.</param>
        /// <returns>True if the device is supported, other false.</returns>
        protected internal virtual bool CanHandle(BaseDeviceInfo device)
        {
            return CanHandle(device.UserAgent);
        }

        /// <summary>
        /// <para>
        /// First checks if the useragent from the request can be handled by 
        /// this handler.
        /// </para>
        /// <para>
        /// If the useragent can't be handled then the request is checked to 
        /// determine if a uaprof header field is provided. If so we check
        /// the list of uaprof domains assigned to this handler to see if
        /// they share the same domain.
        /// </para>
        /// </summary>
        /// <param name="headers">Collection of http headers.</param>
        /// <returns>True if this handler could be able to match the device otherwise false.</returns>
        internal virtual bool CanHandle(NameValueCollection headers)
        {
            bool canHandle = CanHandle(BaseProvider.GetUserAgent(headers));
            if (_checkUAProfs && canHandle == false && _uaProfDomains.Count > 0)
            {
                Uri url = null;
                foreach (string header in UAPROF_HEADERS)
                {
                    string value = headers[header];
                    if (value != null &&
                        Uri.TryCreate(value, UriKind.Absolute, out url) &&
                        _uaProfDomains.Contains(url.Host))
                    {
                        return true;
                    }
                }
            }
            return canHandle;
        }

        /// <summary>
        /// Performs an exact match using the userAgent string. If no results are found
        /// uses the UA prof header parameters to find a list of devices.
        /// </summary>
        /// <param name="headers">Collection of Http headers associated with the request.</param>
        /// <returns>null if no exact match was found. Otherwise the matching devices.</returns>
        internal virtual Results Match(NameValueCollection headers)
        {
            // Check for an exact match of the user agent string.
            string userAgent = BaseProvider.GetUserAgent(headers);
            BaseDeviceInfo device = GetDeviceInfo(userAgent);
            if (device != null)
                return new Results(device);

            // Check to see if we have a uaprof header parameter that will produce
            // an exact match.
            if (_checkUAProfs && headers != null && headers.Count > 0)
            {
                foreach (string header in UAPROF_HEADERS)
                {
                    string value = headers[header];
                    if (String.IsNullOrEmpty(value) == false)
                    {
                        value = UserAgentProfileUrlParser.CleanUserAgentProfileUrl(value);
                        Results results = GetResultsFromUAProf(value);
                        if (results != null && results.Count > 0)
                        {
                            if (EventLog.IsDebug)
                                EventLog.Debug(String.Format("UAProf matched '{0}' devices to header '{1}'.",
                                                             results.Count, value));
                            return results;
                        }
                    }
                }
            }

            // There isn't a UA Prof match so use the handler specific methods.
            return Match(userAgent);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds the device and it's user agent string to the collection
        /// of user agent strings and devices. The useragent string
        /// hashcode is the key of the collection.
        /// </summary>
        /// <param name="device">A new device to add.</param>
        private void SetUserAgent(BaseDeviceInfo device)
        {
            int hashcode = device.UserAgent.GetHashCode();
            lock (_devices)
            {
                BaseDeviceInfo[] value;
                // Does the hashcode already exist?
                if (_devices.TryGetValue(hashcode, out value))
                {
                    // Does the key already exist?
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (value[i].UserAgent == device.UserAgent)
                        {
                            // Yes. Update with the new device and then exit.
                            value[i] = device;
                            return;
                        }
                    }
                    // No. Expand the array adding the new device.
                    List<BaseDeviceInfo> newList = new List<BaseDeviceInfo>(value);
                    newList.Add(device);
                    _devices[hashcode] = newList.ToArray();
                }
                else
                {
                    // Add the device to the collection.
                    _devices.Add(hashcode, new[] { device });
                }
            }
        }

        /// <summary>
        /// Adds the device to the collection of devices with UA prof information.
        /// If the device already exists the previous one is replaced.
        /// </summary>
        /// <param name="device">Device to be added.</param>
        private void SetUaProf(BaseDeviceInfo device)
        {
            foreach (int index in _provider.UserAgentProfileStringIndexes)
            {
                var list = device.GetPropertyValueStringIndexes(index);
                if (list != null)
                {
                    foreach (int userAgentProfileStringIndex in list)
                    {
                        string value = _provider.Strings.Get(userAgentProfileStringIndex);

                        // Don't process empty values.
                        if (String.IsNullOrEmpty(value)) continue;
                        
                        // If the url is not valid don't continue processing.
                        Uri url = null;
                        if (Uri.TryCreate(value, UriKind.Absolute, out url) == false) continue;

                        // Get the hashcode before locking the list and processing
                        // the device and hashcode.
                        int hashcode = value.GetHashCode();
                        lock (_uaprofs)
                        {
                            ProcessUaProf(device, hashcode);
                        }

                        // Add the domain to the list of domains for the handler.
                        lock (_uaProfDomains)
                        {
                            if (_uaProfDomains.Contains(url.Host) == false)
                                _uaProfDomains.Add(url.Host);
                        }
                    }
                }
            }
        }

        private void ProcessUaProf(BaseDeviceInfo device, int hashcode)
        {
            // Does the hashcode already exist?
            if (_uaprofs.ContainsKey(hashcode))
            {
                // Does the key already exist?
                int index;
                for (index = 0; index < _uaprofs[hashcode].Length; index++)
                {
                    if (_uaprofs[hashcode][index].DeviceId != device.DeviceId) continue;
                    // Yes. Update with the new device and then exit.
                    _uaprofs[hashcode][index] = device;
                    return;
                }
                // No. Expand the array adding the new device.
                List<BaseDeviceInfo> newList = new List<BaseDeviceInfo>(_uaprofs[hashcode]) { device };
                _uaprofs[hashcode] = newList.ToArray();
            }
            else
            {
                // Add the device to the collection.
                _uaprofs.Add(hashcode, new[] { device });
            }
        }

        /// <summary>
        /// Returns the devices that match a specific hashcode.
        /// </summary>
        /// <param name="dictionary">Collection of hashcodes and devices.</param>
        /// <param name="value">Value that's hashcode is being sought.</param>
        /// <returns>Array of devices matching the value.</returns>
        private static BaseDeviceInfo[] GetDeviceInfo(SortedDictionary<int, BaseDeviceInfo[]> dictionary, string value)
        {
            BaseDeviceInfo[] result;
            if (dictionary != null && 
                dictionary.TryGetValue(value.GetHashCode(), out result))
                return result;
            return null;
        }

        #endregion
        

    }
}