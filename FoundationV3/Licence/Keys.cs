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

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using FiftyOne.Foundation.Mobile;
using System.IO;
using System.Web.Hosting;
using System.Linq;

namespace FiftyOne.Foundation.Licence
{
    /// <summary>
    /// Class handles validation of the Licence file through IsValid and IsTrial static
    /// methods.
    /// </summary>
    internal static class Keys
    {
        #region Fields

        /// <summary>
        /// Lock used to load the licence instances.
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// An array of all licences available.
        /// </summary>
        private static Key[] _licences;

        #endregion

        #region Internal Properties

        /// <summary>
        /// Returns the active Licences for the current process.
        /// </summary>
        internal static Key[] ActiveLicences
        {
            get
            {
                if (_licences == null)
                {
                    lock (_lock)
                    {
                        if (_licences == null)
                        {
                            var licences = ReadLicences();
                            if (licences == null)
                            {
                                throw new MobileException(String.Format(
                                    "A valid Licence file could not be found for the product. " +
                                    "Please visit '{0}' and obtain a valid Licence file and place " +
                                    "it in the /bin directory of the application.",
                                    RetailerConstants.RetailerUrl));
                            }
                            _licences = licences;
                        }
                    }
                }
                return _licences;
            }
        }
        
        /// <summary>
        /// Gets an array of keys valid for production data updates.
        /// </summary>
        internal static Key[] ProductionKeys
        {
            get
            {
                if (_productionKeys == null)
                {
                    lock (_lock)
                    {
                        if (_productionKeys == null)
                        {
                            _productionKeys = ActiveLicences.Where(i =>
                                i.Products.Any(p => p.IsValid && p.Type == LicenceTypes.Production)
                                ).Distinct().ToArray();
                        }
                    }
                }
                return _productionKeys;
            }
        }
        private static Key[] _productionKeys;

        /// <summary>
        /// Returns a pipe seperated list of active Licence ids.
        /// </summary>
        internal static string LicenceIds
        {
            get
            {
                return String.Join("|", ValidLicences.Select(i => i.LicenceId));
            }
        }

        /// <summary>
        /// Returns an array of valid Licences.
        /// </summary>
        internal static Key[] ValidLicences
        {
            get
            {
                if (_validLicences == null)
                {
                    lock (_lock)
                    {
                        if (_validLicences == null)
                        {
                            _validLicences = ActiveLicences.Where(i => i.IsValid).ToArray();
                        }
                    }
                }
                return _validLicences;
            }
        }
        private static Key[] _validLicences;

        /// <summary>
        /// Returns an array of trial Licences.
        /// </summary>
        internal static Key[] TrialLicences
        {
            get
            {
                return ActiveLicences.Where(i => i.IsTrial && i.DaysRemaining > 0).ToArray();
            }
        }

        /// <summary>
        /// Returns the number of days remaining on the first trial Licence provided.
        /// </summary>
        internal static int DaysRemaining
        {
            get
            {
                var firstTrialLicence = TrialLicences.FirstOrDefault();
                return firstTrialLicence != null ? firstTrialLicence.DaysRemaining : 0;
            }
        }

        #endregion
        
        #region Private Methods

        /// <summary>
        /// Reads all the *.lic files in the same folder as the assembly and return any Licences
        /// that are valid.
        /// </summary>
        /// <returns>
        /// Returns the Licences from the valid files.
        /// </returns>
        private static Key[] ReadLicences()
        {
            var licences = new List<Key>();
            foreach (string fileName in Directory.GetFiles(
                AppDomain.CurrentDomain.BaseDirectory,
                "*.lic",
                SearchOption.AllDirectories))
            {
                Key current = CreateLicence(fileName);
                if (current != null)
                {
                    EventLog.Info(String.Format("Licence file '{0}' valid.", fileName));
                    licences.Add(current);
                }
            }
            return licences.ToArray();
        }

        /// <summary>
        /// Returns instance of Licence class based on supplied string.
        /// </summary>
        /// <param name="fileName">String representing Licence data.</param>
        /// <returns></returns>
        private static Key CreateLicence(string fileName)
        {
            try
            {
                return new Key(File.ReadAllText(fileName));
            }
            catch (Exception ex)
            {
                EventLog.Warn(new MobileException(
                    String.Format("Exception reading Licence file '{0}'", fileName), ex));
            }
            return null;
        }

        #endregion
    }
}
